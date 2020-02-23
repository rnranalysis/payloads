#include <stdio.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <stdlib.h>

struct sockaddr_in server;

int bind_time()
{
    int sockfd = socket(AF_INET, SOCK_STREAM, 0);
    if (sockfd == -1)
    {
        return 0;
    }
    else
    {
        server.sin_family = AF_INET;
        server.sin_addr.s_addr = INADDR_ANY;
        server.sin_port = htons( 9999 );
        if (bind(sockfd,(struct sockaddr *)&server , sizeof(server)) == -1)
        {
            return 0;
        }
        else
        {
            listen(sockfd, 3);
            int c = sizeof(struct sockaddr_in);
            struct sockaddr_in client;
            int new_socket = accept(sockfd, (struct sockaddr *)&client, (socklen_t*)&c);
            return 1;
        }
    }
}

int drop_file()
{
    FILE *fptr = fopen("/tmp/2-12-2020.jpg", "w");
    if (fptr > 0)
    {
        char *buf = "SSdtIG5vdCBzdXBlcnN0aXRpb3VzLCBidXQgSSdtIGEgbGl0dGxlIHN0aXRpb3VzLgo=\n";
        fprintf(fptr, "%s", buf);
        fclose(fptr);
        return 1;
    }
    return 0;
}

int persist()
{
    char path[50];
    char *username = getenv("USER");
    if (username != NULL)
    {
        sprintf(path, "%s%s%s", "/home/", username, "/.bashrc");
        char *ptr_path = &path[0];
        FILE *fptr = fopen(ptr_path, "a");
        if (fptr > 0)
        {
            char *bf = "echo SSdtIG5vdCBzdXBlcnN0aXRpb3VzLCBidXQgSSdtIGEgbGl0dGxlIHN0aXRpb3VzLgo=\n";
            fputs(bf, fptr);
            fclose(fptr);
            return 1;
        }
    }
    return 0;
}

int main() {
    drop_file();
    persist();
    bind_time();
    return 0;
}


