#ifndef SOCKET_COMM_H
#define SOCKET_COMM_H

#include "blktrace_api.h"

int openConnection();
int sendTraceToSocket(struct blk_io_trace*);
int closeConnection();

#endif