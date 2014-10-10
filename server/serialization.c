#include "serialization.h"

int serializeIOTrace(IOTrace* structData, char* buffer)
{
    memcpy(buffer, structData, SE_BUFFER_SIZE);
    return 0;
}

int deserializeIOTraceWithBuffer(char* byteData, int offset, IOTrace* output)
{
    memcpy(output, byteData+offset, SE_BUFFER_SIZE);
    return 0;
}

IOTrace deserializeIOTrace(char* byteData, int offset)
{
    IOTrace t;
    memcpy(&t, byteData+offset, SE_BUFFER_SIZE);
    return t;
}