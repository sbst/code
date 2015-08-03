#include "mainwindow.h"
#include "ui_mainwindow.h"
#include "qstring.h"
#include "qtimer.h"
MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    QWidget::showMaximized();

    if(!Open_Driver_LIR_X(0, handle))   //otkr kontroller
    {
        QMessageBox::information(0, "Message", "Контроллер не найден!");
    }
    timer = new QTimer(this);
    timer->setInterval(20);//20
    timer->setTimerType(Qt::PreciseTimer);

    connect(timer, SIGNAL(timeout()), this, SLOT(onTimerTick()));
    timerFile = new QTimer(this);
    timerFile->setTimerType(Qt::PreciseTimer);
    connect(timerFile, SIGNAL(timeout()) , this, SLOT(onTimerTickFile()));
    QDateTime dateTimeExp = QDateTime::currentDateTime();
    FolderName = dateTimeExp.toString("dd.MM.yyyy");

    OpenGraphWindow = new PainterPathWidget(2);
    projectPath = QDir::currentPath();

    CreateTabAllPainter();

    SmallGraphMain = new PainterPathWidget(1, ui->graphicsViewSmallMain);
    GraphScale(SmallGraphMain);
    SmallGraphMain->show();

    for(int i = 0; i < 4; i++)
        freeSens[i] = i+1;

    CountStr = 1;

    //nomer experimenta
    filedat.setFileName("exp.dat");
    QString temp;

    if (filedat.open (QIODevice::ReadOnly | QIODevice::Text))   //esli fail sushestvuet
    {
        outdat.setDevice(&filedat);
        temp = outdat.readLine();
        if (temp == dateTimeExp.toString("dd.MM.yyyy"))        //esli v nem tekushaya data
        {
            QDir().mkdir(FolderName);
            NumberExperiment = outdat.readLine().toInt();
        }
        else                                                // esli v nem staraya data
        {
                QDir().mkdir(FolderName);
                NumberExperiment = 1;
        }

        filedat.close();
        ui->labelExp->setText(tr("Эксперимент №") + QString::number(NumberExperiment));
    }
    else            // esli fail nesushestvuet
    {
        ConfigureDlg.exec();
        NumberExperiment = ConfigureDlg.expNum;
        ui->labelExp->setText(tr("Эксперимент №") + QString::number(NumberExperiment));
        QDir().mkdir(FolderName);
        filedat.close();
    }
}
//..
void MainWindow::on_pushButtonStart_clicked()
{
    if (listSens.length() != 0)
    {
        int i;
        int tempCountGraph;

        QDateTime dateTime = QDateTime::currentDateTime();
        QDir::setCurrent(FolderName);
        if (SettingsWindow.Filename == "<dateNumExp>")
            file.setFileName(dateTime.toString("hh_mmN")+ QString::number(NumberExperiment) + ".log");
        else
            file.setFileName(SettingsWindow.Filename + ".log");


        timerFile->setInterval(SettingsWindow.IntWriteFile);

        // gotovim pervuy discretu i sozdaem graphiki
        for (i = 0; i < listSens.length(); i++)
        {
            do
            {
                UpdateData_LIR(handle, i+1, ident, stateByte);
            }
            while(ident!=0);

            listSens[i]->PrepareLastDiscr(UpdateData_LIR(handle, listSens[i]->port, ident, stateByte));

            GraphScale(listGraph[i]);
            GraphScale(FG.listSmallGraph[i]);

            listGraph[i]->stopData();

            FG.listSmallGraph[i]->stopData();

        }

        // sozdaem graphiki vse-v-odnom
        if (listSens.length() > 4)
            tempCountGraph = 4;
        else
            tempCountGraph = listSens.length();

        GraphScale(AllGraph, tempCountGraph);

        AllGraph->stopData();

        // sozdaem melkii grafik v tabe s dannimi
        GraphScale(SmallGraphMain);
        SmallGraphMain->stopData();
        CheckSmallGraphMain = 0;


        //startuem timeri
        CurSec.start();
        timer->start();
        timerFile->start();

        ui->pushButtonStart->setEnabled(false);
        ui->pushButtonAllGraph->setEnabled(true);
        ui->pushButtonAdd->setEnabled(false);
        ui->StopButton->setEnabled(true);

        //zapolnyaem legendi
        for (int i = 0; i < listSens.length(); i++)
            AllGraph->legend[i] =  tr(listSens[i]->typeSens) + "(" +
                        QString::number(listSens[i]->port) + ")";

        for (int i = 0; i < listSens.length(); i++)
            FG.listSmallGraph[i]->legend[0] = tr(listSens[i]->typeSens) + "(" +
                        QString::number(listSens[i]->port) + ")";

    }
    else
        QMessageBox::information(0, "Message", tr("Не добавлено ни одного датчика! Эксперимент не начат"));
}

void MainWindow::onTimerTickFile()
{
    out<<(CurSec.elapsed()+0.)/1000<<"\t\t\t";
    for (int i = 0; i < listSens.length(); i++)
        out<<listSens[i]->data2<<"\t\t\t";
    out<<endl;
    CountStr++;
}
//..

void MainWindow::AddSensor(QString type, int port)
{
    //dobavlyaem v massiv novii datchik ishodya iz tipa
    if (type == tr("???"))
        listSens << new Lir1();

    if (type == tr("???"))
        listSens << new lir2();

    if (type == tr("???"))
        listSens << new AnotherLir();

    listSens[listSens.length()-1]->port = port;

    listLabel << new QLabel(ui->tabData);  //podpis
    listLabel[listLabel.length()-1]->setParent(ui->tabData);
    ui->gridLayoutData->addWidget(listLabel[listLabel.length()-1],ui->gridLayoutData->rowCount(),0);//1
    listLabel[listLabel.length()-1]->setText(tr(listSens[listLabel.length()-1]->typeSens) + "(" +
                                             QString::number(port) + ")");
    listLabel[listLabel.length()-1]->show();

    listEdit << new QLineEdit(ui->tabData); //dlya dannih s datchika
    listEdit[listEdit.length()-1]->setParent(ui->tabData);
    listEdit[listEdit.length()-1]->setReadOnly(true);
    ui->gridLayoutData->addWidget(listEdit[listEdit.length()-1],ui->gridLayoutData->rowCount()-1,1);//2
    listEdit[listEdit.length()-1]->show();

    listGraph << new PainterPathWidget;
    ui->tabWidget->addTab(listGraph[listGraph.length()-1], tr(listSens[listLabel.length()-1]->typeSens) + "(" +
                          QString::number(port) + ")");

    ui->comboBoxSmallMain->addItem(tr(listSens[listLabel.length()-1]->typeSens) + "(" +
                                   QString::number(port) + ")");

    FG.CreateTabSmallPainter(listSens[listSens.length()-1]->typeMeas);
    GraphScale(FG.listSmallGraph[FG.listSmallGraph.length()-1]);

    listGraph[listGraph.length()-1]->PrepareLabelOY(listSens[listSens.length()-1]->typeMeas);
    GraphScale(listGraph[listGraph.length()-1]);
}

//..