#include <immintrin.h>
#include <stdio.h>

__uint32_t data[] = {
    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
    1, 0, 9847613, 0
};

unsigned char flag[] = {
    252, 20, 235, 9, 188, 174, 231, 71, 79, 227, 124, 193, 82, 165, 2, 142, 137, 113, 200, 141, 150, 35, 1, 109, 113, 64, 90, 234, 253, 70, 29, 35};

int main() {
    register __m128i row;
    register __m128i cell;
    register __m128i target[4];
    register __m128i temp[4];

    target[0] = _mm_loadu_ps(data + 16);
    target[0] = _mm_shuffle_epi32(target[0], 0x54);
    target[1] = _mm_shuffle_epi32(target[0], 0x51);
    target[2] = _mm_shuffle_epi32(target[0], 0x45);
    target[3] = _mm_shuffle_epi32(target[0], 0x15);

    // for (__int64_t i = 0; i < 12345; i++) {
    for (__int64_t i = 0; i < 1234567890123456789ll; i++) {
        // These fences are needed to disable compiler optimization from pulling the assignments out.
        _mm_sfence();
        row = _mm_loadu_ps(data);
        cell = _mm_shuffle_epi32(row, 0x00);
        temp[0] = _mm_mullo_epi32(cell, target[0]);
        cell = _mm_shuffle_epi32(row, 0x55);
        temp[0] = _mm_add_epi32(temp[0], _mm_mullo_epi32(cell, target[1]));
        cell = _mm_shuffle_epi32(row, 0xaa);
        temp[0] = _mm_add_epi32(temp[0], _mm_mullo_epi32(cell, target[2]));
        cell = _mm_shuffle_epi32(row, 0xff);
        temp[0] = _mm_add_epi32(temp[0], _mm_mullo_epi32(cell, target[3]));

        row = _mm_loadu_ps(data + 4);
        cell = _mm_shuffle_epi32(row, 0x00);
        temp[1] = _mm_mullo_epi32(cell, target[0]);
        cell = _mm_shuffle_epi32(row, 0x55);
        temp[1] = _mm_add_epi32(temp[1], _mm_mullo_epi32(cell, target[1]));
        cell = _mm_shuffle_epi32(row, 0xaa);
        temp[1] = _mm_add_epi32(temp[1], _mm_mullo_epi32(cell, target[2]));
        cell = _mm_shuffle_epi32(row, 0xff);
        temp[1] = _mm_add_epi32(temp[1], _mm_mullo_epi32(cell, target[3]));

        row = _mm_loadu_ps(data + 8);
        cell = _mm_shuffle_epi32(row, 0x00);
        temp[2] = _mm_mullo_epi32(cell, target[0]);
        cell = _mm_shuffle_epi32(row, 0x55);
        temp[2] = _mm_add_epi32(temp[2], _mm_mullo_epi32(cell, target[1]));
        cell = _mm_shuffle_epi32(row, 0xaa);
        temp[2] = _mm_add_epi32(temp[2], _mm_mullo_epi32(cell, target[2]));
        cell = _mm_shuffle_epi32(row, 0xff);
        temp[2] = _mm_add_epi32(temp[2], _mm_mullo_epi32(cell, target[3]));

        row = _mm_loadu_ps(data + 12);
        cell = _mm_shuffle_epi32(row, 0x00);
        temp[3] = _mm_mullo_epi32(cell, target[0]);
        cell = _mm_shuffle_epi32(row, 0x55);
        temp[3] = _mm_add_epi32(temp[3], _mm_mullo_epi32(cell, target[1]));
        cell = _mm_shuffle_epi32(row, 0xaa);
        temp[3] = _mm_add_epi32(temp[3], _mm_mullo_epi32(cell, target[2]));
        cell = _mm_shuffle_epi32(row, 0xff);
        temp[3] = _mm_add_epi32(temp[3], _mm_mullo_epi32(cell, target[3]));

        // I can't find a way to do packed mod, so I just repeatedly subtract.
        // Also, the modulus is 9847613 which is in general too high to 
        // keep under int32, but because our original matrix only has small
        // values, this is fine. To solve this puzzle though, one needs to
        // use fast exponentiation which requires squaring matrices, and in
        // that case it would be necessary to use at least int64.
        for (int i = 0; i < 1000; i++) {
            row = _mm_loadu_ps(data + 16);
            cell = _mm_shuffle_epi32(row, 0xaa);
            row = _mm_shuffle_epi32(row, 0x00);

            target[0] = _mm_cmplt_epi32(temp[0], cell);
            target[0] = _mm_add_epi32(target[0], row);
            target[0] = _mm_mullo_epi32(target[0], cell);
            temp[0] = _mm_sub_epi32(temp[0], target[0]);

            target[0] = _mm_cmplt_epi32(temp[1], cell);
            target[0] = _mm_add_epi32(target[0], row);
            target[0] = _mm_mullo_epi32(target[0], cell);
            temp[1] = _mm_sub_epi32(temp[1], target[0]);

            target[0] = _mm_cmplt_epi32(temp[2], cell);
            target[0] = _mm_add_epi32(target[0], row);
            target[0] = _mm_mullo_epi32(target[0], cell);
            temp[2] = _mm_sub_epi32(temp[2], target[0]);

            target[0] = _mm_cmplt_epi32(temp[3], cell);
            target[0] = _mm_add_epi32(target[0], row);
            target[0] = _mm_mullo_epi32(target[0], cell);
            temp[3] = _mm_sub_epi32(temp[3], target[0]);
        }

        target[0] = temp[0];
        target[1] = temp[1];
        target[2] = temp[2];
        target[3] = temp[3];
    }

    __uint8_t output[16];
    _mm_store_ps(output, target[0]);
    _mm_store_ps(output + 16, target[1]);
    _mm_store_ps(output + 32, target[2]);
    _mm_store_ps(output + 48, target[3]);
    for (int i = 0; i < 16; i++) {
        flag[i * 2] ^= output[i * 4];
        flag[i * 2 + 1] ^= output[i * 4 + 1];
    }
    write(1, "AOTW{", 5);
    write(1, flag, 32);
    write(1, "}\n", 2);
    // TODO: why does it segfault if I return 0 instead?
    exit(0);
}
