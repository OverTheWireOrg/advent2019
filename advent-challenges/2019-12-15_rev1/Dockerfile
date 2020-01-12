FROM ubuntu:18.04

RUN apt-get -y update && apt-get -y install -y --no-install-recommends xinetd

RUN mkdir /home/ctf
ADD flag.txt /home/ctf
ADD chal /home/ctf
RUN useradd -u 8888 -m ctf
CMD ["/usr/sbin/xinetd", "-dontfork"]

