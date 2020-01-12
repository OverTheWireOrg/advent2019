from Crypto.Cipher import AES

#password = b"A"*16
password = b"MiLiT4RyGr4d3MbR"

mbr = bytearray(open("boot.bin", "rb").read())

mbr[-32:-16] = AES.new(bytes(mbr[-16:]), AES.MODE_ECB).encrypt(password)

def encrypt_strings_between(a, b):
	start = mbr.index(a)
	end = mbr.index(b) + len(b)
	for i in range(start, end):
		mbr[i] ^= 0x42

encrypt_strings_between(b"\r\nWe", b"\b \b\0")
encrypt_strings_between(b"\r\n\n", b"Retr0id\0")

S2_START = 0x150
S2_END = 0x1E0

mbr[S2_START:S2_END] = AES.new(password, AES.MODE_ECB).decrypt(bytes(mbr[S2_START:S2_END]))

open("boot.bin", "wb").write(mbr)
