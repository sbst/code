#include "lir1.h"

Lir1::Lir1()
{
    typeSens =  "???";
    typeMeas = "Минуты";
}

void Lir1::DifferentData(double updateInf)         //tolko dlya datchikov tipa ???
{
    nowDiscr = updateInf;
    diffDicsr = nowDiscr - lastDiscr;
    lastDiscr = nowDiscr;
    if(abs(diffDicsr) > 16777216/2)
        diffDicsr -= sign(diffDicsr)* 16777216;
    dataDiscr += diffDicsr;
}

void Lir1::Input(double updateInf)
{
    DifferentData(updateInf);
    data1 = dataDiscr * 0.01125;
    data2 = dataDiscr * 0.675;
}

