from pwn import *
context.log_level = 'debug'
p = remote('localhost', 1214)
quine = open('example_solution').read()
p.sendline(str(len(quine)))
p.send(quine)
p.interactive()

