#!/bin/bash
cd /home/ctf/client
./client 2> /dev/null &
cd /home/ctf/server
./server 2> /dev/null
