#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

#define MAX_STACK_SIZE 65536

typedef struct {
  unsigned char data[MAX_STACK_SIZE];
  int size;
} stack;

stack *new_stack() {
  stack *st = malloc(sizeof(stack));
  st->size = 0;
  return st;
}

void push_stack(stack *st, char value) {
  if (st->size >= MAX_STACK_SIZE) {
    puts("Stack overflow!");
    exit(1);
  }
  st->data[st->size] = value;
  st->size++;
}

char pop_stack(stack *st) {
  if (st->size == 0) {
    puts("Stack underflow!");
    exit(1);
  }
  st->size--;
  return st->data[st->size];
}

typedef struct {
  stack *data;
  stack *code;
  stack *output;
  stack *functions[0x20];
} vm_state;

vm_state new_vm_state() {
  vm_state state;
  state.data = new_stack();
  state.code = new_stack();
  state.output = new_stack();
  for (int i = 0; i < 0x20; i++) {
    state.functions[i] = new_stack();
  }
  return state;
}

void execute_one_inst(vm_state *vm) {
  unsigned char inst = pop_stack(vm->code);
  if (inst < 0x80) {
    push_stack(vm->data, inst);
    return;
  }
  if (inst == 0x80) {
    push_stack(vm->data, pop_stack(vm->data) ^ 0x80);
    return;
  }
  if (inst == 0x81) {
    unsigned char val = pop_stack(vm->data);
    if (val == 0) {
      push_stack(vm->data, 0xff);
    } else {
      push_stack(vm->data, 0);
    }
    return;
  }
  if (inst == 0x82) {
    unsigned char a = pop_stack(vm->data);
    unsigned char b = pop_stack(vm->data);
    push_stack(vm->data, a & b);
    return;
  }
  if (inst == 0x83) {
    unsigned char a = pop_stack(vm->data);
    unsigned char b = pop_stack(vm->data);
    push_stack(vm->data, a | b);
    return;
  }
  if (inst == 0x84) {
    unsigned char a = pop_stack(vm->data);
    unsigned char b = pop_stack(vm->data);
    push_stack(vm->data, a ^ b);
    return;
  }
  if (inst == 0x90) {
    unsigned char a = pop_stack(vm->data);
    unsigned char b = pop_stack(vm->data);
    push_stack(vm->data, a);
    push_stack(vm->data, b);
    return;
  }
  if (inst == 0x91) {
    unsigned char val = pop_stack(vm->data);
    push_stack(vm->data, val);
    push_stack(vm->data, val);
    return;
  }
  if (inst == 0xa0) {
    unsigned char index = pop_stack(vm->data);
    if (index >= 0x20) {
      puts("Invalid index");
      exit(1);
    }
    vm->functions[index]->size = 0;
    unsigned char c;
    while ((c = pop_stack(vm->data)) != 0xa1) {
      push_stack(vm->functions[index], c);
    }
    return;
  }
  if (inst == 0xb0) {
    push_stack(vm->output, pop_stack(vm->data));
    return;
  }
  if (inst >= 0xc0 && inst < 0xe0) {
    unsigned char index = inst - 0xc0;
    stack *func = vm->functions[index];
    for (int i = func->size - 1; i >= 0; i--) {
      push_stack(vm->code, func->data[i]);
    }
    return;
  }
  if (inst >= 0xe0) {
    unsigned char index = inst - 0xe0;
    if (pop_stack(vm->data)) {
      stack *func = vm->functions[index];
      for (int i = func->size - 1; i >= 0; i--) {
        push_stack(vm->code, func->data[i]);
      }
    }
    return;
  }
}

int print_flag() {
  puts("Congratulations!\n");
  system("/bin/cat flag.txt");
}

int main() {
  setvbuf(stdout, NULL, _IONBF, 0);
  puts("We just discovered a strange element called Assemblium.");
  puts("They are like mini robots. There are different isotopes, each having");
  puts("different behavior... though we haven't figured that out exactly yet.");
  puts("");
  puts("But I've got a great idea - why don't we build a toy that replicates");
  puts("itself? That would eliminate all our human, ahem, elvian costs.");
  puts("");
  puts("Let's do this!");
  puts("");
  printf("Length of your Assemblium sequence: ");
  int length = 0;
  scanf("%d", &length);
  if (length <= 0 || length >= 65535) {
    puts("Invalid length!");
    exit(1);
  }
  puts("Enter your Assemblium sequence:");
  unsigned char *user_code = malloc(length);
  for (int i = 0; i < length; i++) {
    read(0, user_code + i, 1);
  }
  vm_state vm = new_vm_state();
  for (int i = length - 1; i >= 0; i--) {
    push_stack(vm.code, user_code[i]);
  }
  while (vm.code->size > 0) {
    execute_one_inst(&vm);
  }
  if (length != vm.output->size) {
    goto fail;
  }
  for (int i = 0; i < length; i++) {
    if (vm.output->data[i] != user_code[i]) {
      goto fail;
    }
  }
  print_flag();
  exit(0);
fail:
  puts("Nope. The Assemblium sequence you provided produced the following:");
  write(1, vm.output->data, vm.output->size);
}
