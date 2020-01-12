FROM ubuntu:latest

#env-vars we need to run smoothly
ENV DEBIAN_FRONTEND noninteractive

#update
RUN apt-get update && apt-get -y upgrade
#system deps
RUN apt-get install -y nginx
# copy vhost config
COPY vhost.conf /etc/nginx/sites-available/default
# copy entry script
COPY entry.sh /etc/entry.sh

RUN mkdir -p /var/www/static
COPY static/style.css /var/www/static/style.css

