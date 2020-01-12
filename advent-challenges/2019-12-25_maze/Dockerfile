FROM ubuntu:latest
ENV DEBIAN_FRONTEND noninteractive

RUN apt-get update && apt-get install -y apt-transport-https
RUN apt-get install -y xinetd
#RUN apt-get install -y python3-pip
#RUN pip3 install gif-for-cli
#RUN apt-get install -y ffmpeg

RUN groupadd -r ctf && useradd -r -g ctf ctf
ADD ctf.xinetd /etc/xinetd.d/ctf
ADD ./files/redir.sh /home/ctf/redir.sh
ADD ./files/maze /home/ctf/maze
RUN mkdir /home/ctf/maps/
ADD ./files/maps/* /home/ctf/maps/


ADD ./files/flag /home/ctf/flag
ADD ./files/flag.gif /home/ctf/flag.gif
ADD ./files/flag.txt /home/ctf/flag.txt
ADD ./files/pre-calc.sh /home/ctf/pre-calc.sh
RUN chmod 755 /home/ctf/pre-calc.sh
#RUN /home/ctf/pre-calc.sh

RUN chmod 755 /home/ctf/flag.txt
RUN chown -R root:ctf /home/ctf
RUN chmod 750 /home/ctf/redir.sh
RUN chmod 750 /home/ctf/maze
RUN chmod 755 /home/ctf/maps/
RUN chmod 644 /home/ctf/maps/*

RUN service xinetd restart
CMD ["/usr/sbin/xinetd", "-dontfork"]
