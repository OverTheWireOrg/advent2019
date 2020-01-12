FROM ubuntu

RUN apt-get update && apt-get -y upgrade
RUN apt-get -y install python3 python3-gevent python3-bottle

RUN mkdir -p /opt/challenge-zero/frames
COPY app.py /opt/challenge-zero/
COPY flames.gif /opt/challenge-zero/
COPY frames/ /opt/challenge-zero/frames/

WORKDIR /opt/challenge-zero/
USER nobody

ENV HOST=0.0.0.0
CMD /opt/challenge-zero/app.py
