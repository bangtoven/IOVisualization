#ifndef SOCKET_COMM_H
#define SOCKET_COMM_H

#include "blktrace_api.h"

int openConnection();
int getSettingFromClient(char** device, char** stopTime);
int sendTraceToClient(struct blk_io_trace*);
void closeConnection();

#endif