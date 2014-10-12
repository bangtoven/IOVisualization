CC=gcc
CFLAGS=-c -Wall

all: echoclient

echoclient: echoclient.o
	$(CC) echoclient.o ../server/serialization.o -o echoclient

echoclient.o: echoclient.c
	$(CC) $(CFLAGS) echoclient.c 

clean:
	rm -rf *o echoclient