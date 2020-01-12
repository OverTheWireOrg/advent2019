#!/bin/sh

for rows in `seq 25 50`;
do
	cols=$((rows * 32 / 10))
	echo "Generating cols $cols rows $rows"
	gif-for-cli --no-display --cols $cols --rows $rows flag.gif
done

