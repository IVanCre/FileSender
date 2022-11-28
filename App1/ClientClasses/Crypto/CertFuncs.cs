using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;



namespace FileSender
{
    class CertFuncs
    {
        public static X509Certificate GetCurrentUserCertificates()//заменить реализацию на работу с файлом
        {
            try
            {
                List<X509Certificate> certificates = new List<X509Certificate>();
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);//тянется только из хранилища текущего юзера!
                store.Open(OpenFlags.OpenExistingOnly);

                foreach (X509Certificate cert in store.Certificates)
                {
                    certificates.Add(cert);
                }


                return certificates[0];
            }
            catch (Exception e)
            {
                UniversalIO.PrintAll("Cant read sertificate");
                return null;
            }
        }
        //public static void ShowInfo(X509Certificate cert)
        //{
        //    UniversalIO.Print($" hostName: {cert.GetName()}\n" +
        //                      $"centrName: {cert.GetIssuerName()}\n");//если самоподписной - одинаковые
        //}

        public static string GetDataFromSSl(SslStream sslStream)
        {
            string s ="Exception_!";
            try
            {
                byte[] buffer = new byte[2048];
                StringBuilder messageData = new StringBuilder();
                int bytes = -1;
                do
                {
                    bytes = sslStream.Read(buffer, 0, buffer.Length);

                    Decoder decoder = Encoding.UTF8.GetDecoder();
                    char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                    decoder.GetChars(buffer, 0, bytes, chars, 0);
                    messageData.Append(chars);

                    if (messageData.ToString().IndexOf("<EOF>") != -1)
                    {
                        break;
                    }
                } while (bytes != 0);

                messageData.Remove(messageData.Length - 5, 5);//вырезаем <eof>

                s= messageData.ToString();
            }
            catch (Exception e)
            {
                UniversalIO.PrintAll("Cant get response from server");//данный момент 
            }
            return s;
        }

        //static public  void AllInfo(SslStream sslStream)
        //{
        //    DisplaySecurityLevel(sslStream);
        //    DisplaySecurityServices(sslStream);
        //    DisplayCertificateInformation(sslStream);
        //    DisplayStreamProperties(sslStream);
        //}
        //static void DisplaySecurityLevel(SslStream stream)
        //{
        //    UniversalIO.Print($"Cipher: {stream.CipherAlgorithm} strength {stream.CipherStrength}");
        //    UniversalIO.Print($"Hash: {stream.HashAlgorithm} strength {stream.HashStrength}");
        //    UniversalIO.Print($"Key exchange: {stream.KeyExchangeAlgorithm} strength {stream.KeyExchangeStrength}");
        //    UniversalIO.Print($"Protocol: {stream.SslProtocol}"); 
        //}
        //static void DisplaySecurityServices(SslStream stream)
        //{
        //    UniversalIO.Print($"Is authenticated: {stream.IsAuthenticated} as server? {stream.IsServer}");
        //    UniversalIO.Print($"IsSigned: {stream.IsSigned}");
        //    UniversalIO.Print($"Is Encrypted: {stream.IsEncrypted}");
        //}
        //static void DisplayStreamProperties(SslStream stream)
        //{
        //    UniversalIO.Print($"Can read: {stream.CanRead}, write {stream.CanWrite}");
        //    UniversalIO.Print($"Can timeout: {stream.CanTimeout}");
        //}
        //static void DisplayCertificateInformation(SslStream stream)
        //{
        //    UniversalIO.Print($"Certificate revocation list checked: {stream.CheckCertRevocationStatus}"); 

        //    X509Certificate localCertificate = stream.LocalCertificate;
        //    if (stream.LocalCertificate != null)
        //    {
        //        UniversalIO.Print($"Local cert was issued to {localCertificate.Subject} and is valid from {localCertificate.GetEffectiveDateString()} until {localCertificate.GetExpirationDateString()}.");
        //    }
        //    else
        //    {
        //        UniversalIO.Print("Local certificate is null.");
        //    }
        //    // Display the properties of the client's certificate.
        //    X509Certificate remoteCertificate = stream.RemoteCertificate;
        //    if (stream.RemoteCertificate != null)
        //    {
        //        UniversalIO.Print($"Remote cert was issued to {remoteCertificate.Subject} and is valid from {remoteCertificate.GetEffectiveDateString()} until {remoteCertificate.GetExpirationDateString()}.");
        //    }
        //    else
        //    {
        //        UniversalIO.Print("Remote certificate is null.");
        //    }
        //}
        //public static bool ShowCertInfo()
        //{
        //    UniversalIO.Print("Show certificate info?  Y/N ");
        //    string r = UniversalIO.EnterMessage();
        //    if (r == "Y" || r == "y" || r == "Н" || r == "н")
        //        return true;
        //    return false;
        //}
    }






}
