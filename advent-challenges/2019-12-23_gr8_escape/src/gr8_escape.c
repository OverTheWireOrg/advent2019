#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <unistd.h>
#include <stdbool.h>
#include <termios.h>
#include <sys/ioctl.h>

#define SCREEN_WIDTH  64
#define SCREEN_HEIGHT 32

#define   VX (cpu.op & 0x0f00) >> 8
#define   VY (cpu.op & 0x00f0) >> 4
#define    N (cpu.op & 0x000f)
#define   KK (cpu.op & 0x00ff)
#define  NNN (cpu.op & 0x0fff)
#define ADDR (cpu.op & 0x0fff)

typedef uint8_t  byte;
typedef uint16_t word;

struct gpu_t {
    byte gfx[SCREEN_WIDTH * SCREEN_HEIGHT];
    bool  draw;
} gpu;

byte keys[0x10] = {
    0x78, // x -> 0
    0x31, // 1 -> 1
    0x32, // 2 -> 2
    0x33, // 3 -> 3
    0x71, // q -> 4
    0x77, // w -> 5
    0x65, // e -> 6
    0x61, // a -> 7
    0x73, // s -> 8
    0x64, // d -> 9
    0x7a, // z -> a
    0x63, // c -> b
    0x34, // 4 -> c
    0x72, // r -> d
    0x66, // f -> e
    0x76, // v -> f
};

struct cpu_t {
    byte sp, dt, st;
    word i,  pc, op;

    byte m[0x1000];
    byte v[0x0010];
    bool k[0x0010];
    word s[0x0010];
    
    void (*debug)();
} cpu;

byte font[80] = {
    0xf0, 0x90, 0x90, 0x90, 0xf0, 0x20, 0x60, 0x20, 0x20, 0x70,
    0xf0, 0x10, 0xf0, 0x80, 0xf0, 0xf0, 0x10, 0xf0, 0x10, 0xf0,
    0x90, 0x90, 0xf0, 0x10, 0x10, 0xf0, 0x80, 0xf0, 0x10, 0xf0,
    0xf0, 0x80, 0xf0, 0x90, 0xf0, 0xf0, 0x10, 0x20, 0x40, 0x40,
    0xf0, 0x90, 0xf0, 0x90, 0xf0, 0xf0, 0x90, 0xf0, 0x10, 0xf0,
    0xf0, 0x90, 0xf0, 0x90, 0x90, 0xe0, 0x90, 0xe0, 0x90, 0xe0,
    0xf0, 0x80, 0x80, 0x80, 0xf0, 0xe0, 0x90, 0x90, 0x90, 0xe0,
    0xf0, 0x80, 0xf0, 0x80, 0xf0, 0xf0, 0x80, 0xf0, 0x80, 0x80,
};

byte last  = 0;
bool debug = false;

char *canvas;

void init(){

    cpu.pc = 0x200;

    for (int i = 0; i < 80; i++)
        cpu.m[i] = font[i];

    srand(time(0));

    canvas = calloc(1, (SCREEN_WIDTH * SCREEN_HEIGHT) * 4);
}

void mode(int dir){

    struct termios term;
    tcgetattr(STDIN_FILENO, &term);

    if (dir == 1){

        term.c_lflag &= ~ECHO;
        tcsetattr(STDIN_FILENO, TCSANOW, &term);

    } else {

        term.c_lflag |= ECHO;
        tcsetattr(STDIN_FILENO, TCSANOW, &term);
    }
}

byte key(){

    struct termios original;
    tcgetattr(STDIN_FILENO, &original);

    struct termios term;
    memcpy(&term, &original, sizeof(term));

    term.c_lflag &= ~ICANON;
    tcsetattr(STDIN_FILENO, TCSANOW, &term);

    int characters_buffered = 0;
    ioctl(STDIN_FILENO, FIONREAD, &characters_buffered);

    tcsetattr(STDIN_FILENO, TCSANOW, &original);

    bool pressed = (characters_buffered != 0);

    if (pressed){

        byte c = getchar();
        if (c != last && c > 47){
            for (int i = 0; i < 0x10; i++){
                if (keys[i] == c){
                    cpu.k[i] = true;
                } else {
                    cpu.k[i] = false;
                }
            }
        }

        last = c;
    } else {
        if (last > 47){
            last = 0;
        } else {
            if (last == 47){
                for (int i = 0; i < 0x10; i++){
                    cpu.k[i] = false;
                }
            }
            last++;
        }
    }

    return pressed;
}

