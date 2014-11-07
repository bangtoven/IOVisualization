#include <stdio.h>

#include "blktrace.h"
#include "socket_comm.h"

int main()
{
	if (openConnection()!=0) {
		printf("open connection failed!\n");
	}
	printf("open connection success!\n");
	
	char *device = "/dev/sda";
	int stopTime = 10;
	if (startBlktrace(device, stopTime)) {
		printf("error occurs during blktrace!\n");
	}
	printf("blktrace finished successfully!\n");
	
	closeConnection();
	
	return 0;
}