using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using System.Drawing;

// пространство имен для сервера ZigBee
namespace ImageOverZigBeeCoordinator
{
    public partial class Form1 : Form
    {
        //реализуется интерфейс главной формы и основные функции
        static public SerialPort ZigUsb;    //основной объект для порта
        bool progressThread;                //флаг того что поток чтения запущен

        int byteSent;                       //подсчет полученных байтов для статистики
        int secSent;                        //подсчет секунд на получение для статистики
        //конструктор по умолчанию главной формы
        public Form1()
        {
            InitializeComponent();          //инициализация интерфейса
            comboBoxBaud.SelectedIndex = 0; //обнуляем показатели в combobox
            comboBoxFlow.SelectedIndex = 0;
            disconnectToolStripMenuItem.Enabled = false;  //переключаем доступность кнопок в интерфейсе
            startToolStripMenuItem.Enabled = false;
            ZigUsb = new SerialPort();      //инициализируем объект порта
            progressThread = false;         //флаг потока - не запущен
        }
        //событие обрабатывающее подключение к порту
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
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is opened" + "\n" + textBoxLog.Text;   //записываем данные в лог
                ZigUsb.DiscardInBuffer();   //обнуляем приемный буффер порта
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT" + "\n" + textBoxLog.Text;
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + Zigbee_Message("AT") + "\n" + textBoxLog.Text;
                //проверяем состояние устройства ZigBee
                string responseATN = Zigbee_Message("AT+N");    //посылаем команду
                string[] splitResponseATN = responseATN.Split(','); //парсим полученную строку
                if (splitResponseATN[0] == "+N=COO")
                    labelRole.Text = "Role: Coordinator";
                if (splitResponseATN[0] == "+N=FFD")
                    labelRole.Text = "Role: Router";
                if (splitResponseATN[0] == "+N=SED")
                    labelRole.Text = "Role: End device";
                labelChannel.Text = "Channel: " + splitResponseATN[1];  //выводим информацию
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT+N" + "\n" + textBoxLog.Text;
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + responseATN + "\n" + textBoxLog.Text;
                //подключение закончено, переключаем интерфейс
                buttonConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
                buttonStart.Enabled = true;
                connectToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = true;
            }
            catch
            {
                //ошибка при подключении к порту
                MessageBox.Show(textBoxPort.Text + " is unreachable", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //функция для запуска прослушивания сети
        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;    //переключаем интерфейс
            startToolStripMenuItem.Enabled = true;
            labelStream.Text = "Listening (started)";
            Thread listenThread = new Thread(Listening);    //инициализируем объект потока передав функцию на выполнение
            listenThread.Start();   //запускаем поток
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
        //событие при закрытии главной формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) && (progressThread == true)) //если форму закрывает пользователь и поток еще работает
            {
                e.Cancel = true;    //отменяем закрытие формы
                MessageBox.Show("Wait until progress will be done", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else    //иначе освобождаем ресурс порта
            ZigUsb.Dispose();
        }
        //функция отключения от порта
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = true;   //переключаем доступность интерфейса и выподим сообщение
            buttonDisconnect.Enabled = false;
            buttonStart.Enabled = false;
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            labelRole.Text = "Role:";
            labelChannel.Text = "Channel:";
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is closed" + "\n" + textBoxLog.Text;
            ZigUsb.Close(); //закрываем порт
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
        //функции и делегаты относящиеся к потоку прослушивания
        delegate void Inform(int data); //делегат записи данных в progressbar
        delegate void InformPicture(Bitmap data);   //делегат передачи массива байтов для вывода картинки в интерфейс
        delegate void InformButton(bool data);      //делегат для контроля интерфейса
        delegate void InformLog(string data);       //делегат для передачи строки в лог
        //функция прослушивания сети и чтения изображения
        private void Listening()
        {
            var startTime = DateTime.Now;   //снимаем временную метку начала и записываем ее в лог
            textBoxLog.Invoke(new InformLog((data) => textBoxLog.Text = data + textBoxLog.Text), DateTime.Now.ToString("HH:mm:ss") + " Listening thread is started" + "\n");
            Invoke(new InformButton((data) => progressThread = data), true);    //поднимаем флаг того что поток запущен
            ZigUsb.DiscardInBuffer();
            string[] tempMessage = ZigUsb.ReadLine().Split('=');    //прослушиваем сеть и разбиваем полученную строку
            string fileName = tempMessage[0];   //первая часть строки содержит имя передаваемого файла
            int fileSize = Int32.Parse(tempMessage[1]); //вторая часть строки содержит размер передаваемого файла
            byte[] bytesFile = new byte[fileSize];  //создаем массив байт, согласно полученному размеру
            int position = 0;   //обнуляем позицию фрагмента получения
            int progress = 0;   //обнуляем прогресс получения
            int iteration = fileSize / 82;  //подсчитываем количество итераций для получения, разбив размер файла на размер пакета
            buttonStart.Invoke(new InformButton((data) => buttonStart.Enabled = data), false);  //переключаем интерфейс
            buttonDisconnect.Invoke(new InformButton((data) => buttonDisconnect.Enabled = data), false);
            progressBar.Invoke(new Inform((data) => progressBar.Maximum = data), iteration);
            progressBar.Invoke(new Inform((data) => progressBar.Value = data), 0);
            for (int i = 0; i < iteration; i++) //запускаем цикл прослушивания
            {
                ZigUsb.Read(bytesFile, position, 82);   //считываем первый фрагмент и записываем его в массив начиная с position размером в 82 байта
                progressBar.Invoke(new Inform((data) => progressBar.Value = data), progress++); //заполняем progressbar
                position += 82; //смещаем позицию записи
            }
            //последняя итерация для записи оставшихся пакетов. Например в случае если размер файла 256/82=3.1 основной цикл - 3, остаток дозаписывает 0.1
            int Remain = bytesFile.Length - position;   //подсчитываем остаток   
            ZigUsb.Read(bytesFile, position, Remain);   //считываем строку с position размером в высчитанный остаток
            var timeDiff = DateTime.Now - startTime;    //считываем временную метку окончания прослушивания
            if (fileSize == bytesFile.Length)   //проверяем на совпадение размеров массивов байт
                pictureBox.Invoke(new InformPicture((data) => pictureBox.Image = data), ByteToImage(bytesFile));    //выводим картинку в интерфейс
            else
                //выводим сообщение об ошибке
                MessageBox.Show("Real length of received bytes is different than expected. Picture is corrupted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //переключаем интерфейс
            buttonStart.Invoke(new InformButton((data) => buttonStart.Enabled = data), true);
            buttonDisconnect.Invoke(new InformButton((data) => buttonDisconnect.Enabled = data), true);
            textBoxLog.Invoke(new InformLog((data) => textBoxLog.Text = data + textBoxLog.Text), DateTime.Now.ToString("HH:mm:ss") + " Reading thread is finished" + "\n");
            textBoxLog.Invoke(new InformLog((data) => textBoxLog.Text = data + textBoxLog.Text), DateTime.Now.ToString("HH:mm:ss") + " File: "+fileName+" "+fileSize+" bits received in "+ timeDiff.Seconds.ToString()+" sec." + timeDiff.Milliseconds.ToString() + " msec." + "\n");
            labelStream.Invoke(new InformLog((data) => labelStream.Text = data), "Listening (stopped)");
            Invoke(new InformButton((data) => progressThread = data), false);
        }
        //преобразование картинки из массива байт в объект Bitmap для вывода в интерфейс
        public static Bitmap ByteToImage(byte[] blob)
        {
            //создаем объект и инициализируем
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length)); //конвертируем
            Bitmap bm = new Bitmap(mStream, false); //создаем объект Bitmap и инициализируем конвертированным массивом
            mStream.Dispose();  //освобождаем память
            return bm;  //возвращаем объект
        }
    }
}
