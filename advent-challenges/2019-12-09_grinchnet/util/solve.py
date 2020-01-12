#!/usr/bin/env python

from z3 import *
"""
/*
* 0 G == x[5] - 1
* 1 R == x[10] - 1
* 2 I == 'I'
* 3 N
* 4 C == x[11] * 2 + 1
* 5 H  == x[2] - 1
* 6 R == x[1]
* 7 U
* 8 L  == x[2] + 3
* 9 E == x[4] + 2
* 10 S == x[7] - 2
* 11 ! == '!'
*/
"""
x = [BitVec("x%d" % i, 8) for i in range(12)]

s = Solver()
s.add(x[0] == x[5] - 1)
s.add(x[1] == x[10] - 1)
s.add(x[2]^0x42 == 11)
s.add(x[3]^x[5] == 6)
s.add(x[4] == x[11] * 2 + 1)
s.add(x[5] == x[2] - 1)
s.add(x[6] ^ x[1] == 0)
s.add((x[7] >> 4) * (x[7] & 0xf) == 25)
s.add(x[8] == x[2] + 3)
s.add(x[9] == x[4] + 2)
s.add(x[10] == x[7] - 2)
s.add(x[11] >> 4 | x[11] << 4 == 0x12)

# + - * xor
# shift left and right

while s.check() == sat:
    m = s.model()
    print "".join([chr(m.evaluate(c).as_long()) for c in x])
    additional = Not(And([ c == m.evaluate(c).as_long() for c in x]))
    s.add(additional)


