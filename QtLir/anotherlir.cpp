#include "anotherlir.h"

AnotherLir::AnotherLir()
{
    this->typeSens = "Другой";
    typeMeas = "";
}

void AnotherLir::Input(double updateInf)
{
    nowDiscr = updateInf;
    diffDicsr = nowDiscr - lastDiscr;
    lastDiscr = nowDiscr;
    if(abs(diffDicsr) > 16777216/2)
        diffDicsr -= sign(diffDicsr)* 16777216;
    dataDiscr += diffDicsr;
    data1 = dataDiscr * 0.018;
    data2 = dataDiscr * 1.08;
}
