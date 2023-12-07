#include <pthread.h>
#include <errno.h>
#include <stdio.h>
#include <unistd.h>

#define MAX_RESOURCES 5
int available_resources = MAX_RESOURCES;
pthread_mutex_t mtx;

int decrease_count(int count)
{
    pthread_mutex_lock(&mtx);
    if(available_resources < count)
    {
        pthread_mutex_unlock(&mtx);
        return -1;
    }
    else
        available_resources -= count;
    printf("Got %d resources, %d remaining\n", count, available_resources);
    pthread_mutex_unlock(&mtx);
    sleep(count);
    return 0;
}

int increase_count(int count)
{
    pthread_mutex_lock(&mtx);
    available_resources += count;
    printf("Released %d resources, %d remaining\n", count, available_resources);
    pthread_mutex_unlock(&mtx);
    return 0;
}

void *
count(void *v)
{
    int* count = (int*) v;
    while(decrease_count(count[0])==-1);
    increase_count(count[0]);
    return NULL;
}

int main()
{
    printf("MAX_RESOURCES=%d\n", available_resources);
    
    pthread_t thr[5];
    int v[5][1] = {{1}, {2}, {2}, {2}, {3}};

    if(pthread_mutex_init(&mtx, NULL)) {
        perror(NULL);
        return errno;
    }

    for(int i = 0; i < 5; i++)
    {
        void *arg = (void *)v[i];
        if(pthread_create(&thr[i], NULL, count, arg))
        {
            perror(NULL);
            return errno;
        }
    }

    for(int i = 0; i < 5; i++)
    {
        void *rez;
        if(pthread_join(thr[i], &rez))
        {
            perror(NULL);
            return errno;
        }
    }

    pthread_mutex_destroy(&mtx);

    return 0;
}