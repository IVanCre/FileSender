using System;
using System.Net.NetworkInformation;
using System.IO;
using Xamarin.Essentials;



namespace FileSender
{
    public class ButtonFuncs
    {
        static string hostAdress = "192.168.1.39";//адрес сервака
        static string sslPortClient = "835";

        public static void ConnectToServer(System.Collections.Generic.List<string> selectedFileNames)
        {
            UniversalIO.Print("");

            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(hostAdress);

            if (reply.Status == IPStatus.Success)
            {
                //string serverCertificateName = CertFuncs.GetCurrentUserCertificates().GetName();// (пример: CN=ILMA\suvorin_ia);это имя, кому выдан сертификат(нужно тянуть из сертификата)
                string serverCertificateName = "C=RU, C=Some-State, L=Tomsk, O=MyComp, CN=Ivan_1";
                SslTcpClient.RunSslClient(hostAdress, Convert.ToInt32(sslPortClient), serverCertificateName,  selectedFileNames);
            }
            else
                UniversalIO.PrintAll($"\n Сервер вне сети! {reply.Status}");
        }   
        



        public static void SaveUserInfo() //меняем свой логин на новый
        {
            if(MainActivity.loginText.Text!="" && MainActivity.paswText.Text!="" )
            {
                try
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(MainActivity.UserSetsFilename, FileMode.OpenOrCreate)))
                    {
                        writer.Write(MainActivity.loginText.Text);
                        writer.Write(MainActivity.paswText.Text);
                    }
                    ClosePaswAndSetbutton();
                    UniversalIO.Print("Логин_пароль сохранены");
                }
                catch(Exception e)
                {
                    UniversalIO.Print("Логин_пароль  НЕ сохранены!");
                }
            }
        }

        public static bool CheckPresentUserSets()
        {
            try
            {
                if (File.Exists(MainActivity.UserSetsFilename))
                {
                    using (var stream = File.Open(MainActivity.UserSetsFilename, FileMode.Open))
                    {
                        using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
                        {
                            MainActivity.loginText.Text = reader.ReadString();
                            string pasw =reader.ReadString();

                            ClosePaswAndSetbutton();

                            if (MainActivity.loginText.Text!= "" && pasw != "")
                               return true;
                        }
                    }
                }
                else
                {
                    UniversalIO.Print(" Логин_пароль  не найдены.\n Заполните поля и сохраните");
                }
            }
            catch (Exception ex)
            {
                UniversalIO.Print("Невозможно извлечь данные пользователя:\n" + ex.Message);
            }
            return false;
        }

        private static void ClosePaswAndSetbutton()
        {
            MainActivity.paswText.Text = "******";
            MainActivity.acceptSettingsButton.Visibility = Android.Views.ViewStates.Invisible;//делаем недоступным кнопку сохранения настроек юзера
        }




        public static async void PickFiles(System.Collections.Generic.List<string> selectedFilenames, Android.Widget.Button sendButton, Android.Widget.Button selectFileButton)//для выбора нескольких файлов нужно на первом выбранном длительное нажатие, остальные выбираются простым нажатием. Затем, в правом верхнем угле жмем Открыть
        {
            try
            {
                if (selectedFilenames != null)
                {
                    selectedFilenames.Clear();
                    sendButton.Visibility = Android.Views.ViewStates.Invisible;
                }


                var selectedImage = await FilePicker.PickMultipleAsync();
                if (selectedImage != null)
                {
                    foreach (var selected in selectedImage)
                    {
                        selectedFilenames.Add(selected.FullPath);
                    }
                }

                sendButton.Visibility = Android.Views.ViewStates.Visible; //если файлы выбраны - можно  разрешать пересылку на сервак
                selectFileButton.Text = $"Выбрано {selectedFilenames.Count} файлов";
                return;
            }
            catch (Exception ex)
            {
                UniversalIO.PrintAll(ex.Message);
            }

        }
    }
}