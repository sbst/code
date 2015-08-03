using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace InteractGoPlus
{
    public partial class Form1 : Form
    {        
        static float[] vol;
        static int flag;
        static int flag3;
        static PXCMSenseManager sm;
        static Dictionary<string, PXCMCapture.DeviceInfo> Devices { get; set; }
        MyBeautifulThread thread;

        pxcmStatus OnModuleProcessedFrame(Int32 mid, PXCMBase module, PXCMCapture.Sample sample)
        {
            thread.Wait();
            if (mid == PXCMHandModule.CUID)
            {
                PXCMHandModule hand = module.QueryInstance<PXCMHandModule>();
                PXCMHandData handData = hand.CreateOutput();
                handData.Update();
                if (handData == null)
                {
                    Console.WriteLine("Failed Create Output");
                    Console.WriteLine("That`s all...");
                    Console.ReadKey();
                    return 0;
                }
                PXCMHandData.JointData[][] nodes = new PXCMHandData.JointData[][] { new PXCMHandData.JointData[0x20], new PXCMHandData.JointData[0x20] };
                int numOfHands = handData.QueryNumberOfHands();
                for (int i = 0; i < numOfHands; i++)
                {
                    //Get hand by time of appearence
                    PXCMHandData.IHand IhandData;
                    if (handData.QueryHandData(PXCMHandData.AccessOrderType.ACCESS_ORDER_BY_TIME, i, out IhandData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                    {
                        if (handData != null)
                        {
                            //Iterate Joints
                            for (int j = 0; j < 0x20; j++)
                            {
                                PXCMHandData.JointData jointData;
                                IhandData.QueryTrackedJoint((PXCMHandData.JointType)j, out jointData);
                                nodes[i][j] = jointData;
                            } // end iterating over joints
                        }
                    }
                } // end itrating over hands
                int numOfGestures = handData.QueryFiredGesturesNumber();
                if (numOfGestures > 0)
                {
                    for (int i = 0; i < numOfGestures; i++)
                    {
                        PXCMHandData.GestureData gestureData;
                        if (handData.QueryFiredGestureData(i, out gestureData) == pxcmStatus.PXCM_STATUS_NO_ERROR)
                        {
                            switch (gestureData.name)
                            {
                                case "thumb_up":
                                    break;

                                case "thumb_down":
                                    break;

                                case "fist":
                                    break;

                                case "spreadfingers":
                                    textSpeed.Invoke(new Inform((s) => textSpeed.Text = s), vol[0].ToString("F"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Spreadfingers" + "\n");
                                    break;

                                case "full_pinch":
                                    textPanorama.Invoke(new Inform((s) => textPanorama.Text = s), vol[4].ToString("0.##"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Full pinch" + "\n");
                                    break;

                                case "v_sign":                                    
                                    textVolume.Invoke(new Inform((s) => textVolume.Text = s), vol[3].ToString("F"));
                                    textSpeed.Invoke(new Inform((s) => textSpeed.Text = s), vol[0].ToString("F"));
                                    textPanorama.Invoke(new Inform((s) => textPanorama.Text = s), vol[4].ToString("0.##"));
                                    logTextBox.Invoke(new Inform((s) =>  logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: V sign" + "\n");
                                    break;
                            }
                        }

                    }
                }

            }
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        delegate void Inform(string text);
        delegate void TextBoxSlide(RichTextBox s);

        public Form1()
        {
            InitializeComponent();
            vol = new float[6] { 1F, 1F, 0F, 1F, 0.5F, 0F };
            textVolume.Text = vol[3].ToString();
            textSpeed.Text = vol[0].ToString();
            textPanorama.Text = vol[4].ToString();
            thread = new MyBeautifulThread(DoRecognition);
            flag = 0;

            sm = PXCMSenseManager.CreateInstance();
            sm.EnableHand();
            PXCMHandModule hand = sm.QueryHand();
            PXCMSenseManager.Handler handler = new PXCMSenseManager.Handler
            {
                onModuleProcessedFrame = OnModuleProcessedFrame
            };
            sm.Init(handler);

            PXCMHandConfiguration handConfiguration = sm.QueryHand().CreateActiveConfiguration();
            handConfiguration.EnableGesture("thumb_down");
            handConfiguration.EnableGesture("thumb_up");
            handConfiguration.EnableGesture("v_sign");
            handConfiguration.EnableGesture("spreadfingers");
            handConfiguration.EnableGesture("fist");
            handConfiguration.EnableGesture("full_pinch");
            handConfiguration.ApplyChanges();

            if (handConfiguration == null)
            {
                Console.WriteLine("Failed Create Configuration");
                Console.WriteLine("That`s all...");
                Console.ReadKey();
            }
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Started" + "\n" + logTextBox.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = true;
            if (flag == 0)
                thread.Start();
            else
                thread.Resume();
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Streaming..." + "\n" + logTextBox.Text;
        }

        private void DoRecognition()
        {
            sm.StreamFrames(false);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            thread.Suspend();
            flag = 1;
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Stopped" + "\n" + logTextBox.Text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Clean Up..." + "\n" + logTextBox.Text;
            thread.Dispose();
            sm.Close();                        
            sm.Dispose();
            
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Exit" + "\n" + logTextBox.Text;
        }
    }
    //в этом примере используется callback в конструкторе этого класса (ThreadStart start)
    //в случае если колбек не нужен, то раскомментируем строки
    public class MyBeautifulThread : IDisposable
    {
        private Thread m_Thread;
        //private volatile bool m_Terminated = false;
        private ManualResetEvent m_EvSuspend = new ManualResetEvent(true);

        public MyBeautifulThread(ThreadStart start)
        {
            m_Thread = new Thread(start); //new ThreadStart(MyThread);
        }

        public void Dispose()
        {
            //m_Terminated = true;
            Resume();
            m_Thread.Join();
            m_EvSuspend.Close();
            m_EvSuspend.Dispose();
        }

        public void Start()
        {
            m_Thread.Start();
        }

        public void Suspend()
        {
            m_EvSuspend.Reset();
        }

        public void Resume()
        {
            m_EvSuspend.Set();
        }
        public void Wait()
        {
            m_EvSuspend.WaitOne();
        }
        /*        private void MyThread()
                {
                    while (!m_Terminated)
                    {
                        // тут чё-то делаем

                        m_EvSuspend.WaitOne();
                    }
                }*/
    }
}
