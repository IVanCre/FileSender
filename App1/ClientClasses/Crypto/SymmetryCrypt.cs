using System;
using System.IO;
using System.Security.Cryptography;

namespace FileSender
{
    class SymmetryCrypt
    {
        public static Stream Encrypt(Stream streamToEncrypt, byte[] key)
        {
            try
            {
                SymmetricAlgorithm algorithm = new RijndaelManaged();

                algorithm.Mode = CipherMode.CFB;
                algorithm.Padding = PaddingMode.PKCS7;
                byte[] _salt = new byte[] { 0x25, 0xdc, 0xff, 0x00, 0xad, 0xed, 0x7a, 0xee, 0xc5, 0xfe, 0x07, 0xaf, 0x4d, 0x08, 0x12, 0x3c };

                Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(key, _salt, 100);
                algorithm.Key = rfc.GetBytes(algorithm.KeySize / 8);
                algorithm.IV = rfc.GetBytes(algorithm.BlockSize / 8);


                ICryptoTransform encryptor = algorithm.CreateEncryptor();
                CryptoStream cryptoStream = new CryptoStream(streamToEncrypt, encryptor, CryptoStreamMode.Write);
                return cryptoStream; //этот поток уничтожится с вызывающей стороны
            }
            catch (Exception e)
            {
                UniversalIO.PrintAll(e.Message);
            }
            return null;
        }

       

        public byte[] Encrypt(byte[] dataToEncrypt, byte[] key)
        {
            MemoryStream ms = new MemoryStream(dataToEncrypt);
            Stream res = Encrypt(ms, key);
            if (res != null)
            {
                byte[] tmp = new byte[res.Length];
                res.Read(tmp, 0, tmp.Length);
                res.Close();
                ms.Close();
                return tmp;
            }

            return null;
        }


    }
}
