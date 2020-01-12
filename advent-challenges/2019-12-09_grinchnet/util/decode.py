#!/usr/bin/env python3

import sys

def crypt(key, data):
    S = list(range(256))
    j = 0

    for i in list(range(256)):
        j = (j + S[i] + ord(key[i % len(key)])) % 256
        S[i], S[j] = S[j], S[i]

    j = 0
    y = 0
    out = []

    for char in data:
        j = (j + 1) % 256
        y = (y + S[j]) % 256
        S[j], S[y] = S[y], S[j]

        out.append(chr(ord(char) ^ S[(S[j] + S[y]) % 256]))

    return ''.join(out)

def x(key):
    arr = [int(x) for x in sys.stdin.readline().strip().split(",") if x != ""]
    return crypt(key, [chr(c) for c in arr])