void cycle(){

    byte h, x, y;

    cpu.op = cpu.m[cpu.pc] << 8 | cpu.m[cpu.pc+1];
    
    switch(cpu.op & 0xf000){

        case 0x0000:

            switch (KK){

                case 0x0000: /* SYS */

                    cpu.pc += 2;

                break;

                case 0x00e0: /* CLS */

                    memset(gpu.gfx, 0, sizeof(gpu.gfx));
                    gpu.draw = true;
                    cpu.pc += 2;

                break;

                case 0x00ee: /* RET */

                    cpu.sp--;
                    cpu.pc = cpu.s[cpu.sp];
                    cpu.pc += 2;

                break;
            }

        break;

        case 0x1000: /* JP addr */

            cpu.pc = ADDR;

        break;

        case 0x2000: /* CALL addr */

            cpu.s[cpu.sp] = (cpu.s[cpu.sp] & 0xf000) + (cpu.pc & 0xfff);
            cpu.sp++;
            cpu.pc = ADDR;

        break;

        case 0x3000: /* SE Vx, byte */

            if (cpu.v[VX] == KK){
                cpu.pc += 4;
            } else {
                cpu.pc += 2;
            }

        break;

        case 0x4000: /* SNE Vx, byte */

            if (cpu.v[VX] != KK){
                cpu.pc += 4;
            } else {
                cpu.pc += 2;
            }

        break;

        case 0x5000: /* SE Vx, Vy */

            if (cpu.v[VX] == cpu.v[VY]){
                cpu.pc += 4;
            } else {
                cpu.pc += 2;
            }

        break;

        case 0x6000: /* LD Vx, byte */

            cpu.v[VX] = KK;
            cpu.pc += 2;

        break;

        case 0x7000: /* ADD Vx, byte */

            cpu.v[0x0f] = (KK > 0xff - cpu.v[VX])? 1: 0;
            cpu.v[VX] += KK;
            cpu.pc += 2;

        break;

        case 0x8000:

            switch (N){

                case 0x0000: /* LD Vx, Vy */

                    cpu.v[VX] = cpu.v[VY];
                    cpu.pc += 2;

                break;

                case 0x0001: /* OR Vx, Vy */

                    cpu.v[VX] |= cpu.v[VY];
                    cpu.pc += 2;

                break;

                case 0x0002: /* AND Vx, Vy */

                    cpu.v[VX] &= cpu.v[VY];
                    cpu.pc += 2;

                break;

                case 0x0003: /* XOR Vx, Vy */

                    cpu.v[VX] ^= cpu.v[VY];
                    cpu.pc += 2;

                break;

                case 0x0004: /* ADD Vx, Vy */

                    cpu.v[0x0f] = (cpu.v[VY] > 0xff - cpu.v[VX])? 1: 0;
                    cpu.v[VX] += cpu.v[VY];
                    cpu.pc += 2;

                break;

                case 0x0005: /* SUB Vx, Vy */

                    //
                    cpu.v[0x0f] = (cpu.v[VX] >= cpu.v[VY])? 1: 0;
                    cpu.v[VX] -= cpu.v[VY];
                    cpu.pc += 2;

                break;

                case 0x0006: /* SHR Vx {, Vy} */

                    cpu.v[0x0f] = cpu.v[VX] & 0x01;
                    cpu.v[VX] = cpu.v[VX] >> 1;
                    cpu.pc += 2;

                break;

                case 0x0007: /* SUBN Vx, Vy */

                    cpu.v[0x0f] = (cpu.v[VY] >= cpu.v[VX])? 1: 0;
                    cpu.v[VX] = cpu.v[VY] - cpu.v[VX];
                    cpu.pc += 2;

                break;

                case 0x000e: /* SHL Vx {, Vy} */

                    cpu.v[0x0f] = (cpu.v[VX] & 0x80)? 1: 0;
                    cpu.v[VX] = cpu.v[VX] << 1;
                    cpu.pc += 2;

                break;
            }

        break;

        case 0x9000: /* SNE Vx, Vy */

            if (cpu.v[VX] != cpu.v[VY]){
                cpu.pc += 4;
            } else {
                cpu.pc += 2;
            }

        break;

        case 0xa000: /* LD I, addr */

            cpu.i = ADDR;
            cpu.pc += 2;

        break;

        case 0xb000: /* JP V0, addr */

            cpu.pc = ADDR + cpu.v[0];

        break;

        case 0xc000: /* RND Vx, byte */

            cpu.v[VX] = (rand() % 0xff) & KK;
            cpu.pc += 2;

        break;

        case 0xd000: /* DRW Vx, Vy, nibble */

            cpu.v[0x0f] = 0;

            for (byte yline = 0; yline < N; yline++){
                byte pixel = cpu.m[cpu.i+yline];

                for (byte xline = 0; xline < 8; xline++){
                    if ((pixel & (0x80 >> xline)) != 0){

                        int i = cpu.v[VX]+xline+((cpu.v[VY]+yline) * SCREEN_WIDTH);

                        if (gpu.gfx[i] == 1)
                            cpu.v[0x0f] = 1;
                        
                        gpu.gfx[i] ^= 1;
                    }
                }
            }

            gpu.draw = true;
            cpu.pc += 2;

        break;

        case 0xe000:

            switch (KK){

                case 0x009e: /* SKP Vx */

                    if (cpu.k[cpu.v[VX]])
                        cpu.pc += 2;

                    cpu.pc += 2;

                break;

                case 0x00a1: /* SKNP Vx */

                    if (!cpu.k[cpu.v[VX]])
                        cpu.pc += 2;

                    cpu.pc += 2;

                break;
            }

        break;

        case 0xf000:

            switch (KK){

                case 0x0007: /* LD Vx, DT */

                    cpu.v[VX] = cpu.dt;
                    cpu.pc += 2;

                break;

                case 0x000a: /* LD Vx, K */

                    for (int i = 0; i < 0x10; i++)
                        if (cpu.k[i] == true)
                            cpu.pc += 2;

                break;

                case 0x0015: /* LD DT, Vx */

                    cpu.dt = cpu.v[VX];
                    cpu.pc += 2;

                break;

                case 0x0018: /* LD ST, Vx */

                    cpu.st = cpu.v[VX];
                    cpu.pc += 2;

                break;

                case 0x001e: /* ADD I, Vx */

                    cpu.i += cpu.v[VX];
                    cpu.pc += 2;

                break;

                case 0x0029: /* LD F, Vx */

                    cpu.i = cpu.v[VX] * 5;
                    cpu.pc += 2;

                break;

                case 0x0033: /* LD B, Vx */

                    cpu.m[cpu.i]   = (byte) (cpu.v[VX] / 100);
                    cpu.m[cpu.i+1] = (byte) (cpu.v[VX] % 100) / 10;
                    cpu.m[cpu.i+2] = (byte) (cpu.v[VX] % 10);

                    cpu.pc += 2;

                break;

                case 0x0055: /* LD [I], Vx */

                    for(word i = 0; i <= VX; i++)
                        cpu.m[cpu.i+i] = cpu.v[i];
                    
                    cpu.i += VX + 1;
                    cpu.pc += 2;

                break;

                case 0x0065: /* LD Vx, [I] */

                    for(word i = 0; i <= VX; i++)
                        cpu.v[i] = cpu.m[cpu.i + i];
                    
                    cpu.i += VX + 1;
                    cpu.pc += 2;

                break;

                case 0x0075: /* DBG */

                    debug = true;
                    cpu.pc += 2;

                break;
            }

        break;
    }
}

