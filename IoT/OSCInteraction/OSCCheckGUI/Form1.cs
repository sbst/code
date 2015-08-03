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
using Ventuz.OSC;

namespace OSCCheckGUI
{
    public partial class Form1 : Form
    {
        
        static PXCMAudioSource source;
        static PXCMSpeechRecognition sr;
        static PXCMSession session;
        static float[] vol;
        static int flag;
        static int flag2;
        static int flag3;
        static PXCMSenseManager sm;
        static Dictionary<string, PXCMCapture.DeviceInfo> Devices { get; set; }
        static private OscManager oscManager1;
        System.Threading.Thread thread;

        pxcmStatus OnModuleProcessedFrame(Int32 mid, PXCMBase module, PXCMCapture.Sample sample)
        {
            
            // check if the callback is from the hand tracking module.
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
                                    if (vol[3] > 10F)
                                        break;
                                    vol[3] += 0.4F;
                                    textVolume.Invoke(new Inform((s) => textVolume.Text = s), vol[3].ToString("F"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Thumb up" + "\n");
                                    Send("/vol4", vol[3]);
                                    break;

                                case "thumb_down":
                                    vol[3] -= 0.4F;
                                    textVolume.Invoke(new Inform((s) => textVolume.Text = s), vol[3].ToString("F"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Thumb down" + "\n");
                                    Send("/vol4", vol[3]);
                                    break;

                                case "fist":
                                    if (vol[0] < 1.6F)
                                        vol[0] += 0.1F;
                                    textSpeed.Invoke(new Inform((s) => textSpeed.Text = s), vol[0].ToString("F"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Fist" + "\n");
                                    Send("/vol1", vol[0]);
                                    break;

                                case "spreadfingers":
                                    if (vol[0] > 0.4F)
                                        vol[0] -= 0.1F;
                                    textSpeed.Invoke(new Inform((s) => textSpeed.Text = s), vol[0].ToString("F"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Spreadfingers" + "\n");
                                //    Send("/vol1", vol[0]);
                                    break;

                                case "full_pinch":
                                    if (vol[4] > 0.9F)
                                        flag3 = 1;

                                    if (vol[4] < 0.1F)
                                        flag3 = 0;

                                    if (flag3 == 0)
                                        vol[4] += 0.1F;
                                    else
                                        vol[4] -= 0.1F;
                                    textPanorama.Invoke(new Inform((s) => textPanorama.Text = s), vol[4].ToString("0.##"));
                                    logTextBox.Invoke(new Inform((s) => logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: Full pinch" + "\n");
                                    Send("/vol2", vol[4]);
                                    break;

                                case "v_sign":
                                    vol[0] = 1F;
                                    Send("/vol1", vol[0]);
                                    vol[4] = 1F;
                                    Send("/vol2", vol[4]);
                                    //vol[2] = 0F;
                                    //Send("/vol3", vol[2]);
                                    vol[3] = 2F;
                                    Send("/vol4", vol[3]);
                                    textVolume.Invoke(new Inform((s) => textVolume.Text = s), vol[3].ToString("F"));
                                    textSpeed.Invoke(new Inform((s) => textSpeed.Text = s), vol[0].ToString("F"));
                                    textPanorama.Invoke(new Inform((s) => textPanorama.Text = s), vol[4].ToString("0.##"));
                                    logTextBox.Invoke(new Inform((s) =>  logTextBox.Text = s + logTextBox.Text), DateTime.Now.ToString("hh:mm:ss") + " Gesture: V sign" + "\n");
                                    //vol[4] = 0.5F;
                                    //Send("/vol5", vol[4]);
                                    //vol[5] = 0F;
                                    //Send("/vol6", vol[5]);
                                    break;
                            }
                            Thread.Sleep(500);//100
                        }

                    }
                }

            }
            // return NO_ERROR to continue, or any error to abort.
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
            thread = new System.Threading.Thread(DoRecognition);
            flag = 0;
            oscManager1 = new OscManager();
            oscManager1.DestIP = "127.0.0.1";

            // Create the SenseManager instance
            sm = PXCMSenseManager.CreateInstance();
            // Enable hand tracking
            sm.EnableHand();
            // Get a hand instance here for configuration
            PXCMHandModule hand = sm.QueryHand();
            // Initialize and stream data.
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
            oscManager1.DestPort = Convert.ToInt32(textBox1.Text);
            if (flag == 0)
                thread.Start();
            else
                thread.Resume();        //rude method
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Streaming..." + "\n" + logTextBox.Text;
            System.Threading.Thread.Sleep(5);
            
        }

        private void DoRecognition()
        {
            sm.StreamFrames(true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = false;
            thread.Suspend();           //rude method
            flag = 1;
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Stopped" + "\n" + logTextBox.Text;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Clean Up..." + "\n" + logTextBox.Text;
            oscManager1.Dispose();
            thread.Suspend();
            sm.Close();
            sm.Dispose();
            thread.Abort();
            logTextBox.Text = DateTime.Now.ToString("hh:mm:ss") + " Exit" + "\n" + logTextBox.Text;
        }

        static public void Send(string addr, params object[] param)
        {
            Ventuz.OSC.OscBundle d = new Ventuz.OSC.OscBundle(0, new Ventuz.OSC.OscElement(addr, param));
            oscManager1.Send(d);
        }
    }
}
