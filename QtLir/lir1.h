#ifndef LIR1_H
#define LIR1_H
#include "sensor.h"

class Lir1 : public sensor
{
protected:
    Lir1();
    virtual void Input(double); //func dlya vichisleniya data2/data1 dlya etogo tipa datchikov
    void DifferentData(double updateInf);

};

#endif 
