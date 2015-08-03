#ifndef ANOTHERLIR_H
#define ANOTHERLIR_H
#include "sensor.h"

class AnotherLir : public sensor
{
public:
    AnotherLir();
    virtual void Input(double); //func dlya vichisleniya dataMin/dataGrad dlya etogo tipa datchikov

private:
    sensor* obj;
};

#endif // ANOTHERLIR_H
