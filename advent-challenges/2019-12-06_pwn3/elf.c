#include <stdio.h>
#include <unistd.h>
#include <stdlib.h>

int main() {
  char buf[128] = {};
  puts("Hello there, what is your name?");
  for (int i = 0; i < 127; i++) {
    read(0, buf + i, 1);
    if (buf[i] == '\n') {
      buf[i] = 0;
      break;
    }
  }
  printf("Greetings %s, let me sing you a song:\n", buf);
  puts("We wish you a Merry Chhistmas\n"
       "We wish you a Merry Christmxs\n"
       "We wish you alMerry Christmas\n"
       "and a HapZy New Year!");
  exit(0);
}
