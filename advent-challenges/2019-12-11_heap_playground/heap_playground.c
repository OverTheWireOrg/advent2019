#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define MAX_SIZE 1024

int glob_id = 0;

struct chunk {
	int id;
	int size;
	struct chunk *next;
};

struct chunk *head = 0;

void print_menu() {
	puts("1. Create chunk");
	puts("2. Delete chunk");
	puts("3. Print chunk");
	puts("4. Edit chunk");
	puts("5. Exit");
	printf("Choice: ");
}

void *find_chunk(int id){
	if(id > glob_id || id < 0){
		puts("No such chunk!");
		return 0;
	}
	for(struct chunk *chunk = head; chunk != 0; chunk = chunk->next){
		if(chunk->id == id)
			return chunk;
	}
	puts("No such chunk!");
	return 0;
}


void *alloc_chunk(size_t size, char *text){
	void *ptr = malloc(size + sizeof(struct chunk));
	if(ptr == 0){
		puts("Could not malloc");
	} else {
		if(head == 0){
			head = ptr;
		} else {
			struct chunk *next = head->next;
			struct chunk *prev = head;
			while(next != 0){
				prev = next;
				next = next->next;
			}
			prev->next = ptr;
		}
		struct chunk *new = ptr;
		new-> size = size;
		new->id = ++glob_id;
		new->next = 0;
		memset(ptr+sizeof(struct chunk),0,size);
		strncpy(ptr+sizeof(struct chunk),text,size-1);
	}
	return ptr;
}

void print_chunk(int id){
	struct chunk *chunk = find_chunk(id);
	if(chunk != 0){
			printf("Chunk %d:\n",chunk->id);
			printf("%s\n",(char *)chunk+sizeof(struct chunk));
	}
}

void free_chunk(int id){
	if(id > glob_id || id < 0){
		puts("No such chunk!");
		return;
	}
	struct chunk *prev = 0;
	struct chunk *chunk = head;
	while(chunk != 0){
		if(chunk->id == id){
			if(prev == 0){
				head = chunk->next;
			} else {
				prev->next = chunk->next;
			}
			free(chunk);
			puts("Chunk deleted!");
			return;
		}
		prev = chunk;
		chunk = chunk->next;
	}
	puts("No such chunk!");
	return;
}

void edit_chunk(struct chunk *chunk, int index, char c){
	if(index < 0){
		index = -index;
	}
	index = index % chunk->size;
	memset((char *)chunk+index+sizeof(struct chunk),c,1);
}

int main() {
	char choice;
	struct chunk *chunk;

	int size, id, index;
	char c;
	char buffer[MAX_SIZE];
	memset(buffer,0,MAX_SIZE);
	setbuf(stdout,0);

	puts("Heap playground!");
	while(1){
		print_menu();
		if(fgets(buffer, MAX_SIZE, stdin) == 0){
			break;
		}
		sscanf(buffer,"%c", &choice);
		switch (choice) {
			case '1':
				printf("Size of the chunk: ");
				fgets(buffer, MAX_SIZE, stdin);
				sscanf(buffer,"%d", &size);
				if (size > MAX_SIZE || size <= 0) {
					printf("Size must not be greater than %d and greater than zero\n", MAX_SIZE);
					break; 
				}
				printf("Content: ");
				fgets(buffer, MAX_SIZE, stdin);
				chunk = alloc_chunk(size, buffer);
				if (chunk != 0){
					printf("Created chunk %d\n", chunk->id);
				}
				break;
			case '2':
				if (head == 0){
					puts("No chunks yet!");
					break;
				}
				printf("ID of chunk: ");
				fgets(buffer, MAX_SIZE, stdin);
				sscanf(buffer,"%d", &id);
				free_chunk(id);
				break;
			case '3':
				if (head == 0){
					puts("No chunks yet!");
					break;
				}
				printf("ID of chunk: ");
				fgets(buffer, MAX_SIZE, stdin);
				sscanf(buffer,"%d", &id);
				print_chunk(id);
				break;
			case '4':
				if (head == 0){
					puts("No chunks yet!");
					break;
				}
				printf("ID of chunk: ");
				fgets(buffer, MAX_SIZE, stdin);
				sscanf(buffer,"%d", &id);
				chunk = find_chunk(id);
				if (chunk != 0){
					printf("Index of character to edit: ");
					fgets(buffer, MAX_SIZE, stdin);
					sscanf(buffer,"%d", &index);
					printf("Character: ");
					fgets(buffer, MAX_SIZE, stdin);
					sscanf(buffer,"%c", &c);
					edit_chunk(chunk,index,c);
				}
				break;
			case '5':
				return 0;
			default:
				printf("Unknown option %c\n", choice);
				break;
		}
	}
}