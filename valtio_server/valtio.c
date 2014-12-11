#include <stdio.h>
#include <unistd.h>
#include <signal.h>
#include <time.h>

#include "blktrace.h"
#include "socket_comm.h"

void start();
void stop(__attribute__((__unused__)) int sig);
void printTime() {
    time_t t = time(NULL);
    struct tm tm = *localtime(&t);
    printf("%d-%d-%d %d:%d:%d\n", tm.tm_year + 1900, tm.tm_mon + 1, tm.tm_mday, tm.tm_hour, tm.tm_min, tm.tm_sec);
}

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
    printf("\n==================================\n");
    printTime();
	printf("Start socket connection\n");
	result = openConnection();
	if (result!=0) 
		return;

	// 2. set settings
    printf("\n==================================\n");
    printTime();
    printf("Get settings from client\n");
	result = getSettingFromClient(&device, &stopTime);
	if (result!=0)
		return;
	printf(" -device\t: %s\n -duration\t: %s\n",device,stopTime);
	
	// 3. start tracing
	printf("Start blktrace\n");
    signal(SIGINT, stop);
    signal(SIGHUP, stop);
    signal(SIGTERM, stop);
    signal(SIGALRM, stop);
    result = startBlktrace(device, stopTime);
	if (result != 0) {
        signal(SIGINT, SIG_IGN);
        signal(SIGHUP, SIG_IGN);
        signal(SIGTERM, SIG_IGN);
        signal(SIGALRM, SIG_IGN);

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
	
    printf("\n\n==================================\n");
	printTime();
    printf("Valtio server finished\n\n");
    
	exit(0);
}
