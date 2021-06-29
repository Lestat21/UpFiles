using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;


namespace UpFiles
{
    class Program
    {
        static async Task Main(string[] args)
        {

            while (true)
            {
                using (FileStream fs = new FileStream("config.json", FileMode.OpenOrCreate))
                {
                    Params dir = await JsonSerializer.DeserializeAsync<Params>(fs); // читаем параметры из конфига

                    string NameFile = GetNameFile(dir.Dir, dir.FileNameStart); // имя текущего файла
                    string NameFileUp = GetNameFile(Directory.GetCurrentDirectory(), dir.FileNameStart); // имня нового файла
                    string FileVers = "Файл отсутствует";
                    string FileVersUp = "Файл отсутствует";

                    if (NameFile != "") // проверяем версии если файлы существуют.
                    {
                        FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(dir.Dir, NameFile));
                        FileVers = myFileVersionInfo.FileVersion;
                    }

                    if (NameFileUp != "")
                    {
                        FileVersionInfo myFileVersionInfo1 = FileVersionInfo.GetVersionInfo(Path.Combine(Directory.GetCurrentDirectory(), NameFileUp));
                        FileVersUp = myFileVersionInfo1.FileVersion;
                    }

                    Console.WriteLine($"Tекущая версия:\t{FileVers} \nНовая версия:\t{FileVersUp}") ;
                    int result = String.Compare(NameFile, NameFileUp); //сравнение названий


                    if (result < 0 || FileVers!= FileVersUp)
                    {
                        Console.WriteLine("Нужно обновлять");

                        Console.WriteLine("Останавливаем службу, если есть такая");

                        Process.Start("CMD.exe", $"/C  net stop SystemLog");

                        string path = dir.Dir + NameFile;

                        FileInfo fileInf = new FileInfo(path);
                        if (fileInf.Exists)
                        {
                            File.Delete(path);
                        }
                        Console.WriteLine("Копируем новый");
                        FileInfo fileInfNew = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), NameFileUp));
                        if (fileInfNew.Exists)
                        {
                            File.Copy(Path.Combine(Directory.GetCurrentDirectory(), NameFileUp), Path.Combine(dir.Dir, NameFileUp), true);
                        }
                        Console.WriteLine("Запускаем службу, если есть такая");
                        Process.Start("CMD.exe", $"/C net start SystemLog");
                    }
                    else
                    {
                        Console.WriteLine("Обновление не требуется");
                    }
                    
                    // при необходимости удалить все.
                    // 1. проверяем вотрой конфиг, если алерт то
                    // 1.1. останавливаем службу
                    // 1.2. Деинсталируем службу
                    // 1.3. Удаляем каталог и прочие файлы
                }

                Thread.Sleep(60000);
                Console.WriteLine(DateTime.Now);
            }
        }

        static public string GetNameFile(string dir, string fileName)  //поиск по части имени файла
        {
            string x = "";
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = new FileInfo(files[i]);

                if (file.Name.StartsWith(fileName))
                    x = file.Name;
            }
            return x;
        }
    }
}
