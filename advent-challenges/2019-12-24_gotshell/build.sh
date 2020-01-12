#!/bin/bash
g++ main.cpp -o got_shell -std=c++11 -O3 -lboost_system -lboost_thread -lpthread --static
pygmentize -O full -o source.html main.cpp