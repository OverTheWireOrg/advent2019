int main() {
  for (int i = 0; i < 1000; i++) {
    if (malloc(100000) == 0) {
      puts("FAIL AS EXPECTED");
      return 1;
    }
  }
  puts("DONE");
}