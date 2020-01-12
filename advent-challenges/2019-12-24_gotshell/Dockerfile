FROM ubuntu:disco

ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get update --fix-missing
RUN apt-get -y upgrade

# add new user if it is needed
RUN useradd -p gotshell -s /bin/false gotshell
#RUN adduser --disabled-password --gecos "" gotshell
RUN echo "gotshell:vrwemjzgfdtgzolferfoixtmdrapfhmf" | chpasswd

RUN chmod 733 /tmp

# Change work directory
RUN mkdir -p /opt/got_shell
RUN mkdir -p /opt/got_server
RUN chmod 711 /opt/got_server
WORKDIR  /opt/got_server
COPY got_shell .
RUN chmod 1 got_shell
COPY ghetto .
RUN chmod 1 ghetto

WORKDIR  /opt/got_shell
# Setup the vulnerability environment
COPY source.html .
COPY flag_reader .
RUN chgrp gotshell flag_reader
RUN chmod 2011 flag_reader
COPY flag .
RUN chgrp gotshell flag
RUN chmod 040 flag

#EXPOSE 1266
# Change user
USER nobody

ENTRYPOINT ["/opt/got_server/ghetto"]