void draw(){

    memset(canvas, 0, (SCREEN_WIDTH * SCREEN_HEIGHT) * 4);
    strncat(canvas, "\033[2J+----------------------------------------------------------------+\n", 71);
    char *ptr = canvas+71;

    for (int y = 0; y < SCREEN_HEIGHT; y++){
        *ptr++ = '|';
        for (int x = 0; x < SCREEN_WIDTH; x++)
            if (gpu.gfx[x+(y*SCREEN_WIDTH)] != 0){
                *ptr++ = '\xe2';
                *ptr++ = '\x96';
                *ptr++ = '\x88';
            } else{
                *ptr++ = ' ';
            }

        *ptr++ = '|';
        *ptr++ = '\n';
    }

    strcat(ptr, "+----------------------------------------------------------------+");
    puts(canvas);

    if (debug && cpu.debug != NULL)
        (*cpu.debug)();

    gpu.draw = false;
}

void timers(){

    if (cpu.st > 0)
        cpu.st--;

    if (cpu.st == 1)
        printf("\a");

    if (cpu.dt > 0)
        cpu.dt--;
}

bool load(){

    char *rom;
    FILE *fp;
    int   sz = 0;

    rom = malloc(0x1000);
    if (rom == NULL)
        return false;

    printf("game: ");
    sz = read(STDIN_FILENO, rom, 0xe00);
    if (sz <= 0)
        return false;

    if (sz > 0xe00)
        return false;

    for (int i = 0; i < sz; i++)
        cpu.m[0x200+i] = rom[i];

    return true;
}

