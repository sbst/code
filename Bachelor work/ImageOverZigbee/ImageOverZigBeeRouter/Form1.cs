using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Diagnostics;

// пространство имен для клиента ZigBee
namespace ImageOverZigBeeRouter
{    
    //класс основной формы
    public partial class Form1 : Form
    {
        public static SerialPort ZigUsb;    //объект порта
        byte[] fileBytes;   //массив байт для изображения
        string fileName;    //имя файла
        bool progressThread;    //флаг о том что поток записи запущен
        //конструктор по умолчанию главной формы
        public Form1()
        {
            InitializeComponent();  //инициализация интерфейса
            comboBoxBaud.SelectedIndex = 0; //значение в combobox по умолчанию
            comboBoxFlow.SelectedIndex = 0;
            disconnectToolStripMenuItem.Enabled = false;    //переключение кнопок интерфейса
            startToolStripMenuItem.Enabled = false;
            ZigUsb = new SerialPort();  //инициализируем объект порта

            fileBytes = null;   //инициализируем массив байт
            fileName = String.Empty;
            progressThread = false; //флаг потока записи опущен
        }
        //функция подключения к порту
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ZigUsb.PortName = textBoxPort.Text; //устанавливаем параметры подключения
                if (comboBoxFlow.Text == "Disable")
                    ZigUsb.Handshake = Handshake.None;
                if (comboBoxFlow.Text == "Xon_Xoff")
                    ZigUsb.Handshake = Handshake.XOnXOff;
                if (comboBoxFlow.Text == "Hardware")
                    ZigUsb.Handshake = Handshake.RequestToSend;
                ZigUsb.BaudRate = Int32.Parse(comboBoxBaud.Text);
                ZigUsb.Parity = Parity.None;
                ZigUsb.DataBits = 8;
                ZigUsb.StopBits = StopBits.One;
                ZigUsb.WriteTimeout = 10000;
                ZigUsb.ReadTimeout = 10000;
                ZigUsb.Open();  //открываем порт с заданными параметрами
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is opened" + "\n" + textBoxLog.Text; //записываем информацию в лог
                ZigUsb.DiscardInBuffer();
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT" + "\n" + textBoxLog.Text;
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + Zigbee_Message("AT") + "\n" + textBoxLog.Text;
                //проверяем данные об устройстве
                string responseATN = Zigbee_Message("AT+N");    //отправляем команду
                string[] splitResponseATN = responseATN.Split(','); //читаем ответ с порта и парсим
                if (splitResponseATN[0] == "+N=COO")
                    labelRole.Text = "Role: Coordinator";
                if (splitResponseATN[0] == "+N=FFD")
                    labelRole.Text = "Role: Router";
                if (splitResponseATN[0] == "+N=SED")
                    labelRole.Text = "Role: End device";
                labelChannel.Text = "Channel: " + splitResponseATN[1];
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT+N" + "\n" + textBoxLog.Text;  //выводим данные в лог
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + responseATN + "\n" + textBoxLog.Text;
                //переключаем интерфейс
                buttonConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
                connectToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = true;
            }
            catch
            {
                //в случае ошибки
                MessageBox.Show(textBoxPort.Text + " is unable to connect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //функция для записи и чтения информации с порта передачи с устройством
        static public string Zigbee_Message(string Message)
        {
            ZigUsb.Write(Message + "\r\n"); //добавляем разделитель к сообщению
            try
            {
                ZigUsb.ReadLine();  //выполняем чтение с порта
                return ZigUsb.ReadLine();   //возвращаем значение
            }
            catch
            {
                //в случае если ничего не пришло и сработал таймаут
                MessageBox.Show("Timeout of response. Try again or close DMODE", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "0";
            }
        }
        //функция отключения от порта
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
                buttonConnect.Enabled = true; //переключаем интерфейс
                buttonDisconnect.Enabled = false;
                buttonStart.Enabled = false;
                connectToolStripMenuItem.Enabled = true;
                disconnectToolStripMenuItem.Enabled = false;
                startToolStripMenuItem.Enabled = false;
                labelRole.Text = "Role:";   //выводим данные
                labelChannel.Text = "Channel:";
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is closed" + "\n" + textBoxLog.Text;
                ZigUsb.Close(); //закрываем порт
        }
        //функция открытия изображения
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();  //создаем объект и иинициализируем
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;    //если нажат не ОК то выходим
            try
            {
                using (FileStream StreamFile = new FileStream(openFileDialog1.FileName, FileMode.Open)) //если изображение доступно то
                {
                    fileBytes = new byte[StreamFile.Length];    //инициализируем массив байт
                    StreamFile.Read(fileBytes, 0, fileBytes.Length);    //заполняем
                }
                pictureBox.Load(openFileDialog1.FileName);  //выгружаем в форму
                fileName = openFileDialog1.FileName;    //запоминаем его название
                buttonStart.Enabled = true; //переключаем интерфейс
                startToolStripMenuItem.Enabled = true;
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " \"" + fileName + "\" is loaded for sending" + "\n" + textBoxLog.Text;
            }
            catch 
            {
                //в случае если попытка открыть не изображение
                MessageBox.Show("Unable to load picture", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        //запуск потока, который записывает массив байт в порт передачи
        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonBrowse.Enabled = false;   //переключаем интерфейс
            buttonStart.Enabled = false;
            buttonDisconnect.Enabled = false;
            Zigbee_Message("AT+SCAST:" + fileName + "=" + fileBytes.Length.ToString());   //передаем первое сообщение на Sink о размере и названии файла            
            Thread writeThread = new Thread(ReadDataSerial);    //инициализируем поток и передадим функцию для выполнения
            writeThread.Start();    //запуск потока
        }
        //функция сохранения лога в файл
        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();   //создаем и инициализируем диалог
            saveDialog.Filter = "Text document (*.txt)|*.txt";  //фильтр расширения файла
            saveDialog.FileName = "log.txt";
            saveDialog.DefaultExt = "txt";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) //если выбрали куда сохранять и нажали ОК то
            {
                StreamWriter sw = new StreamWriter(saveDialog.FileName);    //создаем и инициализируем объект записи
                sw.WriteLine("ImageOverZigBee coordinator: " + textBoxPort.Text + " " + comboBoxBaud.Text + " " + comboBoxFlow.Text);
                for (int i = textBoxLog.Lines.Length - 1; i > -1; i--)  //записываем данные из лога с конца
                    sw.WriteLine(textBoxLog.Lines[i]);
                sw.Close();
            }
        }
        //событие при закрытии главной формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) && (progressThread == true)) //если форму закрывает пользователь и поток еще работает
            {
                e.Cancel = true;    //отменяем закрытие формы
                MessageBox.Show("Wait until progress will be done", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else    //иначе освобождаем ресурс порта
            { 
                ZigUsb.Dispose();
            }
        }
        //функции и делегаты относящиеся к потоку передачи
        delegate void Inform(int data); //делегат записи данных в progressbar
        delegate void InformButton(bool data);      //делегат для контроля интерфейса
        delegate void InformLog(string data);       //делегат для передачи строки в лог
        //функция передачи изображения в сеть ZigBee
        public void ReadDataSerial()
        {
            textBoxLog.Invoke(new InformLog((data) => textBoxLog.Text = data + textBoxLog.Text), DateTime.Now.ToString("HH:mm:ss") + " Writing thread is started" + "\n");
            Invoke(new InformButton((data) => progressThread = data), true);    //запись в лог и изменения состояния флага о запущенном потоке
            int iteration = fileBytes.Length / 82;  //вычисляем количество итераций, разделив размер массива байт на размер пакета
            progressBar.Invoke(new Inform((data) => progressBar.Maximum = data), iteration);    //переключаем интерфейс
            progressBar.Invoke(new Inform((data) => progressBar.Value = data), 0);
            int progress = 0;
            int position = 0;
            int i = 0;
            do  //цикл записи
            {
                progressBar.Invoke(new Inform((data) => progressBar.Value = data), progress++); //изменяем состояние progressbar
                ZigUsb.Write("AT+SCASTB:52\r\n");   //передаем на Sink сообщение о размере пакета
                ZigUsb.Write(fileBytes, position, 82);  //записываем в порт массив байт начиная с позиции - position                
                if (!ErrorChk())    //если пакет доставлен
                {
                    position += 82; //сдвигаем позицию записи
                    i++;            //увеличиваем количество отправленных пакетов
                }
            } while (i < iteration);
            //последняя итерация для записи оставшихся пакетов. Например в случае если размер файла 256/82=3.1 основной цикл - 3, остаток дозаписывает 0.1
            int Remain = fileBytes.Length - position;   //вычисляем остаток
            while (!ErrorChk()) //пока пакет не доставлен
            {                
                ZigUsb.Write("AT+SCASTB:" + (Remain + 1).ToString("X") + "\r\n");   //отправляем запрос на передачу с оставшимся размером пакета
                ZigUsb.Write(fileBytes, position, Remain);  //отправляем данные c позиции position размером Remain
            }
            //выводим информацию о том что передача завершилась и переключаем интерфейс
            textBoxLog.Invoke(new InformLog((data) => textBoxLog.Text = data + textBoxLog.Text), DateTime.Now.ToString("HH:mm:ss") + " Writing thread is finished" + "\n");
            Invoke(new InformButton((data) => progressThread = data), false);
            buttonStart.Invoke(new InformButton((data) => buttonStart.Enabled = data), true);
            buttonBrowse.Invoke(new InformButton((data) => buttonBrowse.Enabled = data), true);
            buttonDisconnect.Invoke(new InformButton((data) => buttonDisconnect.Enabled = data), true);
        }
        //функция проверки доставки пакета
        static public bool ErrorChk()
        {
            string offsetMessage = ZigUsb.ReadLine();   //считываем ответ из порта
            do //пока ответ не начинается c SEQ считываем с порта
            {
                offsetMessage = ZigUsb.ReadLine();
            }
            while ((offsetMessage[0] != 'S') && (offsetMessage[1] != 'E') && (offsetMessage[2] != 'Q'));
            string[] seq = offsetMessage.Split(':');    //парсим и записываем в переменную
            do  //пока ответ не начинается с ACK считываем с порта
            {
                offsetMessage = ZigUsb.ReadLine();
            }
            while (((offsetMessage[0] != 'A') && (offsetMessage[0] != 'N')) && ((offsetMessage[0] != 'C') && (offsetMessage[0] != 'A')));
            string[] ack = offsetMessage.Split(':');    //парсим и записываем в переменную
            //с порта может прийти ACK - ответ устройства и NACK - устройство указанное при отправки данных недоступно
            if (ack[0] != "ACK")    //если с порта пришло не ACK
                return true;    //не прошло проверку
            else                //иначе
                if (seq[1] != ack[1])   //если seq не равно ack
                    return true;        //не прошло проверку
                else                    //иначе
                    return false;       //seq = ack - успешно доставлено
        }

    }
}
