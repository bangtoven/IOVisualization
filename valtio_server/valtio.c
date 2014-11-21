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
	int result;	

	char* device;
	char* stopTime;

	// 1. open connection
	printf("VALTIO: start socket connection\n");
	result = openConnection();
	if (result!=0) 
		return;

	// 2. set settings
	printf("VALTIO: get settings from client\n");
	result = getSettingFromClient(&device, &stopTime);
	if (result!=0)
		return;
	printf(" -device\t: %s\n -duration\t: %s\n",device,stopTime);
	
	// 3. start tracing
	signal(SIGINT, stop);
	signal(SIGHUP, stop);
	signal(SIGTERM, stop);
	signal(SIGALRM, stop);

	printf("VALTIO: start blktrace\n");
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
	
	printf("VALTIO: valtio server finished\n");
	exit(0);
}
