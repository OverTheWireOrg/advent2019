#!/bin/bash

here=$(dirname $(readlink -f $0))

sudo iptables -t mangle -D POSTROUTING --src 66.66.66.0/24 -j DROP
sudo iptables -t mangle -I POSTROUTING --src 66.66.66.0/24 -j DROP

pushd $here
docker-compose $*
popd

sudo iptables -t nat -D POSTROUTING --src 66.66.66.0/24 -j RETURN
sudo iptables -t nat -I POSTROUTING --src 66.66.66.0/24 -j RETURN
sudo iptables -t mangle -D POSTROUTING --src 66.66.66.0/24 -j DROP
