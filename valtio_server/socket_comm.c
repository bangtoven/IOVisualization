#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <stdio.h>
#include "socket_comm.h"

#define VALTIO_PORT 8462
static int connfd;

// static char *serial_buffer;
int sendTraceToClient(struct blk_io_trace * t){	
	if(t->sequence == 0)
		return 0;
	
	printf("#%10d\r",t->sequence);
	
	// printf("콜백 받았음! %d %d %d \t%ld \t%ld\n",t->sequence,t->pid,t->action, t->sector, t->time);
	// if (serial_buffer == NULL)
	// 	serial_buffer = malloc(SE_STRUCT_SIZE);
	// serializeIOTrace(t, serial_buffer); // t의 내용이 소켓통신 가능한 byte stream으로 serialize 되서 buffer에 저장됨.
	//
	// printf("send #%d trace\n",t->sequence);
	//
	// int n = write (connfd, t, SE_STRUCT_SIZE);
	// if(n<0) {
	// 	printf("Error writing to socket: %d\n",n);
	// 	exit(-1);
	// }
	return 0;
}

int getSettingFromClient(char** device, char** stopTime) {
	int BUFSIZE = 100;
	char buf[BUFSIZE];
	bzero(buf, BUFSIZE);

	int n;
	int endIndex = 0;
	while (1){
		/* read: read input string from the client */
		n = read(connfd, buf+endIndex, BUFSIZE-endIndex);
		if (n < 0) {
			perror("ERROR reading from socket");
			return -1;
		}
		
		if (buf[endIndex]=='\n') 
			break;
	
		printf("server received %d bytes: %s", n, buf);
	}
	
	printf("client sended: %s\n",buf);
	
	*device = "/dev/sda";
	*stopTime = "500";

	return 0;
}

int openConnection() {
	int listenfd; 					/* listening socket */
	int clientlen; 					/* byte size of client's address */
	struct sockaddr_in serveraddr; 	/* server's addr */
	struct sockaddr_in clientaddr; 	/* client addr */
	struct hostent *hostp; 			/* client host info */
	char *hostaddrp; 				/* dotted decimal host addr string */
	int optval; 					/* flag value for setsockopt */

	/* socket: create a socket */
	listenfd = socket(AF_INET, SOCK_STREAM, 0);
	if (listenfd < 0) {
		perror("ERROR opening socket");
		return -1;
	}

	/* setsockopt: Handy debugging trick that lets
	* us rerun the server immediately after we kill it;
	* otherwise we have to wait about 20 secs.
	* Eliminates "ERROR on binding: Address already in use" error.
	*/
	optval = 1;
	setsockopt(listenfd, SOL_SOCKET, SO_REUSEADDR, (const void *)&optval , sizeof(int));

	/* build the server's internet address */
	bzero((char *) &serveraddr, sizeof(serveraddr));
	serveraddr.sin_family = AF_INET; /* we are using the Internet */
	serveraddr.sin_addr.s_addr = htonl(INADDR_ANY); /* accept reqs to any IP addr */
	serveraddr.sin_port = htons((unsigned short)VALTIO_PORT); /* port to listen on */

	/* bind: associate the listening socket with a port */
	if (bind(listenfd, (struct sockaddr *) &serveraddr, sizeof(serveraddr)) < 0) {
		perror("ERROR on binding");
		return -1;
	}

	/* listen: make it a listening socket ready to accept connection requests */
	if (listen(listenfd, 5) < 0) { /* allow 5 requests to queue up */
		perror("ERROR on listen");
		return -1;
	}

	/* main loop: wait for a connection request, echo input line,
	then close connection. */
	clientlen = sizeof(clientaddr);

	/* accept: wait for a connection request */
	connfd = accept(listenfd, (struct sockaddr *) &clientaddr, (socklen_t *)&clientlen);
	if (connfd < 0) {
		perror("ERROR on accept");
		return -1;
    }

	/* gethostbyaddr: determine who sent the message */
	hostaddrp = inet_ntoa(clientaddr.sin_addr);
	printf("client addr: %s\n",hostaddrp);
	hostp = gethostbyname(hostaddrp);//, clientlen, AF_INET);
	if (hostp == NULL) {
		perror("ERROR on gethostbyaddr");
		return -1;
	}
	if (hostaddrp == NULL) {
		perror("ERROR on inet_ntoa\n");
		return -1;
	}

	printf("server established connection with %s (%s)\n", hostp->h_name, hostaddrp);
	return 0;
}

void closeConnection() {
	close(connfd);
}