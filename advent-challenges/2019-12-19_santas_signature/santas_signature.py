#!/usr/bin/env python3
import Crypto
from Crypto.PublicKey import RSA
import sys

try:
	with open("key",'r') as f:
		key = RSA.importKey(f.read())
except:
	rng = Crypto.Random.new().read
	key = RSA.generate(4096, rng)
	with open("key",'w') as f:
		f.write(key.exportKey().decode("utf-8"))

def h2i(h):
	try:
		return int(h,16)
	except Exception:
		print("Couldn't hex decode",flush=True)
		sys.exit()

header = \
"""Dear Santa,
Last christmas you gave me your public key,
to confirm it really is you please sign three
different messages with your private key.

Here is the public key you gave me:"""
print(header,flush=True)
print(key.publickey().exportKey().decode("utf-8"),flush=True)
ms = []

for i in range(1,4):
	m = h2i(input("Message %d you signed (hex encoded):" % i))
	if m in ms:
		print("I said different messages!",flush=True)
		sys.exit()
	s = [h2i(input("Signature %d:" % i))]
	if not key.verify(m,s):
		print("Looks like you aren't Santa after all!",flush=True)
		sys.exit()
	ms.append(m)

print("Hello Santa, here is your flag:",flush=True)
with open("flag",'r') as flag:
	print(flag.read(),flush=True)