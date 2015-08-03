using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace ComToServer
{
    public partial class Form1 : Form
    {
        static bool FTPConnected;
        static bool COMConnected;
        static string login;
        static string pass;
        static string incomestr;
        SerialPort ZigUsb;
        static String[] que;
        static int numque;
        Form2 frm2;

        public Form1()
        {
            InitializeComponent();
            FTPPass.PasswordChar = '*';
            FTPConnected = false;
            COMConnected = false;
            frm2 = new Form2();
            que = new String[3];
            progressBar1.Maximum = 3;
            numque = 0;
        }

        private void FtpConnect_Click(object sender, EventArgs e)
        {
            if (!FTPConnected)
            {
                try
                {
                    FTPConnected = true;
                    login = FTPLogin.Text;
                    pass = FTPPass.Text;
                    UploadPacket("");
                    FTPCon.Text = "Connected";
                }
                catch (WebException webExcp)
                {
                    MessageBox.Show(webExcp.Message.ToString(), "Connect FTP error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ComConnect_Click(object sender, EventArgs e)
        {
            if (!COMConnected)
            {
                try
                {
                    COMConnected = true;
                    ZigUsb = new SerialPort(COMPort.Text, 19200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                    //ZigUsb.Handshake = Handshake.RequestToSend;
                    ZigUsb.Handshake = Handshake.XOnXOff;
                    ZigUsb.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    ZigUsb.Open();
                    COMCon.Text = "Connected";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Connect COM error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public delegate void ProgressControl(int value);

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            if (FTPConnected && COMConnected)
            if (COMConnected)
            {
                SerialPort sp = (SerialPort)sender;
                string indata = sp.ReadExisting();
                incomestr += indata;
                if (indata.Contains(':'))                  //if we have delimiter in message
                {                
                    ControlFirstQueue();
                }
            }
        }


        //controllers
        private void ControlFirstQueue()
        {
            string uploadstr = incomestr.Remove(incomestr.Length - 2);
            if (numque < que.Length - 1)            //if main queue has free space
            {
                AddPacket(uploadstr);
                incomestr = String.Empty;
            }
            else                                //else - main queue is full
            {
                AddPacket(uploadstr);           //add last packet
                incomestr = String.Empty;
                for (int i = 0; i < numque; i++) //upload all messages
                {
                    UploadPacket(que[i]);
                    progressBar1.Invoke(new ProgressControl((value) => progressBar1.Value -= value), 1); //progress = 0
                    que[i] = String.Empty;
                }
                numque = 0;                      //cursor = 0
            }
        }

        //packets control functions
        private static void UploadPacket(string str)
        {
            File.WriteAllText("data.log", str);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"ftp://91.238.230.17/ZigBeeData/eape/data.log");
            request.Credentials = new NetworkCredential(login, pass);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            StreamReader sourceStream = new StreamReader("data.log");
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            response.Close();
            File.Delete("data.log");
        }

        private void AddPacket(string uploadstr)
        {
            que[numque] = uploadstr;        //fill this free space according cursor
            progressBar1.Invoke(new ProgressControl((value) => progressBar1.Value += value), 1);
            numque++;
        }

        //final events
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (COMConnected)
                ZigUsb.Dispose();
        }

   /*     private void label7_Click(object sender, EventArgs e)
        {

        }*/

        private void queueLengthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm2.ShowDialog();
            for (int i = 0; i < que.Length; i++)
                que[i] = String.Empty;
            numque = 0;
            progressBar1.Value = 0;
            try
            {
                Int16.Parse(frm2.Data);
                Array.Resize(ref que, Int16.Parse(frm2.Data));
                progressBar1.Maximum = Int16.Parse(frm2.Data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Parse error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
