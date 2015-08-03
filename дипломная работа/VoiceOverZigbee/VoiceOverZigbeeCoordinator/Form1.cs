using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;
//пространство имен для сервера по передачи голоса
namespace VoiceOverZigbeeCoordinator
{
    //главная форма с интерфейсом и основными функциями
    public partial class Form1 : Form
    {
        static public SerialPort ZigUsb;    //объект порта
        DirectSoundOut output;              //объект входящего голоса
        BufferedWaveProvider bufferStream;  //буффер вывода полученной информации на спикер
        static private VoiceOverZigbee.Codec.ALawChatCodec voiceCodec;  //объект кодека сжатия
        bool connected;            //флаг чтения данных
        bool flagThread;           //флаг работы потока
        ListeningThread listenThread;   //поток считывания данных
        System.Windows.Forms.Timer timerStat;   //таймер для статистики
        int byteSent;               //количество полученных байт
        int secSent;                //количество затраченных секунд на получение
        DateTime startTime;         //объект для временной метки
        //конструктор по умолчанию
        public Form1()
        {
            InitializeComponent();  //инициализируем интерфейс
            comboBoxBaud.SelectedIndex = 0; //значения combobox по умолчанию
            comboBoxFlow.SelectedIndex = 0;
            disconnectToolStripMenuItem.Enabled = false;    //переключаем интерфейс
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            ZigUsb = new SerialPort();  //инициализируем объект порта
            voiceCodec = new VoiceOverZigbee.Codec.ALawChatCodec(); //инициализируем объект кодека
            output = new DirectSoundOut();  //инициализируем объект для считывания голоса
            bufferStream = new BufferedWaveProvider(voiceCodec.RecordFormat);   //задаем формат буферу воспроизведения голоса, он соответствует формату кодека
            output.Init(bufferStream);  //задаем соответствие буфера - объекту воспроизведения
            listenThread = new ListeningThread(new ThreadStart(Listening)); //инициализируем поток делегатом с функцией на выполнение в качестве аргумента
            connected = false;  //флаг о передачи данных
            flagThread = false; //флаг о потоке
        }
        //подключение к порту
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                ZigUsb.PortName = textBoxPort.Text; //задаем параметры подключения к порту
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
                ZigUsb.WriteTimeout = 1000;
                ZigUsb.ReadTimeout = 1000;
                ZigUsb.Open();  //открываем
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is opened" + "\n" + textBoxLog.Text;   //запись в лог
                ZigUsb.DiscardInBuffer();   //если есть данные в порту - очищаем
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT" + "\n" + textBoxLog.Text;
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + Zigbee_Message("AT") + "\n" + textBoxLog.Text;
                //отправляем команду, чтобы узнать о состоянии устройства           
                string responseATN = Zigbee_Message("AT+N");
                string[] splitResponseATN = responseATN.Split(','); //считываем и парсим ответ
                if (splitResponseATN[0] == "+N=COO")
                    labelRole.Text = "Role: Coordinator";
                if (splitResponseATN[0] == "+N=FFD")
                    labelRole.Text = "Role: Router";
                if (splitResponseATN[0] == "+N=SED")
                    labelRole.Text = "Role: End device";
                labelChannel.Text = "Channel: " + splitResponseATN[1];
                //переключаем интерфейс
                buttonConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
                checkBoxStatistic.Enabled = true;
                connectToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
            }
            catch
            {
                //в случае ошибки
                MessageBox.Show(textBoxPort.Text + " is unreachable", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //функция начала считывания
        private void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;    //переключение интерфейса
            buttonStop.Enabled = true;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            saveLogToolStripMenuItem.Enabled = false;
            checkBoxStatistic.Enabled = false;
            labelStream.Text = "Listening (started)";
            if (flagThread == false)    //если поток не запускался
            {
                listenThread.Start();   //стартуем поток
                flagThread = true;      //поднимаем флаг потока
            }
            else                        //иначе
                listenThread.Resume();  //возобновляем поток
            connected = true;           //поднимаем флаг о считывании данных
            if (checkBoxStatistic.Checked == true)  //если запущен
            {
                if (timerStat == null)  //если таймер ранее не инициализирован
                {
                    timerStat = new System.Windows.Forms.Timer();   //инициализируем
                    timerStat.Interval = 1000;  //задаем шаг таймера в 1 сек
                    timerStat.Tick += timerStat_Tick;   //связываем callback с шагом
                }
                startTime = DateTime.Now;   //запоминаем временную метку
                byteSent = 0;   //обнуляем счетчики
                secSent = 0;
                timerStat.Start();  //стартуем таймер
            }
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Listening thread started" + "\n" + textBoxLog.Text;
        }
        //остановка считывания данных
        private void buttonStop_Click(object sender, EventArgs e)
        {         
            labelStream.Text = "Listening (stopped)";
            connected = false;      //опускаем флаг считывания
            listenThread.Suspend(); //поток приостанавливается            
            if (checkBoxStatistic.Checked == true)  //если ведется запись статистики
            {
                timerStat.Stop();   //останавливаем таймер
                FormScore formStat = new FormScore();   //создаем форму для оценки и инициализируем
                formStat.ShowDialog();  //выводим как диалог
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Score of session: " + formStat.Data + "\n" + textBoxLog.Text;
            }
            Thread.Sleep(500);  //ждем пол секунды для восстановления модуля после передачи
            ZigUsb.ReadExisting();  //очищаем буферы и перключаем интерфейс
            ZigUsb.DiscardInBuffer();
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            checkBoxStatistic.Enabled = true;
            startToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.Enabled = false;
            saveLogToolStripMenuItem.Enabled = true;
        }

        //функция записи строки на передачу в открытый порт
        static public string Zigbee_Message(string Message)
        {
            ZigUsb.Write(Message + "\r\n"); //добавляем разделитель
            try
            {
                ZigUsb.ReadLine();  //считываем
                return ZigUsb.ReadLine();
            }
            catch
            {
                //в случае ошибки возвращаем 0
                MessageBox.Show("Timeout of response. Try again or close DMODE", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "0";
            }
        }
        //событие закрытия формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Clean Up..." + "\n" + textBoxLog.Text;
            listenThread.Dispose(); //освобождаем ресурсы потока
            if (ZigUsb.IsOpen)      //если порт открыт
            {
                Zigbee_Message("+++");  //закрываем прозрачный канал
                ZigUsb.Close();         //закрываем порт
            }
            ZigUsb.Dispose();       //особождаем ресурсы порта
            timerStat.Stop();       //останавливаем таймер
            timerStat.Dispose();    //освобождаем ресурсы таймера
        }
        //при отключении от порта
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = true;   //перключаем интерфейс и выводим информацию
            buttonDisconnect.Enabled = false;
            buttonStart.Enabled = false;
            buttonStop.Enabled = false;
            checkBoxStatistic.Enabled = false;
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            labelRole.Text = "Role:";
            labelChannel.Text = "Channel:";
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is closed" + "\n" + textBoxLog.Text;
            ZigUsb.Close(); //закрываем порт
        }
        //сохранение лога в файл
        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();   //создаем объект и инициализируем диалогом
            saveDialog.Filter = "Text document (*.txt)|*.txt";  //применяем фильтры
            saveDialog.FileName = "log.txt";
            saveDialog.DefaultExt = "txt";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)    //если диалог закрыт успешно
            {   
                StreamWriter sw = new StreamWriter(saveDialog.FileName);
                sw.WriteLine("VoiceOverZigBee coordinator: " + textBoxPort.Text + " " + comboBoxBaud.Text + " " + comboBoxFlow.Text);
                for (int i = textBoxLog.Lines.Length - 1; i > -1; i--)  //записываем лог с конца
                    sw.WriteLine(textBoxLog.Lines[i]);
                sw.Close();
            }
        }
        //функция потока прослушивания
        private void Listening()
        {
            listenThread.Wait();    //точка остановки и восстановления потока
            connected = true;       //флаг записи данных
            byte[] data = new byte[80]; //создаем массив для чтения
            while (connected == true)   //пока флаг не опущен
            {                 
                ZigUsb.Read(data, 0, 80);   //считываем полностью в массив              
                byteSent += ZigUsb.BytesToRead; //считаем для статистики
                byte[] decodedData = voiceCodec.Decode(data, 0, 80);    //расжимаем данные в массив
                bufferStream.AddSamples(decodedData, 0, decodedData.Length);    //записываем данные в буфер воспроизведения
                output.Play();  //воспроизводим
            }
            
        }
        //callback таймера для статистики
        void timerStat_Tick(object sender, EventArgs e)
        {
            var timeDiff = DateTime.Now - startTime;    //высчитываем затраченное время
            secSent++;  //считаем время затраченное на получение
            int loss = 100 - (byteSent * 100 / 8000);   //считаем потери 4000 - количество байт за 1 секунду без потерь
            double bytesPerSecond = byteSent / timeDiff.TotalSeconds;   //считаем количество байт в секунду
            textBoxLog.Text = secSent + " second: " + loss + "% loss, " + bytesPerSecond.ToString("0") + " Bps" + "\n" + textBoxLog.Text;
            byteSent = 0;
        }

    }
    //класс потока
    public class ListeningThread : IDisposable
    {
        private Thread m_Thread;    //объект потока
        private ManualResetEvent m_EvSuspend;   //объект для контроля потока
        //конструктор с параметром. в качестве параметра ожидается делегат функции для выполнения в потоке
        public ListeningThread(ThreadStart start)
        {
            m_EvSuspend = new ManualResetEvent(true);   //инициализируем объект контроля потока
            m_Thread = new Thread(start);               //инициализируем поток с функцией на выполнение
        }
        //метод освобождения ресурсов
        public void Dispose()
        {
            Resume();   //возобновляем поток
            m_Thread.Join();    //блокируем его выполнение до завершения главного потока
            m_EvSuspend.Close();    //завершаем объект контроля потока
            m_EvSuspend.Dispose();
        }
        //метод запуска потока
        public void Start()
        {
            m_Thread.Start();
        }
        //метод приостановки потока
        public void Suspend()
        {
            m_EvSuspend.Reset();
        }
        //метод возобновления потока
        public void Resume()
        {
            m_EvSuspend.Set();
        }
        //метод для точки ожидания потока
        public void Wait()
        {
            m_EvSuspend.WaitOne();
        }
    }
}
