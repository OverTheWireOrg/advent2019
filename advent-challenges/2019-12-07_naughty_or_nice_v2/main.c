#include <stdio.h>
#include <stdlib.h>
#include <sys/mman.h>
#include <stdint.h>
#include <string.h>
#include <unistd.h>
#include <gmp.h>

#define E 0x10001

#define RSA_SIZE_BYTES 128

static uint8_t pubkey[RSA_SIZE_BYTES] = {
	0x68, 0x4d, 0x23, 0x17, 0x94, 0x50, 0x38, 0x1b,
	0xbd, 0x59, 0x71, 0x6c, 0x68, 0xb0, 0x55, 0xff,
	0xe5, 0x3d, 0x12, 0xd6, 0x2d, 0x76, 0x26, 0x35,
	0xbb, 0xb1, 0xe1, 0x72, 0x9d, 0x4f, 0x3c, 0x2d,
	0x88, 0x7d, 0xd3, 0x01, 0x9b, 0xe8, 0x60, 0x1c,
	0x28, 0x34, 0xfa, 0x15, 0x79, 0x3a, 0xee, 0x86,
	0xb6, 0x35, 0x0a, 0x67, 0x24, 0x0c, 0x67, 0xad,
	0xfc, 0x64, 0xd3, 0x20, 0xb5, 0xb5, 0xbb, 0x54,
	0xbd, 0x26, 0x7a, 0xa1, 0x16, 0x97, 0xf3, 0xad,
	0x9f, 0x22, 0x82, 0xb7, 0x38, 0x88, 0xd0, 0xac,
	0x4c, 0x8f, 0xae, 0x2a, 0xf6, 0x88, 0x25, 0xfd,
	0x7d, 0xbe, 0xb3, 0x31, 0x72, 0xef, 0x4d, 0x4d,
	0x69, 0x51, 0x54, 0x3e, 0xc7, 0x34, 0x99, 0x2d,
	0x74, 0x2f, 0x21, 0xe8, 0x6e, 0x1f, 0x72, 0x80,
	0x7d, 0x57, 0xb6, 0x21, 0x31, 0x5a, 0xb7, 0x88,
	0xa9, 0x23, 0xca, 0xd1, 0x73, 0x12, 0xb6, 0xeb
};

static uint8_t ciphertext[RSA_SIZE_BYTES];
static uint8_t plaintext[RSA_SIZE_BYTES+1];

void make_executable(void *buf, size_t len)
{
	mprotect((void*)(((uintptr_t)buf)&~0xFFF), (len&~0xFFF) + 0x1000, PROT_READ|PROT_EXEC);
}

uint8_t * rsa_pkcs1_unpad(uint8_t * plaintext)
{
	int i=2;
	if (plaintext[0] != 0) return NULL;
	if (plaintext[1] != 2) return NULL;
	while (plaintext[i] && i<RSA_SIZE_BYTES) i++;
	i++;
	if (i > RSA_SIZE_BYTES) return NULL;
	if (i < 11) return NULL;
	return &plaintext[i];
}

void fancy_puts(const char *msg)
{
	while(*msg) {
		putchar(*msg++);
		usleep(20000);
	}
	usleep(1000000);
}

int main()
{
	mpz_t n, ct;
	
	setvbuf(stdout, NULL, _IONBF, 0);
	
	// TODO: ANSI art
	fancy_puts("After last year's embarrassment, Santa decided to simplify how he authenticated letters.\n\n");
	fancy_puts(" \"What's the point of hashing the message first, if the message is short?\n");
	fancy_puts("  We can just encrypt the message itself with the private key!\n");
	fancy_puts("  It should be fine as long as we use a secure padding scheme like PKCS#1 1.5, right?\"\n");
	fancy_puts("\nSo, what would you like for Christmas?\n");
	
	if (fread(ciphertext, 1, sizeof(ciphertext), stdin) != sizeof(ciphertext)) {
		printf("Your wish list is too short!\n");
		exit(-1);
	}
	
	mpz_init(n);
	mpz_import(n, sizeof(pubkey), 1, 1, 0, 0, pubkey);
	
	mpz_init(ct);
	mpz_import(ct, sizeof(ciphertext), 1, 1, 0, 0, ciphertext);
	
	mpz_powm_ui(ct, ct, E, n);
	
	size_t pt_size;
	mpz_export(plaintext, &pt_size, 1, 1, 0, 0, ct);
	memmove(plaintext+RSA_SIZE_BYTES-pt_size, plaintext, pt_size);
	memset(plaintext, 0, RSA_SIZE_BYTES-pt_size);
	plaintext[RSA_SIZE_BYTES] = 0;
	
	uint8_t * shellcode = rsa_pkcs1_unpad(plaintext);
	
	if (shellcode) {
		printf("Nice!\n");
		
		make_executable(shellcode, strlen((char*)shellcode));
		((void (*)(void))shellcode)();
	} else {
		printf("Naughty!\n");
	}
}
