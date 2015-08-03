#ifndef LIR2_H
#define LIR2_H
#include "sensor.h"

class lir2 : public sensor
{
public:
    lir2();
    virtual void Input(double);
    void DifferentData(double updateInf);
};

#endif // LIR2_H
