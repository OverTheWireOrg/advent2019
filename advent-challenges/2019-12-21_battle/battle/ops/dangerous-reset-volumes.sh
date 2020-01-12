set -e
set -x

rm -rf volumes/*
mkdir volumes/binaries volumes/results volumes/db
chmod 700 volumes/binaries
chmod 700 volumes/results
chown 8888:8888 volumes/binaries
chown 8888:8888 volumes/results