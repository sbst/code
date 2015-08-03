using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
namespace _3LABAClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SendMessageFromSocket(11000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        static void SendMessageFromSocket(int port)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            sender.Connect(ipEndPoint);

            string path = Console.ReadLine();

            NetworkStream stream = sender.GetStream();
            BinaryFormatter format = new BinaryFormatter();
            byte[] buf = new byte[1024];
            int count;
            int process = 0;
            int endprocess = 100;
            FileStream fs = new FileStream("C:\\" + path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            long k = fs.Length;
            format.Serialize(stream, path);
            format.Serialize(stream, k.ToString());
            while ((count = br.Read(buf, 0, 1024)) > 0)
            {
                process++;
                Console.Clear();
                Console.Write(((process * 1024) * 100) / fs.Length + "%");
                format.Serialize(stream, buf);
            }
            Console.WriteLine("\nSuccessfully send");
            stream.Close();

            // Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}