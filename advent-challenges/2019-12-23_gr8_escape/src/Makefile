CFLAGS=-s -no-pie -fno-pic -O0 -static
SRC=gr8_escape.c
OBJ=$(SRC:.c=.o)
BIN=gr8_escape

.PHONY : clean

all:
	$(MAKE) $(BIN)

$(BIN): $(OBJ)
	$(CC) $(CFLAGS) $(OBJ) -o $@

.o:
	$(CC) $(CFLAGS) $< -o $@

challenge: all
	rm -f $(OBJ)

clean:
	rm -f $(BIN) $(OBJ)
