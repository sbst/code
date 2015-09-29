using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Runtime.InteropServices;

// генерация сигнатуры указанного файла
namespace SignatureFiles
{
    // класс объекта состояния потоков для синхронизации и поля для передачи данных между потоками
    class StateObject
    {
        public object syncState { get; private set; }   // объект для синхронизации потоков
        public int blockSize { get; private set; }      // размер блока данных
        public String path { get; private set; }        // путь до выбранного файла
        public MD5 md5Hash { get; private set; }        // объект для создания хеша
        public volatile bool readRunning;               // флаг о том что поток чтения запущен

        // конструктор с параметрами для инициализации
        public StateObject(String path, int blockSize)
        {
            this.readRunning = false;
            this.md5Hash = MD5.Create();
            this.syncState = new object();
            this.path = path;
            this.blockSize = blockSize;
        }
    }
    class Program
    {
        static public byte[] block;     // массив байт для считывания
        static public int blockNum;     // номер блока при считывании
        static StateObject stateThread; // объект состояния потоков
        
        static int Main()
        {
            Console.Write("Input full path to the file: "); // ввод данных о файле
            String file = Console.ReadLine();
            if (!File.Exists(file))     // проверка на существование
            {
                Console.WriteLine("File not found!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return 1;               // выходим с ошибкой
            }
            Console.Write("Input block size in KBytes: ");      // ввод данных о блоке
            int blockSize = 0;
            try
            {
                blockSize = Int32.Parse(Console.ReadLine());    // переводим из строки в число
                blockSize = blockSize * 1024;                   // переводим в килобайты
                block = new byte[blockSize];
            }
            catch
            {                
                Console.WriteLine("Format error. Setting default size: 1024KB");  // введено не число, присваиваем по умолчанию
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                blockSize = 1048576;
                block = new byte[blockSize];
            }
            blockNum = 0;
            stateThread = new StateObject(file, blockSize);     // инициализируем объект состояния
            Thread threadRead = new Thread(ReadFile);           // создаем два потока для чтения из файла
            Thread threadHash = new Thread(HashFile);           // и для генерации хеша и вывода на экран
            
            threadRead.Start(stateThread);                      // запускаем
            threadHash.Start(stateThread);

            Console.CancelKeyPress += new ConsoleCancelEventHandler(handler);   // назначаем функцию при прерывании по ctrl+c
            
            threadRead.Join();                                  // ждем заверешения потоков
            threadHash.Join();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return 0;
        }

        // функция при событии ctrl+c
        static void handler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;     // отменяем выход
            stateThread.readRunning = false;    // поднимаем флаг о прекращении чтения из файла
        }

        // функция выполняющаяся в потоке threadHash - хеширует блок данных и выводит номер этого блока и хеш сумму на экран
        static private void HashFile(object arg)
        {
            StateObject stateThread = arg as StateObject;   // объект с состоянием потоков
            byte[] dataConstruct = new byte[stateThread.blockSize];     //буффер для хеш значения блока данных
            StringBuilder sBuilder = new StringBuilder();   // для построения информации для вывода
            while (true)
            {
                lock (stateThread.syncState)    // блокировка состояния
                {
                    Monitor.Pulse(stateThread.syncState);   // освобождаем блокировку для другого потока
                    Monitor.Wait(stateThread.syncState);    // блокируем текущий поток
                    dataConstruct = stateThread.md5Hash.ComputeHash(block); // создаем хеш
                    for (int i = 0; i < dataConstruct.Length; i++)          //формируем строку
                    {
                        sBuilder.Append(dataConstruct[i].ToString("x2"));
                    }
                    Console.WriteLine("{0}. {1}", blockNum, sBuilder.ToString());   // выводим информацию
                    sBuilder.Remove(0, sBuilder.Length);    // очищаем строку
                    if (!stateThread.readRunning)           // если поток чтения завершился
                    {
                        Monitor.Pulse(stateThread.syncState);   // освобождаем блокировку
                        break;                                  // выходим из цикла
                    }
                }
            }
        }

        // функция выполняющаяся в потоке threadRead - считывает блок данных из файла и считает его номер
        // открытие файла, контроль доступа к файлу
        static private void ReadFile(object arg)
        {
            StateObject stateThread = arg as StateObject;
            try
            {
                using (FileStream streamFile = new FileStream(stateThread.path, FileMode.Open, FileAccess.Read))    // открываем файл на чтение
                {
                    stateThread.readRunning = true;     // поток чтения запущен

                    // пока позиция считывание больше или равна размеру блока
                    while (streamFile.Length - streamFile.Position >= stateThread.blockSize)
                    {
                        ReadFromFile(stateThread, streamFile, true);    // считываем файл с последующей блокировкой потока
                        if (!stateThread.readRunning)                   // если флаг потока чтения false (событие ctrl+c)
                        {
                            return;                                     // завершаем поток
                        }
                    }
                    // последняя итерация остатка - то что меньше размера блока чтения
                    ReadFromFile(stateThread, streamFile, false, Convert.ToInt32(streamFile.Length - streamFile.Position)); // считываем без блокировки остаток
                    stateThread.readRunning = false;    // поток чтения завершен
                }
            }
            catch
            {
                stateThread.readRunning = false;    // в случае если запоминающее устройство не доступно
                lock (stateThread.syncState)        
                {
                    Monitor.Pulse(stateThread.syncState);
                }
                Console.WriteLine("I/O error! Memory device not found.");
                Console.ReadKey();
            }
        }

        // функция чтения из уже открытого файла и подсчет номера блока
        static void ReadFromFile(StateObject state, FileStream streamFile, bool blocked, int blockSizeRemain = 0)
        {
            lock (state.syncState)      // блокируем состояние
            {
                if (blockSizeRemain == 0)   // если это не последняя итерация - остаток файла больше размера блока
                    streamFile.Read(block, 0, state.blockSize);     // считываем                    
                else                        // итерация последняя - остаток файла меньше размера блока
                {
                    streamFile.Read(block, 0, blockSizeRemain);     // считываем
                    for (int i = blockSizeRemain; i < state.blockSize; i++)     // дополняем нулями
                        block[i] = 0;
                }
                blockNum++;                 // считаем номер блока
                Monitor.Pulse(state.syncState);     // освобождаем блокировку для другого потока
                if (blocked)                // если итерации еще будут то блокируем поток
                    Monitor.Wait(state.syncState);
            }
        }
    }
}
