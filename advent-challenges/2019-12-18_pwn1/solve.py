def egcd(a, b):
    if a == 0:
        return (b, 0, 1)
    else:
        g, y, x = egcd(b % a, a)
        return (g, x - (b // a) * y, y)

def modinv(a, m):
    g, x, y = egcd(a, m)
    if g != 1:
        raise Exception('modular inverse does not exist')
    else:
        return x % m

def moddiv2s(a, b, N):
    while a % 2 == 0 and b % 2 == 0:
        a /= 2
        b /= 2
    return (a * modinv(b, N)) % N

def modsqrt2s(a, N):
    n = 8
    sol = 1
    while n < N:
        if (sol * sol - a) / n % 2 == 1:
            sol += n / 2
        n *= 2
    assert sol * sol % N == a, (sol, N, a)
    return sol

def attempt(k1, k2, m):
    N = 2 ** 32
    mid = (k1 + k2 + m - 41 + N) % N
    if mid % 2 != 0:
        print 'Fail: k1 + k2 + m - 41 not even'
        return None
    mid2 = mid / 2
    if mid2 % 2 == 0:
        print 'Fail: (k1 + k2 + m - 41) not odd'
        return None
    if k1 * k2 * m % 32 == 0:
        print 'Fail: k1 k2 m contains more than 4 powers of 2'
        return None
    rhs = (moddiv2s(N - 362880, k1 * k2 * m, N) + mid2 ** 2) % N
    lhs = modsqrt2s(rhs, N)
    a = lhs - mid2
    b = (-a-mid)
    a = (a+N)%N
    b = (b+N)%N

    assert lhs * lhs % N == rhs
    assert (a * (N - b) + mid2 ** 2) % N == rhs

    print lhs, rhs, a, b, k1, k2, m
    assert ((a+b+k1+k2+m+4)%N  == 45), (a+b+k1+k2+m+4)%N
    assert (a*b*k1*k2*m % N == 362880), a*b*k1*k2*m % N
    return (a, b)




    
#attempt(151208,412513,131202)

def make_sudoku(a, b, k1, k2, m):
    sudoku = [
        [1, 4, 5, 2, 3, 6, 7, 9, 8],
        [6, 2, 7, 4, 8, 9, 3, 1, 5],
        [8, 9, 3, 5, 1, 7, 2, 4, 6],
        [2, 3, 4, 1, 5, 8, 9, 6, 7],
        [5, 6, 9, 7, 2, 4, 8, 3, 1],
        [7, 1, 8, 6, 9, 3, 4, 5, 2],
        [4, 8, 2, 9, 6, 5, 1, 7, 3],
        [9, 5, 1, 3, 7, 2, 6, 8, 4],
        [3, 7, 6, 8, 4, 1, 5, 2, 9]
    ]
    mapping = {1: 1, 2: 1, 3: 1, 4: 1, 5: a, 6: b, 7: m, 8: k1, 9: k2}
    result = ''
    for row in sudoku:
        for col in row:
            result += str(mapping[col]) + ' '
        result += '\n'
    return result

def solve(puts_got, scorer, win):
    N = 2 ** 32
    puts_off = (puts_got - scorer) / 4
    k1 = (N + puts_off) % N
    k2 = win
    for m in range(1, 17, 1):
        res = attempt(k1, k2, m)
        if res is not None:
            (a, b) = res
            sudoku = make_sudoku(a, b, k1, k2, m)
            return sudoku
    return None

from pwn import *

p = process('./chal')
elf = ELF('./chal')
print elf.symbols
puts_got = elf.symbols['got.puts']
scorer = elf.symbols['scorer']
win = elf.symbols['_Z3winv']
sudoku = solve(puts_got, scorer, win)
print sudoku
p.send(sudoku)
p.interactive()
