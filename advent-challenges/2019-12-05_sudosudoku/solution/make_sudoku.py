#!/usr/bin/env python

import random, pprint
from z3 import *

dim = 3
dim2 = dim*dim

def make_sum(X, l, res):
        varlist = [X[i][j] for (i,j) in l]
        acc = 0
        for v in varlist:
            acc += v
        return (acc == res)

def make_prod(X, l, res):
        varlist = [X[i][j] for (i,j) in l]
        acc = 1
        for v in varlist:
            acc *= v
        return (acc == res)

def make_eq(X, l, res):
        return And([X[i][j] == res for (i,j) in l])

def get_all_results(base_constraints):
    # 9x9 matrix of integer variables
    X = [ [ Int("x[%s][%s]" % (i, j)) for j in range(dim2) ] 
          for i in range(dim2) ]

    # each cell contains a value in {1, ..., maxval}
    cells_c  = [ And(1 <= X[i][j], X[i][j] <= dim2) 
                 for i in range(dim2) for j in range(dim2) ]

    # each row contains a digit at most once
    rows_c   = [ Distinct(X[i]) for i in range(dim2) ]

    # each column contains a digit at most once
    cols_c   = [ Distinct([ X[i][j] for i in range(dim2) ]) 
                 for j in range(dim2) ]

    # each 3x3 square contains a digit at most once
    sq_c     = [ Distinct([ X[dim*i0 + i][dim*j0 + j] 
                            for i in range(dim) for j in range(dim) ]) 
                 for i0 in range(dim) for j0 in range(dim) ]

    sudoku_c = cells_c + rows_c + cols_c + sq_c

    # sudoku instance, we use '0' for empty cells
    instance = [ [ 0 for _ in range(dim2) ] for _ in range(dim2) ]

    s = Solver()
    s.add(sudoku_c)

    solcount = 0

    # add base constraints
    # sum, prod, xor

    for op, res, l in base_constraints:
        if op == "+":
            s.add(make_sum(X, l, res))
        if op == "*":
            s.add(make_prod(X, l, res))
        if op == "==":
            s.add(make_eq(X, l, res))

    while s.check() == sat:
        m = s.model()
        r = [ [ m.evaluate(X[i][j]) for j in range(dim2) ] 
              for i in range(dim2) ]
        print_matrix(r)

        additional = Not(And([ X[i][j] == r[i][j] for j in range(dim2) for i in range(dim2) ]))
        s.add(additional)
        solcount += 1
    else:
        print "failed to solve"

    print solcount


def make_random_sum(sol, size):
    varlist = []

    res = 0

    for _ in range(size):
        x = random.randrange(dim2)
        y = random.randrange(dim2)
        v = sol[x][y]
        res += v

        varlist += [(x,y)]

    return ("+", res, varlist)

def make_random_prod(sol, size):
    varlist = []

    res = 1

    for _ in range(size):
        x = random.randrange(dim2)
        y = random.randrange(dim2)
        v = sol[x][y]
        res *= v

        varlist += [(x,y)]

    return ("*", res, varlist)

def make_random_eq(sol, size):
    res = 1 + random.randrange(dim2)
    varlist = [(x,y) for x in range(dim2) for y in range(dim2) if sol[x][y] == res]
    random.shuffle(varlist)
    varlist = varlist[:size]

    return ("==", res, varlist)

soll4 = [[7, 3, 4, 14, 8, 10, 9, 5, 2, 11, 16, 6, 1, 13, 12, 15],
         [11, 6, 5, 1, 15, 14, 7, 12, 3, 8, 9, 13, 16, 2, 4, 10],
         [10, 9, 2, 8, 4, 3, 13, 16, 12, 1, 15, 5, 11, 6, 14, 7],
         [13, 15, 16, 12, 6, 2, 1, 11, 14, 4, 7, 10, 9, 5, 8, 3],
         [6, 16, 10, 3, 9, 15, 12, 1, 13, 2, 14, 11, 7, 8, 5, 4],
         [12, 1, 11, 5, 3, 16, 4, 14, 7, 15, 6, 8, 2, 10, 9, 13],
         [14, 4, 7, 15, 13, 8, 5, 2, 10, 9, 12, 3, 6, 16, 1, 11],
         [2, 8, 9, 13, 11, 7, 6, 10, 4, 5, 1, 16, 3, 14, 15, 12],
         [1, 12, 6, 4, 5, 9, 11, 13, 8, 16, 2, 7, 15, 3, 10, 14],
         [9, 5, 14, 7, 2, 4, 15, 6, 1, 10, 3, 12, 13, 11, 16, 8],
         [16, 13, 3, 11, 10, 12, 8, 7, 6, 14, 4, 15, 5, 9, 2, 1],
         [8, 10, 15, 2, 16, 1, 14, 3, 11, 13, 5, 9, 4, 12, 7, 6],
         [4, 7, 13, 6, 1, 5, 2, 8, 16, 12, 11, 14, 10, 15, 3, 9],
         [5, 2, 8, 10, 14, 6, 3, 9, 15, 7, 13, 1, 12, 4, 11, 16],
         [3, 14, 1, 9, 12, 11, 16, 15, 5, 6, 10, 4, 8, 7, 13, 2],
         [15, 11, 12, 16, 7, 13, 10, 4, 9, 3, 8, 2, 14, 1, 6, 5]]

soll3 = [[8, 6, 4, 7, 2, 9, 5, 3, 1],
         [9, 1, 2, 4, 5, 3, 7, 6, 8],
         [3, 7, 5, 6, 1, 8, 2, 4, 9],
         [6, 4, 9, 8, 7, 5, 3, 1, 2],
         [7, 2, 1, 9, 3, 6, 8, 5, 4],
         [5, 3, 8, 2, 4, 1, 6, 9, 7],
         [4, 8, 6, 5, 9, 7, 1, 2, 3],
         [1, 9, 7, 3, 6, 2, 4, 8, 5],
         [2, 5, 3, 1, 8, 4, 9, 7, 6]]

if dim == 3:
    solll = soll3
if dim == 4:
    solll = soll4

xxx = [ ]

pprint.pprint(solll)

random.seed(31337)
xxx += [make_random_eq(solll, random.choice([4,5,6]))]
xxx += [make_random_eq(solll, random.choice([4,5,6]))]

for _ in range(12):
    xxx += [make_random_sum(solll, random.choice([4,5,6]))]

for (op, res, varlist) in xxx:
    out = (" %s " % op).join(["s[%d,%d]" % (x+1,y+1) for (x,y) in varlist])
    out += " = %d" % res
    print out

flag = ""
for i in range(dim2 - 1):
    flag += ("%d" % solll[0][i])
for i in range(dim2 - 1):
    flag += ("%d" % solll[i][dim2 - 1])
for i in range(dim2-1, 0, -1):
    flag += ("%d" % solll[dim2 - 1][i])
for i in range(dim2-1, 0, -1):
    flag += ("%d" % solll[i][0])

print flag

get_all_results(xxx)
