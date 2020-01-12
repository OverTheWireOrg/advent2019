FROM ubuntu:latest

RUN apt-get update
RUN apt-get install --no-install-recommends -y python2.7
RUN apt-get install --no-install-recommends -y python-pip
RUN pip install flask

RUN groupadd -r ctf && useradd -r -g ctf ctf

ADD static/index.html /home/ctf/static/index.html
ADD flag.txt /home/ctf/flag.txt
ADD server.py /home/ctf/server.py

USER ctf
CMD ["python2.7", "server.py"]
