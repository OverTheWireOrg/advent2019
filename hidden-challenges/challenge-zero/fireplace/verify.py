#!/usr/bin/env python3

import binascii
from glob import glob
import io
import os
import os.path
import re
import sys
import uu


def die(fstring, *args):
    print(fstring.format(*args), file=sys.stderr)
    sys.exit(1)


if len(sys.argv) != 3:
    die('usage: {} <dir> <mbr>', sys.argv[0])

_, DIR, MBR = sys.argv


ansi = b''
for path in sorted(glob(os.path.join(DIR, '*.ansi'))):
    ansi += open(path, 'rb').read()

ansi_regex = rb'\x1b\[(2J|H|(\d+;?)+m)'
clean = re.sub(ansi_regex, b'', ansi)
clean = re.sub(b'[#\n]', b'', clean)
b64s = re.findall(b'[^=]+=+', clean)
if b64s[0] != b64s[1]:
    die('b64 mismatch')
u = binascii.a2b_base64(b64s[0])
out = io.BytesIO()
uu.decode(io.BytesIO(u), out)
out.seek(0)
binary = out.read()
mbr = open(MBR, 'rb').read()
if binary != mbr:
    die('binary mismatch')
print('works as designed')
