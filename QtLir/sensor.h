#ifndef SENSOR_H
#define SENSOR_H
#include <QMainWindow>
#include <QDebug>
#include <QtGui>
#include <QWidget>
#include <QGridLayout>
#include <QApplication>
#include <QMessageBox>
#include <QString>

class sensor
{
public:
    sensor();

    char* typeSens;
    char* typeMeas;

    int lastDiscr;
    int nowDiscr;
    int diffDicsr;

    double dataDiscr;
    double data1;
    long long int data2;

    int port;
	
protected:
    virtual void Input(double);      //func dlya zapolneniya nowDiscr
    void PrepareLastDiscr(double);   //func dlya lastDiscr pri pike po knopke start

private:
    int sign(int);
};

#endif // SENSOR_H
