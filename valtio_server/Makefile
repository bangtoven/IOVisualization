CC	= gcc
CFLAGS	= -Wall -O2 -g -W
ALL_CFLAGS = $(CFLAGS) -D_GNU_SOURCE -D_LARGEFILE_SOURCE -D_FILE_OFFSET_BITS=64
PROGS	= valtio
LIBS	= -lpthread

ALL = $(PROGS) 

all: $(ALL)

%.o: %.c
	$(CC) -o $*.o -c $(ALL_CFLAGS) $<

valtio: serialization.o blktrace.o act_mask.o valtio.o 
	$(CC) $(ALL_CFLAGS) -o $@ $(filter %.o,$^) $(LIBS)

clean: 
	-rm -f *.o $(PROGS) .depend btrace-1.0.tar.bz2

ifneq ($(wildcard .depend),)
include .depend
endif
