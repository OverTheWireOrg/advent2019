FROM ubuntu:18.04

RUN apt-get -y update && apt-get -y install -y --no-install-recommends python2.7 xinetd gcc-7 libc6-dev

RUN mkdir /home/ctf
ADD flag.txt /home/ctf
ADD chal.py /home/ctf
ADD elf.c /home/ctf
RUN cd /home/ctf && gcc-7 -o elf -O1 elf.c && rm elf.c
RUN useradd -u 8888 -m ctf
RUN chmod -x /tmp
CMD ["/usr/sbin/xinetd", "-dontfork"]

