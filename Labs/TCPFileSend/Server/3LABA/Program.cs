using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace HWThreads
{
    class myServer
    {
        Thread Threadgo;
        public myServer(string name, Int32 port)
        {
            Threadgo = new Thread(this.func);
            Threadgo.Name = name;
            Threadgo.Start(port);
        }

        public void func(Object StateInfo)
        {
                   TcpListener server = null;
            try
            {
                Int32 port = (Int32)StateInfo;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");


                server = new TcpListener(localAddr, port);

                server.Start();

                Byte[] bytes = new Byte[256];
                String data = null;

                while (true)
                {
                    DoBeginAcceptTcpClient(server);
                    Console.WriteLine("Connected #" + Thread.CurrentThread.Name);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
        public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        public static void DoBeginAcceptTcpClient(TcpListener
            listener)
        {
            tcpClientConnected.Reset();            
            listener.BeginAcceptTcpClient(
                new AsyncCallback(DoAcceptTcpClientCallback),
                listener);
            tcpClientConnected.WaitOne();
        }

        public static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            Thread.Sleep(0);
            Byte[] bytes = new Byte[1024];
            String data = null;
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            Console.WriteLine("Client connect completed");

            data = null;
            NetworkStream stream = client.GetStream();

            BinaryFormatter outformat = new BinaryFormatter();

            string name;
            name = outformat.Deserialize(stream).ToString();
            FileStream fs = new FileStream("D:\\" + name, FileMode.OpenOrCreate);
            BinaryWriter bw = new BinaryWriter(fs);
            int count;
            count = int.Parse(outformat.Deserialize(stream).ToString());
            int i = 0;
            for (; i < count; i += 1024)
            {

                byte[] buf = (byte[])(outformat.Deserialize(stream));
                bw.Write(buf);
            }
            Console.WriteLine("Successfully read in D:\\" + name);
            bw.Close();
            fs.Close();
            tcpClientConnected.Set();

        }

    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Listening...");
            myServer myServ1 = new myServer("1", 13324);
            myServer myServ2 = new myServer("2", 13325);
            myServer myServ3 = new myServer("3", 13326);
            Console.ReadKey();
        }
    }
}
