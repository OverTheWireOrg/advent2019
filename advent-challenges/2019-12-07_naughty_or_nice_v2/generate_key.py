from Crypto.Util import number

p = number.getPrime(512)
q = number.getPrime(512)

n = (p*q).to_bytes(128, "big")

for i in range(0, len(n), 8):
	print("\t" + ", ".join(["0x{:02x}".format(x) for x in n[i:i+8]]) + ",")
