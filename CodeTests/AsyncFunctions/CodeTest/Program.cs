/*
--------------------------------------------------
Задание:
1. Изменить код функции ValueProcessor.SaveAsync, чтобы она сохраняла значения (values) в  папку (directory). Каждый элемент в отдельный файл. (Код нужно дописывать, не переписывать => изменять нужно ТОЛЬКО ValueProcessor.SaveAsync, другие реализованные классы и функции изменять не надо).
2.
Написать код, который в завивимости от переданных приложению аргументов:
- Создаёт N значений с помощью функции IValueProcessor.CreateAsync
- Считает сумму значений с помощью функции IMathAsync.SumAsync
- Считает максимальное значение с помощью функции IMathAsync.MaxAsync
- Считает минимальное значение с помощью функции IMathAsync.MinAsync
- Записывает все значения в файлы в папку
- Фиксирует в логе прогресс и вычисленные значения
--------------------------------------------------
Аргументы:
count <N> (обязательно) - количество значений, которое нужно создать
dir <DIR> - папка для записи файлов (если указана, то записывать)
sum - флаг подсчёта суммы (если указан, то считать)
max - флаг подсчёта максимума (если указан, то считать)
min - флаг подсчёта минимума (если указан, то считать)
--------------------------------------------------
Пример:
     
app.exe --count 100 --min --max
Выведет минимум и максимум
     
app.exe --count 200 --min --sum --dir "C:\dir1\dir2\dir3\"
Выведет минимум и сумму
Запишет все файлы в папку "C:\dir1\dir2\dir3\"
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace CodeTest
{
    interface IMathAsync
    {
        Task<int> MinAsync(IEnumerable<int> values, CancellationToken cancellationToken = default (CancellationToken));
        Task<int> MaxAsync(IEnumerable<int> values, CancellationToken cancellationToken = default (CancellationToken));
        Task<int> SumAsync(IEnumerable<int> values, CancellationToken cancellationToken = default (CancellationToken));
    }

    interface IValueProcessor
    {
        Task<int[]> CreateAsync(int count, CancellationToken cancellationToken = default (CancellationToken));
        Task SaveAsync(string directory, IEnumerable<int> values, CancellationToken cancellationToken = default (CancellationToken));
    }

    class SlowMath : IMathAsync
    {
        public async Task<int> MinAsync(IEnumerable<int> values, CancellationToken cancellationToken)
        {
            await DoStuffAsync(values, cancellationToken);
            return values.Min();
        }

        public async Task<int> MaxAsync(IEnumerable<int> values, CancellationToken cancellationToken)
        {
            await DoStuffAsync(values, cancellationToken);
            return values.Max();
        }

        public async Task<int> SumAsync(IEnumerable<int> values, CancellationToken cancellationToken)
        {
            await DoStuffAsync(values, cancellationToken);
            return values.Sum();
        }

        private async Task DoStuffAsync(IEnumerable<int> values, CancellationToken cancellationToken)
        {
            const int delay = 20;
            var count = values.Count();
            await Task.Delay(delay * count, cancellationToken);
        }
    }

    abstract class AbstractValueProcessor : IValueProcessor
    {
        public async Task<int[]> CreateAsync(int count, CancellationToken cancellationToken)
        {
            const int delay = 10;
            var random = new Random();
            var array = new int[count];
            for (var i = 0; i < count; ++i)
            {
                array[i] = random.Next(Int16.MinValue, Int16.MaxValue);
            }

            // doing some hard work
            await Task.Delay(count * delay, cancellationToken);
            return array;
        }
        public abstract Task SaveAsync(string directory, IEnumerable<int> enumerable, CancellationToken cancellationToken);
    }

    sealed class ValueProcessor : AbstractValueProcessor
    {
        // необходимо чтобы функция была асинхронная
        // для этого добавил ключевое слово async
        public override async Task SaveAsync(string directory, IEnumerable<int> values, CancellationToken cancellationToken)
        {
            int i = 0;
            Directory.CreateDirectory(directory);
            foreach (var item in values)
            {
                i++;
                using (StreamWriter writer = File.CreateText(directory + i + "value.txt"))
                {
                    await writer.WriteAsync(item.ToString());   // асинхронно записываем в файл
                }
            }
        }
    }

    static class Program
    {
        // глобальный токен для остановки, для того чтобы попадал в зону видимости делегата handler 
        static CancellationTokenSource cancelToken;
        // структура с переданными аргументами
        public struct features
        {
            public int countValues;
            public string directory;
            public bool sum;
            public bool max;
            public bool min;
        }

        static int Main(string[] args)
        {
            features feature;
            try
            {
                feature = getArguments(new features(), args);   // парсим аргументы и записываем в структуру
            }
            catch (FormatException)     // обработка случая, когда вместо числа передается символ
            {
                Console.WriteLine("Arguments: ");
                Console.WriteLine("count <N> (required) - the number of values");
                Console.WriteLine("dir <DIR> - folder for files");
                Console.WriteLine("sum - flag for sum");
                Console.WriteLine("max - flag for founding maximum");
                Console.WriteLine("min - flag for founding minimum");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return 0;
            }
            catch (IndexOutOfRangeException)    // обработка запуска без аргументов
            {
                Console.WriteLine("Arguments required!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return 0;
            }

            cancelToken = new CancellationTokenSource();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(handler);   // добавление делегата для корректного завершения работы
                                                                                // по сочетанию Ctrl+C
            ValueProcessor genValues = new ValueProcessor();

            Task<int[]> array = genValues.CreateAsync(feature.countValues, cancelToken.Token);  // генерируем массив чисел
            Console.Write("Creating instances...");

            while ((!array.IsCompleted) && (!array.IsCanceled))     // задержка Main
                Thread.Sleep(1);

            if (array.IsCanceled)       // если генерация чисел отменена
            {
                Console.WriteLine(" cancelled");
                Console.WriteLine("Process was terminated.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return 0;
            }
            else
            {
                Console.WriteLine(" done");
                // обработка каждого аргумента (--sum --max --min --dir) в отдельности
                Task saveFile = null;       
                Task<int> maxTask = null;
                Task<int> minTask = null;
                Task<int> sumTask = null;
                SlowMath math = new SlowMath();
                // если аргумент был передан, то запуск аснихронного метода, который соответствует этому аргументу
                if (feature.directory != "")
                {
                    saveFile = genValues.SaveAsync(feature.directory, array.Result, cancelToken.Token);
                    Console.WriteLine("Saving file...");
                }
                if (feature.max == true)
                {
                    maxTask = math.MaxAsync(array.Result, cancelToken.Token);
                    Console.WriteLine("Calculating maximum...");
                }
                if (feature.min == true)
                {
                    minTask = math.MinAsync(array.Result, cancelToken.Token);
                    Console.WriteLine("Calculating minimum...");
                }
                if (feature.sum == true)
                {
                    sumTask = math.SumAsync(array.Result, cancelToken.Token);
                    Console.WriteLine("Calculating sum...");
                }
                // задержка Main до выполнения всех асинхронных методов
                if (feature.max == true)
                    Console.WriteLine("Maximum: {0}", wait(maxTask));
                if (feature.min == true)
                    Console.WriteLine("Minumum: {0}", wait(minTask));
                if (feature.sum == true)
                    Console.WriteLine("Sum: {0}", wait(sumTask));
                if (feature.directory != "")
                {
                    while ((!saveFile.IsCompleted) && (!saveFile.IsCanceled))
                        Thread.Sleep(1);
                    Console.WriteLine("Files was saved in folder: {0}", feature.directory);
                }

            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return 0;
        }
        // функция парсинга аргументов в структуру
        static public features getArguments(features feature, string[] args)
        {
            int index = Array.IndexOf(args, "--count");
            feature.countValues = Int32.Parse(args[index + 1]);
            index = Array.IndexOf(args, "--dir");
            if (index != -1)
                feature.directory = args[index + 1];
            else
                feature.directory = String.Empty;
            index = Array.IndexOf(args, "--sum");
            if (index != -1)
                feature.sum = true;
            index = Array.IndexOf(args, "--max");
            if (index != -1)
                feature.max = true;
            index = Array.IndexOf(args, "--min");
            if (index != -1)
                feature.min = true;
            return feature;
        }
        // функция для задержки методов
        static private string wait(Task<int> ts)
        {
            while ((!ts.IsCompleted) && (!ts.IsCanceled))
                Thread.Sleep(1);

            if (!ts.IsCanceled)
                return ts.Result.ToString();
            else
                return " cancelled";
        }
        // событие по Ctrl+C
        static void handler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            cancelToken.Cancel();
        }
    }
}

