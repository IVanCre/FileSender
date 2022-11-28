using System;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO.Compression;



namespace FileSender
{
    class ClientFileTransfer
    {
        public static async void SendAllFiles(string serverAddress, int serverPort, string login, byte[] cryptKey, List<string> selectedfilePaths)
        {
            try
            {
                var folder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim);

                long startTime = DateTime.Now.Ticks;
                string archiveName = $"Compressed.zip";
                string archivePathName = folder.AbsolutePath + "/" + archiveName;



                try
                {
                    CreateCompressedArchive(selectedfilePaths, archivePathName);
                    if (File.Exists(archivePathName))
                    {

                        await Task.Factory.StartNew(() =>
                                        {
                                            bool flag = false;
                                            using (TcpClient dataTcpClient = new TcpClient())//выделенное подключение для каждого файла(так надежнее, что один не завалит всех остальных)
                                            {
                                                dataTcpClient.Connect(serverAddress, serverPort);

                                                using (NetworkStream workStream = dataTcpClient.GetStream())
                                                {
                                                    using (Stream s = SymmetryCrypt.Encrypt(workStream, cryptKey))
                                                    {
                                                        flag = SendFile(archivePathName, s);//шифруем поток
                                                    }
                                                }
                                            }
                                            if (flag)
                                            {
                                                TimeSpan end = new TimeSpan(DateTime.Now.Ticks - startTime);
                                                UniversalIO.PrintAll(" Сжатие+Шифрование+Передача заняли: " + ((int)(end.TotalMilliseconds / 1000)).ToString() + " sec");
                                            }
                                            File.Delete(archivePathName);
                                        });
                    }
                    else
                    {
                        UniversalIO.PrintAll("\n Архив для передачи не найден!");
                    }
                }
                catch (Exception e)
                {
                    UniversalIO.PrintAll("\nОшибка в потоке передачи данных:\n " + e.Message);
                }
                
            }
            catch (Exception exp)
            {
                UniversalIO.PrintAll("\nОшибка передачи: " + exp.Message);
            }
        }




        private static void CreateCompressedArchive(List<string> filePaths, string archiveName)
        {
            try
            {
                if (File.Exists(archiveName))//удаляем старый архив, если он остался после неудачной передачи
                    File.Delete(archiveName);


                foreach (string file in filePaths)
                {
                    byte[] data = File.ReadAllBytes(file);
                    using (ZipArchive zip = ZipFile.Open(archiveName, ZipArchiveMode.Update))
                    {
                        zip.CreateEntry(Path.GetFileName(file)).Open().Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception e)
            {
                UniversalIO.PrintAll("Ошибка создания архива" + e.Message);
            }
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }




        private static bool SendFile(string filePath, Stream workStream)
        {
            try
            {
                int packetSize = 1024;
                byte[] packetForSend = new byte[packetSize];
                byte[] fileArr = File.ReadAllBytes(filePath);

                if (fileArr.Length > 0)
                {
                    int numCycle = fileArr.Length / packetSize;//сколько будет циклов передачи

                    int onePercent = fileArr.Length / 100;
                    int uploadedBytes = 0;
                    short percentUpload = 1;//т.к. нумерация идет с нуля, а писать загружено 0 процентов не гуд

                    if (numCycle > 0)
                    {
                        int ostatok = fileArr.Length - (numCycle * packetSize);
                        if (ostatok > 0)
                            numCycle += 1;


                        for (int i = 0; i < numCycle; i++)
                        {
                            if (i == numCycle - 1)//остаток
                            {
                                if (ostatok > 0)
                                {
                                    Array.Copy(fileArr, i * packetSize, packetForSend, 0, ostatok);
                                    Array.Resize(ref packetForSend, ostatok);
                                }
                            }
                            else
                                Array.Copy(fileArr, i * packetSize, packetForSend, 0, packetSize);

                            workStream.Write(packetForSend, 0, packetForSend.Length);

                            uploadedBytes += packetSize;
                            if (uploadedBytes >= onePercent)
                            {
                                percentUpload += 1;
                                uploadedBytes = 0;
                                UniversalIO.Print($" Передано {percentUpload}% ");
                            }
                        }
                    }
                    else//файл меньше буфера отправки
                    {
                        Array.Copy(fileArr, packetSize, packetForSend, 0, fileArr.Length);
                        workStream.Write(packetForSend, 0, packetForSend.Length);
                    }
                    UniversalIO.Print($"\n Архив полностью передан!");
                    return true;
                }
                else
                {
                    UniversalIO.PrintAll($"\nАрхив для передачи пуст!");
                    return false;
                }

            }
            catch(Exception e)
            {
                UniversalIO.PrintAll($"\nОшибка передачи пакета данных:\n {e.Message}");
                return false;
            }
        }
    }
        
}