void highscore(){

    int  n  = 0;
    char c  = 0;
    char *ptr = 0;
    char name[0x10];

    mode(0);
    printf("\e[1mCongrats!\e[0m You got a highscore.\nEnter your name below t" \
    "o submit to our servers...\n\n");

    c = 0;
    do {

        if (c > 0 && (c = getchar()) != '\n')
            ungetc(c, stdin);

        printf("name: ");
        ptr = name;
        while ((c = getchar()) != EOF && c != '\n' && c != '\r')
            *ptr++ = c;

        printf("%s would you like to try again? (y/n): ", name);

    } while (scanf(" %c", &c) && (c == 'y' || c == 'Y'));

    printf("\e[1mSubmitting...\e[0m\n\n");

}

void debug_registers(){

    printf("| \e[1mRegisters:\e[0m                                           " \
    "          |\n|                                                          " \
    "      |\n| \e[1mV0\e[0m 0x%02x           \e[1mV1\e[0m 0x%02x           "  \
    "\e[1mV2\e[0m 0x%02x           \e[1mV3\e[0m 0x%02x  |\n| \e[1mV4\e[0m 0x%" \
    "02x           \e[1mV5\e[0m 0x%02x           \e[1mV6\e[0m 0x%02x         " \
    "  \e[1mV7\e[0m 0x%02x  |\n| \e[1mV8\e[0m 0x%02x           \e[1mV9\e[0m 0" \
    "x%02x           \e[1mVa\e[0m 0x%02x           \e[1mVb\e[0m 0x%02x  |\n| " \
    "\e[1mVc\e[0m 0x%02x           \e[1mVd\e[0m 0x%02x           \e[1mVe\e[0m" \
    " 0x%02x           \e[1mVf\e[0m 0x%02x  |\n|                             " \
    "                                   |\n| PC 0x%04x         SP 0x%04x     " \
    "                               |\n+-------------------------------------" \
    "---------------------------+\n",
        cpu.v[0x00], cpu.v[0x01], cpu.v[0x02], cpu.v[0x03],
        cpu.v[0x04], cpu.v[0x05], cpu.v[0x06], cpu.v[0x07],
        cpu.v[0x08], cpu.v[0x09], cpu.v[0x0a], cpu.v[0x0b],
        cpu.v[0x0c], cpu.v[0x0d], cpu.v[0x0e], cpu.v[0x0f],
        cpu.pc, cpu.sp
    );
}

