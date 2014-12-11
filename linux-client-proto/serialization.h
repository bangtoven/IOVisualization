// 10.10.2014. Jungho Bang
// serialize blk_io_trace structure

/*
 * serializeIOTrace
 input
 - IOTrace* structData : data to serialize
 - char* buffer : buffer to store byte stream. it must have been allocated by the caller.
 output
 - 0 if success (for error handling)

 * deserializeIOTraceWithBuffer
 input
 - char* byteData : data to de-serialize
 - int offset : starting point of the struct data. this will read 48 byte from it.
 - IOTrace* output : we will allocate only one memory space for structure. it must have been allocated by the caller.
 output
 - 0 if success (for error handling)
This function is more memory-efficient.

 * deserializeIOTrace
 input
 - char* byteData : data to de-serialize
 - int offset : starting point of the struct data. this will read 48 byte from it.
 output
 - generate IOTrace struct
 
 */

#ifndef SERIALIZE_H
#define SERIALIZE_H

#include <string.h>
#include <asm/types.h>

#define SE_STRUCT_SIZE 32

struct blk_io_trace {
    __u64 time;
    __u64 sector;
    __u32 bytes;
    __u32 action;
    __u32 pid;
    __u32 sequence;	// can be used.
};

/*
struct blk_io_trace {
    __u32 magic;	
    __u32 sequence;	
    __u64 time;
    __u64 sector;	
    __u32 bytes;	
    __u32 action;	
    __u32 pid;
    __u32 device;	
    __u32 cpu;
    __u16 error;	
    __u16 pdu_len;
};
*/
typedef struct blk_io_trace IOTrace;

int serializeIOTrace(IOTrace* structData, char* buffer);
int deserializeIOTraceWithBuffer(char* byteData, int offset, IOTrace* output);
IOTrace deserializeIOTrace(char* byteData, int offset);

#endif
