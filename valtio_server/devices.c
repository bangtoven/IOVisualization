#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <regex.h>

void device_list (char* buffer) {
	regex_t regex;
	int reti = regcomp(&regex, "^/dev/[^/,]*,", 0);

    char *filename = "/proc/mounts";
    FILE *fp = fopen(filename, "r");
    char line[1000];

	int offset = 0;
    while(!feof(fp)) {
        fgets(line, 1000, fp);
	    char mountname[20] = {0};
		sscanf(line, "%s", mountname);

		int size = strlen(mountname);
		mountname[size] = ',';
		size++;
		
		reti = regexec(&regex, mountname, 0, NULL, 0);
		if( reti == 0 ) {
			memcpy(buffer+offset, mountname, size);
			offset += size;
		}
    }
	buffer[offset] = '\n';
	
   	regfree(&regex);	
	return;
}

int main()
{
	char buffer[100] = {0};
	device_list(buffer);
	printf("%s",buffer);
    return 0;
}
