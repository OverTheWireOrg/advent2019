BITS	16
ORG	0x7c00

start:
	cli
	xor	ax, ax
	mov	ds, ax
	mov	es, ax
	mov	ss, ax

	mov	sp, 0x7c00

	inc	ax
	cpuid

	; what osdev says to do to enable SSE
	mov	eax, cr0
	and	ax, 0xfffb
	or	ax, 0x2
	mov	cr0, eax
	mov	eax, cr4
	or	ax, 3 << 9
	mov	cr4, eax

	mov	si, msg
	call	puts

	bt	ecx, 25
	jnc	noaes

init:
	mov	si, prompt
	call	puts


	mov	di, password ; user input goes here
input:
	xor	ax, ax
	int	0x16
	cmp	al, 0xd
	je	input_done
	cmp	al, 0x8
	jne	nobs
	
	cmp	di, password
	jle	input
	
	mov	si, bs
	call	puts
	dec	di
	jmp	input
nobs:
	stosb
	mov	ah, 0xe
	int	0x10
	jmp	input

input_done:
	cmp	di, password+16 ; check pw not too long
	jne	init

	movaps	xmm0, [aes_key]
	movaps	xmm3, [password]

	call	do_aes

	pxor	xmm3, [pw_enc]
	ptest	xmm3, xmm3
	
	je	decs2

	jmp	init

noaes:
	mov	si, aesnt
	call	puts
	jmp	$


decs2:
	mov	si, stage2
s2loop:
	movaps	xmm0, [password] ; password is now key
	movaps	xmm3, [si]
	call	do_aes
	movaps	[si], xmm3
	add	si, 0x10
	cmp	si, pw_enc
	jne	s2loop

	jmp	stage2

; xmm0 = key
; xmm3 = data
do_aes:
	pxor	xmm2, xmm2
	pxor	xmm3, xmm0

	mov	bx, 0x36e5
	mov	ah, 0x73

encloop:

	shr	ax, 7
	div	bl

	mov	byte [rcon+5], ah

rcon:	aeskeygenassist	xmm1, xmm0, 69
	pshufd	xmm1, xmm1, 0b11111111
shuf:	shufps	xmm2, xmm0, 0b00010000
	pxor	xmm0, xmm2
	xor	byte [shuf+3], 0b00010000 ^ 0b10001100
	js	shuf
	pxor	xmm0, xmm1
	
	cmp	ah, bh
	je	last
	
	aesenc	xmm3, xmm0
	jmp	encloop

last:	aesenclast	xmm3, xmm0
	ret

puts:
	mov 	ah, 0xe
	lodsb
	xor	al, 0x42
	jz	.done
	xor	bx, bx
	int	0x10
	jmp	puts
.done:
	ret

msg:
DB	0xd, 0xa, "We upgraded from RC4 this year!", 0

prompt:
DB	0xd, 0xa, "Password: ", 0

aesnt:
DB	0xd, 0xa, "Come back with a modern CPU :P", 0

bs:
DB	0x8, 0x20, 0x8, 0





ALIGN	16


stage2:
	mov	si, winmsg
	call	puts
	jmp	$

winmsg:
DB	0xd, 0xa, 0xa, "Wow, 512 bytes is a lot of space.", 0xd, 0xa
DB	"Enjoy the rest of AOTW!", 0xd, 0xa
DB	0xa, "Anyway, here's your flag: ", 0x1a," AOTW{31oct__1s__25dec} ", 0x1b, 0xd, 0xa
DB	0xa, " - Retr0id", 0






TIMES	0x200-0x20-($-$$) \
	DB	0

pw_enc:
TIMES	16	DB	0

aes_key: ;random bytes + 55aa
DB	109, 121, 128, 185, 165, 10, 151, 36, 13, 45, 252, 54, 13, 149
DB	0x55, 0xAA

password:
