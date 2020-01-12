import tempfile
import os
import zlib
import resource

content = open('elf').read()

print 'We just rescued an elf that was captured by The Grinch'
print 'for his cruel genetic experiments.'
print
print 'But we were late, the poor elf was already mutated.'
print 'Could you help us restore the elf\'s genes?'
print
print 'Here is the elf\'s current DNA, zlib compressed and'
print 'then hex encoded:'
print '=================================================='
print zlib.compress(content, 9).encode('hex')
print '=================================================='
print
print 'You may mutate up to 4 bytes of the elf.'

count = int(raw_input("How many bytes to mutate (0 - 4)? "))
if count < 0 or count > 4:
    print "Invalid number"
    quit()
for i in range(count):
    pos = int(raw_input('Which byte to mutate? '))
    val = int(raw_input('What to set the byte to? '))
    assert 0 <= pos < len(content)
    assert 0 <= val < 256
    content = content[:pos] + chr(val) + content[pos+1:]

print 'Alright - let\'s see what the elf has to say.'
print '=================================================='

try:
    mutated_elf, elf_name = tempfile.mkstemp('mutated_elf')
    os.write(mutated_elf, content)
    os.close(mutated_elf)
    os.chmod(elf_name, int('700', 8))
    resource.setrlimit(resource.RLIMIT_CPU, (1, 1))
    os.system(elf_name)
finally:
    os.remove(elf_name)
