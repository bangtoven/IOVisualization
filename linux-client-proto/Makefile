CC=gcc
CFLAGS=-c -Wall

all: echoclient

echoclient: echoclient.o serialization.o
	$(CC) echoclient.o serialization.o -o echoclient

echoclient.o: echoclient.c
	$(CC) $(CFLAGS) echoclient.c 
	
serialization.o: serialization.c
	$(CC) $(CFLAGS) serialization.c 

clean:
	rm -rf *o echoclient