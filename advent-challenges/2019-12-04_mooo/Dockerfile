FROM ubuntu:latest

RUN apt-get update --fix-missing
RUN apt-get -y upgrade

RUN apt-get -y --no-install-recommends install python3 python3-pip cowsay gunicorn3
RUN pip3 install Flask

RUN mkdir -p /opt/mooo
COPY src/server.py /opt/mooo/
COPY src/templates/index /opt/mooo/templates/
COPY src/templates/cow_designer /opt/mooo/templates/
COPY flag /opt/mooo/
WORKDIR /opt/mooo

ENV LC_ALL C.UTF-8
ENV LANG C.UTF-8

USER nobody

CMD gunicorn3 -w 4 -b 0.0.0.0:1299 server:app

EXPOSE 1299

