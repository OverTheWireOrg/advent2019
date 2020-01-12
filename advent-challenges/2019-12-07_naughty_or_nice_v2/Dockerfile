FROM ubuntu:latest

ENV DEBIAN_FRONTEND noninteractive

#update
RUN apt-get update --fix-missing
RUN apt-get -y upgrade

#system deps
RUN apt-get install -y libgmp10 xinetd

#create ctf-user
RUN groupadd -r ctf && useradd -r -g ctf ctf

#add chall items
ADD ctf.xinetd /etc/xinetd.d/ctf
ADD chall_init.sh /etc/chall_init.sh
ADD flag /home/ctf/flag
ADD naughty_or_nice /home/ctf/naughty_or_nice
ADD redir.sh /home/ctf/redir.sh

#set some proper permissions
RUN chown -R root:ctf /home/ctf
RUN chmod 750 /home/ctf/naughty_or_nice
RUN chmod 750 /home/ctf/redir.sh
RUN chmod 440 /home/ctf/flag
RUN chmod 700 /etc/chall_init.sh

RUN service xinetd restart

### IF NETWORK-DEBUG
#RUN apt-get -y install netcat
#RUN apt-get -y install net-tools
### IF NETWORK-DEBUG
