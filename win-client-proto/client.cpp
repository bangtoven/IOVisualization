#define _WIN32_WINNT  0x501
#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdlib.h>
#include <stdio.h>
#include <iostream>

#include "blktrace_win.h" // windows용 blktrace_api

// Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")

// c++ class 정의
class BlkIOTrace {
	uint32_t magic;
	uint32_t sequence;
	uint64_t time;	
	uint64_t sector;
	uint32_t bytes;
	uint32_t action;
	uint32_t pid;	
	uint32_t device;
	uint32_t cpu;	
	uint16_t error;
	uint16_t pdu_len;
public:
	BlkIOTrace (struct blk_io_trace* blkStruct)
	{
		magic = blkStruct->magic;
		sequence = blkStruct->sequence;
		time = blkStruct->time;
		sector = blkStruct->sector;
		bytes = blkStruct->bytes;
		action = blkStruct->action;
		pid = blkStruct->pid;
		device = blkStruct->device;
		cpu = blkStruct->cpu;
		error = blkStruct->error;
		pdu_len = blkStruct->pdu_len;
	}
	
	void printBlkIOTrace();
};

void BlkIOTrace::printBlkIOTrace()
{
	std::cout << "seq: " << sequence << "\t";
	std::cout << "pid: " << pid << "\t";
	std::cout << "time: " << time << "\t";
	std::cout << "sector: " << sector << "\t";
	std::cout << "bytes: " << bytes << "\t";
	std::cout << "\n";
};

#define DEFAULT_BUFLEN 4096
#define DEFAULT_PORT "8462"

int __cdecl main(int argc, char **argv) 
{
    WSADATA wsaData;
    SOCKET ConnectSocket = INVALID_SOCKET;
    struct addrinfo *result = NULL,
                    *ptr = NULL,
                    hints;
    char recvbuf[DEFAULT_BUFLEN];
    int iResult;
    int recvbuflen = DEFAULT_BUFLEN;
	
	int BLK_STRUCT_SIZE = sizeof(struct blk_io_trace);
    
    // Validate the parameters
    if (argc != 2) {
        printf("usage: %s server-name\n", argv[0]);
        return 1;
    }

    // Initialize Winsock
    iResult = WSAStartup(MAKEWORD(2,2), &wsaData);
    if (iResult != 0) {
        printf("WSAStartup failed with error: %d\n", iResult);
        return 1;
    }

    ZeroMemory( &hints, sizeof(hints) );
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    // Resolve the server address and port
    iResult = getaddrinfo(argv[1], DEFAULT_PORT, &hints, &result);
    if ( iResult != 0 ) {
        printf("getaddrinfo failed with error: %d\n", iResult);
        WSACleanup();
        return 1;
    }

    // Attempt to connect to an address until one succeeds
    for(ptr=result; ptr != NULL ;ptr=ptr->ai_next) {

        // Create a SOCKET for connecting to server
        ConnectSocket = socket(ptr->ai_family, ptr->ai_socktype, 
            ptr->ai_protocol);
        if (ConnectSocket == INVALID_SOCKET) {
            printf("socket failed with error: %ld\n", WSAGetLastError());
            WSACleanup();
            return 1;
        }

        // Connect to server.
        iResult = connect( ConnectSocket, ptr->ai_addr, (int)ptr->ai_addrlen);
        if (iResult == SOCKET_ERROR) {
            closesocket(ConnectSocket);
            ConnectSocket = INVALID_SOCKET;
            continue;
        }
        break;
    }

    freeaddrinfo(result);

    if (ConnectSocket == INVALID_SOCKET) {
        printf("Unable to connect to server!\n");
        WSACleanup();
        return 1;
    }

    // shutdown the connection since no more data will be sent
    iResult = shutdown(ConnectSocket, SD_SEND);
    if (iResult == SOCKET_ERROR) {
        printf("shutdown failed with error: %d\n", WSAGetLastError());
        closesocket(ConnectSocket);
        WSACleanup();
        return 1;
    }

    // Receive until the peer closes the connection
	struct blk_io_trace *t = (struct blk_io_trace *)malloc(BLK_STRUCT_SIZE); // struct의 buffer
	int endIndex = 0;
    while(1) {
        iResult = recv(ConnectSocket, recvbuf, recvbuflen, 0);
        if ( iResult < 0 ) {
            printf("recv failed with error: %d\n", WSAGetLastError());
			break;
		}
		else if (iResult == 0) {
			printf("connection ended\n");
			break;
		}
        else {
            printf("Bytes received: %d\n", iResult);
			
			int bufferedLength = iResult + endIndex; // 지금 받아온 거 + 아까 남은 거.
			int offset = 0; // de-serialization 시작할 곳.
			while(bufferedLength-offset >= BLK_STRUCT_SIZE ) { // 남은 buffer된 정보가 struct 크기보다 크면 진행.				
			    memcpy(t, recvbuf+offset, BLK_STRUCT_SIZE); // struct 부활.
			
				// c++ class 생성.
				BlkIOTrace obj = BlkIOTrace(t);
				obj.printBlkIOTrace();
			
				offset += BLK_STRUCT_SIZE; // struct size 만큼 돌면서 계속 진행.
			}
			endIndex = bufferedLength % BLK_STRUCT_SIZE;
			if (endIndex>0)
				printf("remaining data: %d\n",endIndex);		
			
			memmove(recvbuf, recvbuf+offset, endIndex); // 버퍼의 맨 앞으로 남은 data 당겨 놓기.
		}
    };

    // cleanup
    closesocket(ConnectSocket);
    WSACleanup();

    return 0;
}