void banner(){

    puts("\n\e[38;2;67;143;201m\e[48;2;88;49;12m▀▀\e[48;2;67;143;201m\e[38;2;" \
    "144;199;240m▄▄\e[0m\e[38;2;67;143;201m\e[48;2;88;49;12m▀▀\e[0m \e[48;2;6" \
    "7;143;201m\e[38;2;144;199;240m▄▄\e[0m   \e[48;2;67;143;201m\e[38;2;144;1" \
    "99;240m▄▄\e[0m \e[48;2;67;143;201m\e[38;2;144;199;240m▄▄\e[0m\e[38;2;67;" \
    "143;201m\e[48;2;88;49;12m▀▀▀▀\e[0m   \e[38;2;144;199;240m▄\e[48;2;67;143" \
    ";201m▄\e[0m\e[38;2;67;143;201m\e[48;2;88;49;12m▀▀▀▀\e[0m\e[38;2;144;199;" \
    "240m▄\e[0m \e[48;2;67;143;201m\e[38;2;144;199;240m▄▄\e[0m\e[38;2;67;143;" \
    "201m\e[48;2;88;49;12m▀▀▀\e[48;2;67;143;201m\e[38;2;144;199;240m▄\e[0m\e[" \
    "38;2;144;199;240m▄\e[0m \e[38;2;144;199;240m▄\e[48;2;67;143;201m▄\e[0m\e" \
    "[38;2;67;143;201m\e[48;2;88;49;12m▀▀▀\e[48;2;67;143;201m\e[38;2;144;199;" \
    "240m▄\e[0m\e[38;2;144;199;240m▄\e[0m\n  \e[48;2;211;237;254m\e[38;2;255;" \
    "255;250m▄▄\e[0m   \e[48;2;211;237;254m\e[38;2;255;255;250m▄▄\e[0m\e[38;2" \
    ";255;255;250m▄▄▄\e[48;2;211;237;254m▄▄\e[0m \e[48;2;211;237;254m\e[38;2;" \
    "255;255;250m▄▄\e[0m\e[38;2;255;255;250m▄▄▄\e[0m    \e[48;2;211;237;254m"  \
    "\e[38;2;255;255;250m▄▄\e[0m  \e[38;2;255;255;250m▄▄\e[48;2;88;49;12m▄\e[" \
    "0m \e[48;2;211;237;254m\e[38;2;255;255;250m▄▄\e[0m\e[38;2;255;255;250m▄▄" \
    "▄\e[48;2;211;237;254m▄\e[0m\e[38;2;211;237;254m\e[48;2;88;49;12m▀\e[0m "  \
    "\e[38;2;211;237;254m\e[48;2;88;49;12m▀\e[48;2;211;237;254m\e[38;2;255;25" \
    "5;250m▄\e[0m\e[38;2;255;255;250m▄▄▄\e[48;2;211;237;254m▄\e[0m\e[38;2;211" \
    ";237;254m\e[48;2;88;49;12m▀\e[0m\n  \e[48;2;161;116;31m\e[38;2;216;163;5" \
    "1m▄▄\e[0m   \e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e[38;2;88;49;1" \
    "2m▀▀▀\e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m \e[48;2;161;116;31m\e" \
    "[38;2;216;163;51m▄▄\e[0m\e[38;2;88;49;12m▀▀▀\e[0m    \e[48;2;161;116;31m" \
    "\e[38;2;216;163;51m▄▄\e[0m  \e[38;2;88;49;12m▀\e[0m\e[48;2;161;116;31m\e" \
    "[38;2;216;163;51m▄▄\e[0m \e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e" \
    "[38;2;88;49;12m▀\e[38;2;161;116;31m\e[48;2;88;49;12m▀\e[48;2;161;116;31m" \
    "\e[38;2;216;163;51m▄\e[0m\e[38;2;216;163;51m\e[48;2;88;49;12m▄\e[0m  \e[" \
    "48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e[38;2;88;49;12m▀▀▀\e[0m\e[48" \
    ";2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\n  \e[38;2;244;207;127m\e[48;2;" \
    "88;49;12m▀▀\e[0m   \e[38;2;244;207;127m\e[48;2;88;49;12m▀▀\e[0m   \e[38;" \
    "2;244;207;127m\e[48;2;88;49;12m▀▀\e[0m \e[38;2;244;207;127m\e[48;2;88;49" \
    ";12m▀▀▀▀▀▀\e[0m   \e[38;2;88;49;12m▀\e[0m\e[38;2;244;207;127m\e[48;2;88;" \
    "49;12m▀▀▀▀▀\e[0m\e[38;2;88;49;12m▀\e[0m \e[38;2;244;207;127m\e[48;2;88;4" \
    "9;12m▀▀\e[0m  \e[38;2;88;49;12m▀\e[38;2;244;207;127m\e[48;2;88;49;12m▀▀"  \
    "\e[0m \e[38;2;88;49;12m▀\e[0m\e[38;2;244;207;127m\e[48;2;88;49;12m▀▀▀▀▀"  \
    "\e[0m\e[38;2;88;49;12m▀\e[0m\n\n \e[48;2;67;143;201m\e[38;2;144;199;240m" \
    "▄▄\e[0m\e[38;2;67;143;201m\e[48;2;88;49;12m▀▀▀▀\e[0m \e[38;2;144;199;240" \
    "m▄\e[48;2;67;143;201m▄\e[0m\e[38;2;67;143;201m\e[48;2;88;49;12m▀▀▀\e[48;" \
    "2;67;143;201m\e[38;2;144;199;240m▄\e[0m\e[38;2;144;199;240m▄\e[0m \e[38;" \
    "2;144;199;240m▄\e[48;2;67;143;201m▄\e[0m\e[38;2;67;143;201m\e[48;2;88;49" \
    ";12m▀▀▀\e[48;2;67;143;201m\e[38;2;144;199;240m▄\e[0m\e[38;2;144;199;240m" \
    "▄\e[0m  \e[38;2;144;199;240m▄\e[48;2;67;143;201m▄▄▄\e[0m\e[38;2;144;199;" \
    "240m▄\e[0m  \e[48;2;67;143;201m\e[38;2;144;199;240m▄▄\e[0m\e[38;2;67;143" \
    ";201m\e[48;2;88;49;12m▀▀▀\e[48;2;67;143;201m\e[38;2;144;199;240m▄\e[0m\e" \
    "[38;2;144;199;240m▄\e[0m \e[48;2;67;143;201m\e[38;2;144;199;240m▄▄\e[0m"  \
    "\e[38;2;67;143;201m\e[48;2;88;49;12m▀▀▀▀\e[0m\n \e[48;2;211;237;254m\e[3" \
    "8;2;255;255;250m▄▄\e[0m\e[38;2;255;255;250m▄▄▄\e[0m  \e[38;2;211;237;254" \
    "m\e[48;2;88;49;12m▀\e[48;2;211;237;254m\e[38;2;255;255;250m▄\e[0m\e[38;2" \
    ";255;255;250m▄▄▄▄\e[0m  \e[48;2;211;237;254m\e[38;2;255;255;250m▄▄\e[0m " \
    "  \e[38;2;211;237;254m\e[48;2;88;49;12m▀▀\e[0m \e[48;2;211;237;254m\e[38" \
    ";2;255;255;250m▄▄\e[0m\e[38;2;88;49;12m▀▀▀\e[0m\e[48;2;211;237;254m\e[38" \
    ";2;255;255;250m▄▄\e[0m \e[48;2;211;237;254m\e[38;2;255;255;250m▄▄\e[0m\e" \
    "[38;2;255;255;250m▄▄▄\e[48;2;211;237;254m▄\e[0m\e[38;2;211;237;254m\e[48" \
    ";2;88;49;12m▀\e[0m \e[48;2;211;237;254m\e[38;2;255;255;250m▄▄\e[0m\e[38;" \
    "2;255;255;250m▄▄▄\e[0m\n \e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e" \
    "[38;2;88;49;12m▀▀▀\e[0m  \e[38;2;216;163;51m▄\e[48;2;88;49;12m▄\e[0m\e[3" \
    "8;2;88;49;12m▀▀▀\e[0m\e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m \e[48" \
    ";2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m   \e[48;2;161;116;31m\e[38;2;21" \
    "6;163;51m▄▄\e[0m \e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e[38;2;16" \
    "1;116;31m\e[48;2;88;49;12m▀▀▀\e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[" \
    "0m \e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e[38;2;88;49;12m▀▀▀▀\e[" \
    "0m  \e[48;2;161;116;31m\e[38;2;216;163;51m▄▄\e[0m\e[38;2;88;49;12m▀▀▀\e[" \
    "0m\n \e[38;2;244;207;127m\e[48;2;88;49;12m▀▀▀▀▀▀\e[0m \e[38;2;88;49;12m▀" \
    "\e[38;2;244;207;127m\e[48;2;88;49;12m▀▀▀▀▀\e[0m\e[38;2;88;49;12m▀ ▀\e[38" \
    ";2;244;207;127m\e[48;2;88;49;12m▀▀▀▀▀\e[0m\e[38;2;88;49;12m▀ \e[38;2;244" \
    ";207;127m\e[48;2;88;49;12m▀▀\e[0m   \e[38;2;244;207;127m\e[48;2;88;49;12" \
    "m▀▀\e[0m \e[38;2;244;207;127m\e[48;2;88;49;12m▀▀\e[0m      \e[38;2;244;2" \
    "07;127m\e[48;2;88;49;12m▀▀▀▀▀▀\e[0m\n");
}

int main(int argc, char *argv[]){

    setvbuf(stdin, 0, _IONBF, 0);
    setvbuf(stdout, 0, _IONBF, 0);

    banner();

    if (!load()){
        printf("error: cannot load game.\n");
        exit(EXIT_FAILURE);
    }

    struct timespec tim;
    tim.tv_sec = 0;
    tim.tv_nsec = 4166666; // 1000000000 / 240;
    tim.tv_nsec = 33333333;

    init();
    mode(1);

    cpu.debug = &debug_registers;

    while (1){

        key();
        for (int i = 0; i < 10; i++)
            cycle();

        if (gpu.draw)
            draw();

        timers();
        nanosleep(&tim, NULL);
    }

    return EXIT_SUCCESS;
}