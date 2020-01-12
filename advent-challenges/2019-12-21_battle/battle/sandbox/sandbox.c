#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <seccomp.h>
#include <string.h>
#include <errno.h> 
#include <signal.h>
#include <sys/mman.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/resource.h>
#include <sys/wait.h>
#include <fcntl.h>

#define WRITE               1
#define READ                0
#define DEFAULT_MEM_LIMIT   120 * (1024*1024)
#define DEFAULT_CPU_LIMIT   2

char *map;
int   mem = DEFAULT_MEM_LIMIT;
int   cpu = DEFAULT_CPU_LIMIT;

void setup_limits(){

    struct rlimit limit;

    limit.rlim_cur = cpu;
    limit.rlim_max = cpu;
    if (setrlimit(RLIMIT_CPU, &limit) != 0){
        fprintf(stderr, "setrlimit(RLIMIT_CPU): %s\n", strerror(errno));
        exit(EXIT_FAILURE);
    }

    limit.rlim_cur = mem;
    limit.rlim_max = mem;
    if (setrlimit(RLIMIT_AS, &limit) != 0){
        fprintf(stderr, "setrlimit(RLIMIT_AS): %s\n", strerror(errno));
        exit(EXIT_FAILURE);
    }
}

int setup_seccomp(char *bin){

    void *addr;
    int   fd;

    fd = open("/dev/urandom", O_RDONLY);
    read(fd, &addr, sizeof(addr));
    close(fd);

    map = mmap(addr, 0x1000, PROT_READ|PROT_WRITE, MAP_PRIVATE|MAP_ANONYMOUS, -1, 0);
    memcpy(map, bin, strlen(bin));

    scmp_filter_ctx ctx;
    ctx = seccomp_init(SCMP_ACT_KILL);

    if (seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(read), 1,
                  SCMP_A0(SCMP_CMP_LT, 6)) != 0){
        fprintf(stderr, "seccomp: read\n");
        exit(EXIT_FAILURE);
    }

    if (seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(write), 1,
                  SCMP_A0(SCMP_CMP_LT, 3),
                  SCMP_A0(SCMP_CMP_EQ, 2)) != 0){
        fprintf(stderr, "seccomp: write\n");
        exit(EXIT_FAILURE);
    }

    if (seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(openat), 1,
                  SCMP_A2(SCMP_CMP_MASKED_EQ, O_RDONLY)) != 0){
        fprintf(stderr, "seccomp: openat\n");
        exit(EXIT_FAILURE);
    }

    if (seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(execve), 3,
                  SCMP_A0(SCMP_CMP_EQ, (scmp_datum_t) map),
                  SCMP_A1(SCMP_CMP_NE, 0),
                  SCMP_A2(SCMP_CMP_NE, 0)) != 0){
        fprintf(stderr, "seccomp: execve\n");
        exit(EXIT_FAILURE);
    }

    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(access), 0);             // 21
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(fstat), 0);              // 5
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(close), 0);              // 3
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(dup2), 0);               // 33
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(exit), 0);               // 60
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(exit_group), 0);         // 231
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(mmap), 0);               // 9
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(mprotect), 0);           // 10
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(munmap), 0);             // 11
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(brk), 0);                // 12
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(alarm), 0);              // 37
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(arch_prctl), 0);         // 158
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(sched_getaffinity), 0);  // 204
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(getrusage), 0);          // 98

    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(rt_sigprocmask), 0);     // 14
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(futex), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(readlinkat), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(clone), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(rt_sigaction), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(gettid), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(sigaltstack), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(fcntl), 0);
    seccomp_rule_add(ctx, SCMP_ACT_ALLOW, SCMP_SYS(uname), 0);

    seccomp_load(ctx);
    return 0;
}

void usage(char *prog){

    fprintf(stderr, "Usage: %s [-cm] [file...]\n", prog);
    exit(EXIT_FAILURE);
}

int main(int argc, char *argv[]){

    char *bin;
    int   opt;
    
    while ((opt = getopt(argc, argv, "c:m:")) != -1) {
        switch (opt) {
        case 'c': cpu = atol(optarg); break;
        case 'm': mem = atol(optarg); break;
        default:
            usage(argv[0]);
        }
    }

    bin = argv[optind];
    if (strlen(bin) == 0){
        usage(argv[0]);
    }

    signal(SIGCHLD, SIG_IGN);

    setup_limits();
    setup_seccomp(bin);

    char *argx[] = { map, NULL };
    char *envx[] = { NULL };
    execve(argx[0], argx, envx);

    exit(EXIT_FAILURE);

    return EXIT_SUCCESS; 
}
