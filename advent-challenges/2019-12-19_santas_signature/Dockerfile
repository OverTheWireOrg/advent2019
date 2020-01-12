FROM ubuntu

RUN apt-get update && apt-get -y upgrade
RUN apt-get update && apt-get install -y apt-transport-https
RUN apt-get -y --no-install-recommends install openbsd-inetd build-essential libssl-dev libffi-dev python3 python3-pip python3-setuptools python3-dev
RUN pip3 install wheel
RUN pip3 install pycrypto
RUN mkdir -p /opt/santas_signature
COPY ./santas_signature.py /opt/santas_signature
COPY ./flag /opt/santas_signature
COPY ./key /opt/santas_signature
COPY ./inetd.santas_signature /etc/inetd.conf

WORKDIR /opt/santas_signature

EXPOSE 1337

CMD /usr/sbin/inetd -d
