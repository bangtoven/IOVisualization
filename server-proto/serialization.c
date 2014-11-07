#include "serialization.h"
// 현재는 속도문제가 발생하지 않는 것 같지만, memcpy 없이 하는 대체방법 있을지 찾아보면 좋을 듯.

int serializeIOTrace(IOTrace* structData, char* buffer)
{
    memcpy(buffer, structData, SE_STRUCT_SIZE);
    return 0;
}

int deserializeIOTraceWithBuffer(char* byteData, int offset, IOTrace* output)
{
    memcpy(output, byteData+offset, SE_STRUCT_SIZE);
    return 0;
}

IOTrace deserializeIOTrace(char* byteData, int offset)
{
    IOTrace t;
    memcpy(&t, byteData+offset, SE_STRUCT_SIZE);
    return t;
}
