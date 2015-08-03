using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO.Ports;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;
//пространство имен для клиента по передачи голоса
namespace VoiceOverZigbee
{
    //главная форма с интерфейсом и основными функциями
    public partial class Form1 : Form
    {
        static public SerialPort ZigUsb;    //объект порта        
        WaveIn input;   //объект для входящего голоса
        static private VoiceOverZigbee.Codec.ALawChatCodec voiceCodec;  //объект кодека
        //конструктор по умолчанию
        public Form1()
        {
            InitializeComponent();  //инициализация интерфейса
            comboBoxBaud.SelectedIndex = 0; //значения combobox по умолчанию
            comboBoxFlow.SelectedIndex = 0;
            disconnectToolStripMenuItem.Enabled = false;    //переключение интерфейса
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            ZigUsb = new SerialPort();  //инициализация объекта порта
            voiceCodec = new VoiceOverZigbee.Codec.ALawChatCodec(); //инициализация кодека
        }
        //функция подключения к порту
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
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + Zigbee_Message("AT") +"\n" + textBoxLog.Text;
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
                //выводим информацию и переключаем интерфейс
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT+N" + "\n" + textBoxLog.Text;
                textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + responseATN + "\n" + textBoxLog.Text;
                buttonConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
                connectToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = true;
                startToolStripMenuItem.Enabled = true;
                stopToolStripMenuItem.Enabled = false;
            }
            catch
            {
                // в случае ошибки
                MessageBox.Show(textBoxPort.Text + " is unreachable", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        //функция начала передачи
        private void buttonStart_Click(object sender, EventArgs e)
        {
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: AT+DMODE:0000" + "\n" + textBoxLog.Text; //записываем информацию
            string responseDMODE = Zigbee_Message("AT+DMODE:0000"); //передаем команду на координатор для запроса открытия прозрачного канала
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + responseDMODE + "\n" + textBoxLog.Text;
            buttonStart.Enabled = false;    //переключаем интерфейс
            buttonStop.Enabled = true;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            saveLogToolStripMenuItem.Enabled = false;
            labelStream.Text = "Streaming (started)";
            input = new WaveIn();   //инициализируем объект для входящего голоса
            input.DataAvailable += Voice_Input; //задаем callback при записи голоса
            input.BufferMilliseconds = 10;  //размер буффера, иными словами каждые 10 секунд будет выполнятся callback
            input.WaveFormat = voiceCodec.RecordFormat; //задаем для объекта записи формат записи, оно соответствует формату кодека            
            input.StartRecording(); //запускаем запись голоса            
        }
        //функция остановки передачи
        private void buttonStop_Click(object sender, EventArgs e)
        {
            input.StopRecording();  //останавливаем запись голоса
            input = null;           //обнуляем
            ZigUsb.BaseStream.FlushAsync(); //очищаем буферы записи данных
            ZigUsb.DiscardOutBuffer();            
            ZigUsb.DiscardInBuffer();
            Thread.Sleep(500);  //ждем пол секунды для корректной работы модуля
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Query: +++" + "\n" + textBoxLog.Text;   //выводим информацию
            string responseDMODE = Zigbee_Message("+++");   //закрываем прозрачный канал
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Response: " + responseDMODE + "\n" + textBoxLog.Text;
            labelStream.Text = "Streaming (stopped)";
            buttonStart.Enabled = true; //переключаем интерфейс
            buttonStop.Enabled = false;
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
        //событие при закрытии главного
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " Clean Up..." + "\n" + textBoxLog.Text;  //выводим информацию
            if (ZigUsb.IsOpen)  //если порт открыт
            {
                Zigbee_Message("+++");  //закрываем прозрачный канал
                ZigUsb.Close(); //закрываем порт
            }                
            ZigUsb.Dispose();   //освобождаем ресурсы
        }
        //при отключении от порта передачи
        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = true;   //переключаем интерфейс
            buttonDisconnect.Enabled = false;
            buttonStart.Enabled = false;
            buttonStop.Enabled = false;
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = false;
            labelRole.Text = "Role:";
            labelChannel.Text = "Channel:";
            textBoxLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + textBoxPort.Text + " is closed" + "\n" + textBoxLog.Text;
            ZigUsb.Close(); //закрываем порт
        }
        //функция сохранения лога в файл
        private void saveLogToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();   //создаем и инициализируем объект
            saveDialog.Filter = "Text document (*.txt)|*.txt";  //применяем фильтры
            saveDialog.FileName = "log.txt";
            saveDialog.DefaultExt = "txt";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)    //если файл выбран
            {
                StreamWriter sw = new StreamWriter(saveDialog.FileName);
                sw.WriteLine("VoiceOverZigBee router: " + textBoxPort + " " + comboBoxBaud + " " + comboBoxFlow);
                for (int i = textBoxLog.Lines.Length - 1; i > -1; i--)  //записываем лог с конца
                    sw.WriteLine(textBoxLog.Lines[i]);
                sw.Close();
            }
        }
        //callback при записи данных с микрофона
        private void Voice_Input(object sender, WaveInEventArgs e)  //в качестве аргумента WaveInEventArgs е объект содержащий данные записи
        {
            var encodedData = voiceCodec.Encode(e.Buffer, 0, e.BytesRecorded);  //перекодируем входящий массив с 0 позиции
            //e.Buffer массив записанных байт, e.BytesRecorded его размер
            //размер буфера записи и сжатие кодека гарантируют размер массива в 80 байт
            ZigUsb.BaseStream.WriteAsync(encodedData, 0, encodedData.Length);   //записываем в порт асинхронно
        }
        
    }
}
