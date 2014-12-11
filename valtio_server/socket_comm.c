#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <asm/types.h>
#include "socket_comm.h"

#define VALTIO_PORT 8462
static int connfd;

struct valtio_trace {
    __u64 time;		
    __u64 sector;	
    __u32 bytes;	
    __u32 action;	
    __u32 pid;		
    __u32 dontcare;	// can be used as sequence
};

// static char *serial_buffer;
int sendTraceToClient(struct blk_io_trace * b){
	if (b->sector == 0)
		return 0;
	
//    struct valtio_trace *t = ((void*)b)+8;
//    t->sequence = b->sequence;
//    printf("#%10d %lu\n",t->pid,(long unsigned int)t->sector);
    printf("#%10d %lu\n",b->pid,(long unsigned int)b->sector);
    
	int n = write (connfd, ((void*)b)+8, 32);
	if(n<0) {
        socketError = 1;
        alarm(1);
		return -1;
	}
	
	return 0;
}

int getSettingFromClient(char** device, char** stopTime) {
	int BUFSIZE = 100;
	char buf[BUFSIZE];
	bzero(buf, BUFSIZE);

	int n;
	int endIndex = 0;
    int try = 10;
	while (try-- > 0){
		/* read: read input string from the client */
		n = read(connfd, buf+endIndex, BUFSIZE-endIndex);
		if (n < 0) {
			perror("ERROR reading from socket");
			return -1;
		}
		endIndex += n;
		if (buf[endIndex-1]=='\n')
			break;
	}
		
	printf("client sended: %s",buf);
    
    for (n = 0; n<BUFSIZE; n++) {
        if (buf[n]==',') {
            *device = malloc(n+1);
            memcpy(*device, buf, n);
            (*device)[n] = 0;
            break;
        }
    }
    
    int comma = n;
    for ( ; n<BUFSIZE; n++) {
        if (buf[n]=='\n') {
            int length = n-comma-1;
            *stopTime = malloc(length+1);
            memcpy(*stopTime, buf+(comma+1), length);
            (*stopTime)[length] = 0;
            break;
        }
    }

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
    socketError = 0;
    
	return 0;
}

void closeConnection() {
	close(connfd);
}