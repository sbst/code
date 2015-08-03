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
using System.Net;
using System.IO;

namespace InteractGoPlus
{
    public partial class Form1 : Form
    {        
        static int flagThread;
        static int flagLight;
        static int LightState;
        static int flagWind;
        static int WindState;
        static int SendState;
        const string DidLight = "51pgvpo06ue2";
        const string DidWind = "ictvj8authr4";
        static PXCMSenseManager sm;
        static Dictionary<string, PXCMCapture.DeviceInfo> Devices { get; set; }
        MyBeautifulThread thread;
      //  System.Windows.Forms.Timer _timer;

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
                                case "wave":
                                    string[] response = GET("http://168.63.82.20/api/login", "login=sbst&password=ebc1628c26f8515f81a5178a5abfcbd9").Split('\"');
                                    response = GET("http://168.63.82.20/api/thing", @"user_id=" + response[7] + @"&token=" + response[3] + @"&did=yk5ynj69aw7z").Split('\"'); ;
                                    label1.Invoke(new Inform((s) => label1.Text = s), response[15] + " C");
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text + WindState.ToString()), "GETDATA Temperature:" + response[15] + " C" + "\n");
                                    break;


                                case "tap":
                                    flagLight++;
                                    if (flagLight == 2)
                                    {
                                        if (LightState == 0)
                                            LightState = 1;
                                        else
                                            LightState = 0;
                                        flagLight = 0;
                                        logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), "MAKEDATA LightState:" + LightState + " WindState:" + WindState + "\n");
                                    }
                                    
                                    break;

                                case "swipe_up":
                                    flagWind++;
                                    if (flagWind == 2)
                                    {
                                        if (WindState == 0)
                                            WindState = 1;
                                        else
                                            WindState = 0;
                                        flagWind = 0;
                                        logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text + WindState.ToString()), "MAKEDATA LightState:" + LightState + " WindState:" + WindState + "\n");
                                    }
                                    break;

                                case "thumb_up":
                                    SendState++;
                                    if (SendState == 2)
                                    {
                                        if (LightState == 0)
                                            ChangeSwitch(DidLight, "501");
                                        else
                                            ChangeSwitch(DidLight, "498");

                                        if (WindState == 0)
                                            ChangeSwitch(DidWind, "501");
                                        else
                                            ChangeSwitch(DidWind, "498");
                                        logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), "SENDDATA LightState:" + LightState + " WindState:" + WindState + "\n");
                                        //SendState = 0;
                                    }
                                    break;                                    
                                    //textVolume.Invoke(new Inform((s) => textVolume.Text = s), vol[3].ToString("F"));
                                    //textSpeed.Invoke(new Inform((s) => textSpeed.Text = s), vol[0].ToString("F"));
                                    //textPanorama.Invoke(new Inform((s) => textPanorama.Text = s), vol[4].ToString("0.##"));
                                    //logTextBox.Invoke(new Inform((s) =>  logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: V sign" + "\n");
                            }
                        }

                    }
                }                
            }
            //Thread.Sleep(100);
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        public static byte[] GetRGB32Pixels(PXCMImage image, out int cwidth, out int cheight)
        {
            PXCMImage.ImageData cdata;
            byte[] cpixels = null;
            cwidth = cheight = 0;
            if (image.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out cdata) >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                cwidth = cdata.pitches[0] / sizeof(Int32);
                cheight = image.info.height;
                cpixels = cdata.ToByteArray(0, cdata.pitches[0] * cheight);
                image.ReleaseAccess(cdata);
            }
            return cpixels;
        }

        delegate void Inform(string text);
        delegate void TextBoxSlide(RichTextBox s);

        public Form1(PXCMSession session)
        {
            InitializeComponent();

            //_timer = null;

            thread = new MyBeautifulThread(DoRecognition);
            flagThread = 0;

            LightState = 0;
            flagLight = 0;
            WindState = 0;
            flagWind = 0;
            SendState = 0;

            sm = PXCMSenseManager.CreateInstance();
            sm.EnableHand();
            PXCMHandModule hand = sm.QueryHand();
            sm.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 320, 240, 60);
            PXCMSenseManager.Handler handler = new PXCMSenseManager.Handler
            {
                onModuleProcessedFrame = OnModuleProcessedFrame
            };


            sm.Init(handler);

            PXCMHandConfiguration handConfiguration = sm.QueryHand().CreateActiveConfiguration();
            handConfiguration.EnableGesture("wave");
            handConfiguration.EnableGesture("swipe_up");
            handConfiguration.EnableGesture("thumb_up");
            handConfiguration.EnableGesture("tap");
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
            if (flagThread == 0)
                thread.Start();
            else
                thread.Resume();
            /*if (_timer == null)
            {
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 5000;
                _timer.Tick += _timer_Tick;
            }
            _timer.Start();*/
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
            flagThread = 1;
           // _timer.Stop();
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

        static void ChangeSwitch(string DID ,string state)
        {
            
            WebRequest request = WebRequest.Create("http://168.63.82.20/server/income/?did=" + DID + "&action=put&value=" + state);
            DoWithResponse(request, (response) =>
            {
                var body = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Debug.Print(body);
                if (SendState >= 2)
                    SendState = 0;
            });
        }

        //For GetQuery
        static void DoWithResponse(WebRequest request, Action<HttpWebResponse> responseAction)
        {
            Action wrapperAction = () =>
            {
                request.BeginGetResponse(new AsyncCallback((iar) =>
                {
                    var response = (HttpWebResponse)((HttpWebRequest)iar.AsyncState).EndGetResponse(iar);
                    responseAction(response);
                }), request);
            };
            wrapperAction.BeginInvoke(new AsyncCallback((iar) =>
            {
                var action = (Action)iar.AsyncState;
                action.EndInvoke(iar);
            }), wrapperAction);
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            /*string[] response = GET("http://168.63.82.20/api/login", "login=sbst&password=ebc1628c26f8515f81a5178a5abfcbd9").Split('\"');
            response = GET("http://168.63.82.20/api/thing", @"user_id=" + response[7] + @"&token=" + response[3] + @"&did=yk5ynj69aw7z").Split('\"'); ;
            label1.Text = response[15] + " C";*/
        }
        
        static private string GET(string Url, string Data)
        {
            WebRequest req = WebRequest.Create(Url + "?" + Data);
            WebResponse resp = req.GetResponse();
            Stream stream = resp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
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
