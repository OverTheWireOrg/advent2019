import numpy as np

def matrix_power_final(A, x):
  result = np.identity(4, dtype=np.int64)
  while x > 0:
    if x % 2 == 1:
      result = np.matmul(result, A) % 9847613
    A = np.matmul(A, A) % 9847613
    x = x / 2
  return result


matrix = np.array([
    [1, 2, 3, 4],
    [5, 6, 7, 8],
    [9, 10, 11, 12],
    [13, 14, 15, 16]
], dtype=np.int64)

mat = matrix_power_final(matrix, 1234567890123456789)
#mat = matrix_power_final(matrix, 12345)
print mat

from pwn import *

buf = ''
for u in mat.reshape((16,)):
    print u
    buf += p16(u % 65536)

flag = 'M4tr1x_3xp0n3nti4t1on_5728391723'
assert len(flag) == len(buf)
print hexdump(buf)

print map(ord, xor(buf, flag))