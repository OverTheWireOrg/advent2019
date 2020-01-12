#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>
#include <signal.h>

void handler(){
	puts("\nToo slow");
	exit(1);
}

int main(void){
	signal(SIGALRM, handler);
	alarm(20);
	setbuf(stdin,0);
	setbuf(stdout,0);
	setbuf(stderr,0);
	puts("Got shell?");
	/* simple captcha for proven interactivity */
	int a, b, fd;
	long c, d;

	char seed[4];

	fd = open("/dev/urandom", 0);
    if(fd == -1) 
        return 1;
    if(read(fd, seed, sizeof seed) != sizeof seed)
        return 1;
    close(fd);

	srand(*(unsigned int *) &seed);
	a = rand();
	b = rand();
	c = (long)a + (long)b;

	fflush(stdin);
	printf("%d + %d = ", a, b);
	scanf("%ld", &d);

	if (c != d){
		puts("Incorrect captcha :(");
		return 1;
	}

	/* print flag */ 
	FILE *fp = fopen("flag", "r");
	if (fp == NULL) {
		puts("No flag file :(");
		return 1;
	}
	int ch;
	while ((ch = fgetc(fp)) != EOF) {
		printf("%c",ch);
	}
	fclose(fp);
	return 0;
}