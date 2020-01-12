#!/usr/bin/env python3

from glob import glob
import os.path
import sys


WHITELIST = b'X'


def die(fstring, *args):
    print(fstring.format(*args), file=sys.stderr)
    sys.exit(1)


if len(sys.argv) != 3:
    die('usage: {} <dir> <mbr.bin>', sys.argv[0])

_, DIR, MBR = sys.argv


def goodchars(path):
    content = open(path, 'rb').read()
    count = 0
    for char in content:
        if char in WHITELIST:
            count += 1
    return count


def embed(inpath, outpath, payload):
    content = open(inpath, 'rb').read()
    i = 0
    with open(outpath, 'wb') as f:
        for char in content:
            if char in WHITELIST and i < len(payload):
                i += f.write(bytes([payload[i]]))
            elif char == ord('?'):
                f.write(b'#')
            else:
                f.write(bytes([char]))


def main():
    paths = sorted(glob(os.path.join(DIR, '*.txt')))
    payload = open(MBR, 'rb').read()
    offset = 0
    for inpath in paths:
        size = len(payload)
        chars = goodchars(inpath)
        p, _ = os.path.splitext(inpath)
        outpath = p + '.ansi'
        c = min(chars, size)
        print('embedding {} chars in {}...'.format(c, outpath))
        embed(inpath, outpath, payload[offset:offset+c])
        size -= c
        offset += c


if __name__ == '__main__':
    main()
