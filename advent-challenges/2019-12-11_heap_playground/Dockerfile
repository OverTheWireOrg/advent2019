FROM ubuntu

RUN apt-get update && apt-get -y upgrade
RUN apt-get -y --no-install-recommends install openbsd-inetd
RUN mkdir -p /opt/heap_playground
COPY ./heap_playground /opt/heap_playground
COPY ./flag /opt/heap_playground
COPY ./inetd.heap_playground /etc/inetd.conf


WORKDIR /opt/heap_playground

EXPOSE 1337

CMD /usr/sbin/inetd -d
