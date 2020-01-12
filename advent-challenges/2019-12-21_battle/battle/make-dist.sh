#!/bin/sh
mkdir -p dist
rm -rf dist/*
cp example/skeleton.cc dist
cp example/dist-Makefile dist/Makefile
cp simulator/*.cs dist
cp simulator/*.csproj dist
zip -r dist.zip dist common example-replay-file
mv dist.zip $(sha256sum dist.zip | awk '{print $1}')-dist.zip
