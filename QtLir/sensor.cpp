#include "sensor.h"
void sensor::Input(double){}

sensor::sensor()
{
    lastDiscr = 0;
    nowDiscr = 0;
    diffDicsr = 0;
    dataDiscr = 0;
    data1 = 0;
    data2 = 0;
    port = 0;
}


void sensor::PrepareLastDiscr(double updateInf)
{
    lastDiscr = updateInf;
}

int sensor::sign(int a)
{
    if(a>0)
        return 1;
    else
    {
        if(a == 0)
            return 0;
        else
            return -1;
    }
}
