using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Decryptor
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private static List<string> All = new List<string>();
        private static int Max = 0;
        private static int Min = 0;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "WannaCry file (*.wncry)|*.wncry";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
                SECF1.Text = openFileDialog.FileName;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    SECF2.Text = dialog.FileName;
            }
        }

        private void SECF1_TextChanged(object sender, TextChangedEventArgs e)
        {
            isTextFFCheck();
        }

        private void SECF2_TextChanged(object sender, TextChangedEventArgs e)
        {
            isTextFFCheck();
        }

        private void isTextFFCheck()
        {
            try
            {
                if (new System.IO.FileInfo(SECF1.Text).Exists && new System.IO.DirectoryInfo(SECF2.Text).Exists)
                {
                    B1.IsEnabled = true;
                }
                else
                {
                    B1.IsEnabled = false;
                }
            }
            catch (Exception) { B1.IsEnabled = false; }
        }

        private void Directory_All_FileName(string folderName)
        {
            try
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
                foreach (System.IO.FileInfo f in di.GetFiles())
                {
                    if (f.Extension.ToLower().CompareTo(".wnry") == 0)
                        All.Add(di.FullName + "\\" + f.Name);
                    
                }
                foreach (System.IO.DirectoryInfo sd in di.GetDirectories()) Directory_All_FileName(sd.FullName);
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

        private void B1_Click(object sender, RoutedEventArgs e)
        {
            P1.Value = 0;
            P2.Text = "";
            Min = 0;
            Max = 0;
            string get_1 = File.ReadAllText(SECF1.Text);
            string get_2 = SECF2.Text;
            B1.IsEnabled = false;
            Thread th = new Thread(delegate () 
            {
                Directory_All_FileName(get_2);
                Max = All.Count;
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    P1.Maximum = Max;
                    P2.Text = Min + "/" + Max;
                }));

                foreach (string s in All)
                {
                    try
                    {
                        File_AES_Decrypt(s, s.Substring(0, s.Length - 5), get_1);
                        File.Delete(s);
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            Min = Min + 1;
                            P1.Value = P1.Value + 1;
                            P2.Text = Min + "/" + Max;
                        }));
                    }
                    catch (Exception) { }
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    B1.IsEnabled = true;
                }));
            });
            th.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
