all: boot.bin

boot.bin: boot.asm encrypt.py
	nasm boot.asm -o boot.bin
	ndisasm -o 0x7c00 -b16 boot.bin
	python3 encrypt.py

test: boot.bin
	qemu-system-x86_64 -hda boot.bin -cpu max -s

clean:
	rm -f boot.bin
