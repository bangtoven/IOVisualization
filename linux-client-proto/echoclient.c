/* 
* echoclient.c - A simple connection-based client
* usage: echoclient <host> <port>
*/
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h> 

#include "serialization.h"

#define BUFSIZE 1024

void error(char *msg) {
	perror(msg);
	exit(0);
}

int main(int argc, char **argv) {
	int sockfd, portno, n;
	struct sockaddr_in serveraddr;
	struct hostent *server;
	char *hostname;

	/* check command line arguments */
	// if (argc != 3) {
	// 	fprintf(stderr,"usage: %s <hostname> <port>\n", argv[0]);
	// 	exit(0);
	// }
	hostname = "0.0.0.0";
	portno = 8462;

	/* socket: create the socket */
	sockfd = socket(AF_INET, SOCK_STREAM, 0);
	if (sockfd < 0) 
		error("ERROR opening socket");

	/* gethostbyname: get the server's DNS entry */
	server = gethostbyname(hostname);
	if (server == NULL) {
		fprintf(stderr,"ERROR, no such host as %s\n", hostname);
		exit(0);
	}

	/* build the server's Internet address */
	bzero((char *) &serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET;
	bcopy((char *)server->h_addr, 
	(char *)&serveraddr.sin_addr.s_addr, server->h_length);
	serveraddr.sin_port = htons(portno);

	/* connect: create a connection with the server */
	if (connect(sockfd, (const struct sockaddr *)&serveraddr, sizeof(serveraddr)) < 0) 
		error("ERROR connecting asdfasfdafds");

	write(sockfd,"/dev/sda,500\n",13);

	/* read: print the server's reply */
	char buffer[BUFSIZE];
	memset(buffer, 0, BUFSIZE);
	int endIndex = 0;
	while(1){
		n = read(sockfd, buffer+endIndex, BUFSIZE-endIndex); // 아까 남은 거 뒤에 버퍼 추가. 0으로 리셋 안함.
		if (n < 0) {
			error("ERROR reading from socket");
			break;
		}

		int bufferedLength = n + endIndex; // 지금 받아온 거 + 아까 남은 거.
		int offset = 0; // de-serialization 시작할 곳.
		while(bufferedLength-offset >= SE_STRUCT_SIZE ) { // 남은 buffer된 정보가 struct 크기보다 크면 진행.
			
			struct blk_io_trace t = deserializeIOTrace(buffer, offset); // structure 부활ㅋ
			
			printf("seq:%u\t", t.sequence);
			printf("pid:%8u\t", t.pid);
			printf("bytes:%8u\t", t.bytes);
			printf("sector:%llu\t", (unsigned long long) t.sector);
			printf("\n");
			
			offset += SE_STRUCT_SIZE; // struct size 만큼 돌면서 계속 진행.
		}
		endIndex = bufferedLength % SE_STRUCT_SIZE;		
		memmove(buffer, buffer+offset, endIndex); // 버퍼의 맨 앞으로 남은 data 당겨 놓기.
	}
	close(sockfd);
	return 0;
}
