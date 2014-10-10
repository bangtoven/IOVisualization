CC=gcc
CFLAGS=-c -Wall

all: client

client: echoclient.o
	$(CC) echoclient.o ../server/serialization.o -o client

echoclient.o: echoclient.c
	$(CC) $(CFLAGS) echoclient.c 

clean:
	rm -rf *o client