#include "lir2.h"

lir2::lir2()
{
    this->typeSens = "???";
    typeMeas = "Миллиметры";
}

 void lir2::DifferentData(double updateInf)         //tolko dlya datchikov tipa ???
 {
     nowDiscr = updateInf;
     diffDicsr = nowDiscr - lastDiscr;
     lastDiscr = nowDiscr;
     if(abs(diffDicsr) > 16777216/2)
         diffDicsr -= sign(diffDicsr)* 16777216;
     dataDiscr += diffDicsr;
 }

 void lir2::Input(double updateInf)
 {
     DifferentData (updateInf);
     data1 = dataDiscr * 0.001;
     data2 = dataDiscr * 0.001;//0.0001;
 }
