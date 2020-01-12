#include <cstdio>
#include <cstdlib>

uint sudoku[9][9];
uint scorer[9 + 1];

bool checkrow(uint nums[9]) {
    uint sum = 0;
    uint prod = 1;
    for (int i = 0; i < 9; i++) {
        sum += nums[i];
        prod *= nums[i];
    }
    // Lazy way to check nums is a permutation of 1 - 9.
    return sum == 45 && prod == 362880;
}

bool check() {
    uint row[9];
    for (int i = 0; i < 9; i++) {
        for (int j = 0; j < 9; j++) {
            row[j] = sudoku[i][j];
        }
        if (!checkrow(row)) {
            return false;
        }
    }
    for (int j = 0; j < 9; j++) {
        for (int i = 0; i < 9; i++) {
            row[i] = sudoku[i][j];
        }
        if (!checkrow(row)) {
            return false;
        }
    }
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            for (int ii = 0; ii < 3; ii++) {
                for (int jj = 0; jj < 3; jj++) {
                    row[ii * 3 + jj] = sudoku[i * 3 + ii][j * 3 + jj];
                }
            }
            if (!checkrow(row)) {
                return false;
            }
        }
    }
    return true;
}

void win() {
    system("/bin/cat flag.txt");
}

void score() {
    puts("Let me take a look...");
    for (int i = 0; i < 8; i++) {
        scorer[sudoku[i][i]] = sudoku[i + 1][i + 1];
    }
    uint score = 1;
    for (int i = 1; i <= 9; i++) {
        score *= scorer[i];
    }
    if (score >= 1000000) {
        win();
    } else {
        puts("That is an unimpressive sudoku.");
        exit(0);
    }
}

int main() {

    setvbuf(stdin, 0, _IONBF, 0);
    setvbuf(stdout, 0, _IONBF, 0);
    
    puts("Could you give me a fully solved sudoku?");
    puts("Enter 9 lines, each containing 9 integers, space separated:");
    for (int i = 0; i < 9; i++) {
        for (int j = 0; j < 9; j++) {
            scanf("%u", &sudoku[i][j]);
        }
    }
    if (!check()) {
        puts("That is not a valid sudoku.");
        exit(0);
    }
    score();
}