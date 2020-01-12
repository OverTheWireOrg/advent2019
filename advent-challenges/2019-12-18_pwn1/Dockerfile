FROM ubuntu:18.04

RUN apt-get -y update && apt-get -y install -y --no-install-recommends xinetd gcc-multilib g++-multilib

RUN mkdir /home/ctf
ADD flag.txt /home/ctf
ADD chal /home/ctf
ADD redir.sh /home/ctf
RUN useradd -u 8888 -m ctf

RUN chmod 440 /home/ctf/*
RUN chown -R root:ctf /home/ctf
RUN chmod 750 /home/ctf/redir.sh
RUN chmod 750 /home/ctf/chal

CMD ["/usr/sbin/xinetd", "-dontfork"]

