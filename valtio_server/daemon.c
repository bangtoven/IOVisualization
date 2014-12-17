//
//  valtiod.c
//  valtio-server
//
//  Created by Jungho Bang on 2014. 12. 11..
//  Copyright (c) 2014년 VALTIO. All rights reserved.
//

#include <stdio.h>
#include <stdlib.h>

int main( int argc, char *argv[] )
{
    
    FILE *fp;
    char path[200];
    
    while (1) {
        fp = popen("./valtio", "r");
        if (fp == NULL) {
            printf("Failed to run command\n" );
            exit(1);
        }
        
        printf("\nRun valtio.\n");
        
        while (fgets(path, sizeof(path)-1, fp) != NULL) {
            printf("%s", path);
        }
        
        pclose(fp);
        
        sleep(3);
    }
    
    return 0;
}