#include <stdio.h>
#include <unistd.h>
#include <signal.h>

#include "blktrace.h"
#include "socket_comm.h"

void start();
void stop(__attribute__((__unused__)) int sig);

int main()
{
	start();
	
	return 0;
}

void start() {
	signal(SIGINT, stop);
	signal(SIGHUP, stop);
	signal(SIGTERM, stop);
	signal(SIGALRM, stop);

	int result;	

	// 1. open connection
	result = openConnection();
	if (result!=0)
		return;


	char *device = "/dev/sda";
	int stopTime = 500;
	
	// 3. start tracing
	result = startBlktrace(device, stopTime);
	if (result != 0) {
		printf("error occurs during starting blktrace!\n");
		return;
	}
}

void stop(__attribute__((__unused__)) int sig) {
	signal(SIGINT, SIG_IGN);
	signal(SIGHUP, SIG_IGN);
	signal(SIGTERM, SIG_IGN);
	signal(SIGALRM, SIG_IGN);
	
	stopBlktrace();
	closeConnection();
	
	printf("valtio server finished\n");
	exit(0);
}
