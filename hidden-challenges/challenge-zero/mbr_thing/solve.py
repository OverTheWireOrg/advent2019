from Crypto.Cipher import AES

mbr = open("boot.bin", "rb").read()
pw = AES.new(mbr[-16:], AES.MODE_ECB).decrypt(mbr[-32:-16])
print("Password: " + pw.decode())

# This part is unnecessary, could just type the password above into QEMU and read out the flag
for s in AES.new(pw, AES.MODE_ECB).encrypt(mbr).split(b" "):
	if b"AOTW{" in s:
		print("Flag: " + s.decode())
