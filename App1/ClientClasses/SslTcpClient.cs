using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace FileSender
{
    public class SslTcpClient
    {
        public static bool ValidateServerCertificate(
                                                    object sender,
                                                    X509Certificate certificate,
                                                    X509Chain chain,
                                                    SslPolicyErrors sslPolicyErrors)
        {
            return true;//для использования самоподписного сертификата, всегда гуд!(ну или можно заморочиться и попробовать дернуть валидный публичный и на нем все проверять)
        }


        public static void RunSslClient(string hostAddres, int port, string serverHostName, System.Collections.Generic.List<string> selectedFileNames)
        {
            TcpClient client=null;
            try
            {
                client = new TcpClient(hostAddres, port);
            }
            catch(Exception e)
            {
                UniversalIO.PrintAll("Сервер не отвечает(не запущен)!\n");
            }




            if (client != null)
            {
                SslStream sslStream = new SslStream(client.GetStream(),
                                                                false,
                                                                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                                                                null);

                try
                {
                    sslStream.AuthenticateAsClient(serverHostName);
                    UniversalIO.PrintAll("Сервер подтвердил свою подлинность!\n");
                }
                catch (AuthenticationException e)
                {
                    UniversalIO.PrintAll($"Exception: {e.Message}");
                    if (e.InnerException != null)
                    {
                        UniversalIO.PrintAll($"Inner exception: {e.InnerException.Message}");
                    }
                    UniversalIO.PrintAll("Сервер не смог\n подтвердить свою подлинность!\n Завершение сессии.");
                    client.Close();
                    return;
                }

                DataForSession session = new DataForSession();
                session.GetSettingsFromFile();
                string serverMessage = "";
                
                try
                {
                    byte[] messsage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(session) + "<EOF>");
                    sslStream.Write(messsage);//передаем ключ и данные для текущей сессии
                    sslStream.Flush();

                    serverMessage = CertFuncs.GetDataFromSSl(sslStream);//сервер передает номер порта, который будет слушать для получения данных
                    UniversalIO.PrintAll($"Ответ сервера: {serverMessage}\n");
                }
                catch (Exception e)
                {
                    UniversalIO.PrintAll("Сервер на смог ответить: "+e.Message);
                }
                finally
                {
                    sslStream.Close();
                    client.Close();
                }


                int serverDataPort = 0;
                if (serverMessage.Contains("Server listen"))//запускаем передачу данных
                {
                    string[] words = serverMessage.Split(new char[] { '_' });
                    serverDataPort = Convert.ToInt32(words[1]);

                    byte[] cryptKey = Encoding.ASCII.GetBytes(session.cryptKey);
                    ClientFileTransfer.SendAllFiles(hostAddres, serverDataPort, session.login, cryptKey, selectedFileNames);
                }
            }
        }
    }
    public struct DataForSession
    {
        private static string userDataSet = MainActivity.UserSetsFilename; 
        public string login { get; set; }
        public string pasw { get; set; }
        public string cryptKey { get; set; }

        public void GetSettingsFromFile()
        {
            try
            {
                if (File.Exists(userDataSet))
                {
                    using (var stream = File.Open(userDataSet, FileMode.Open))
                    {
                        using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                        {
                            login = reader.ReadString();
                            pasw = reader.ReadString();
                            cryptKey = GenerateKey();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UniversalIO.Print("Ошибка чтения пользовательских\nданных: "+ex.Message);
            }
        }



        private string GenerateKey()
        {
            Random randomaiser = new Random();
            return randomaiser.Next(1, 1000).ToString();
        }
    }
}

