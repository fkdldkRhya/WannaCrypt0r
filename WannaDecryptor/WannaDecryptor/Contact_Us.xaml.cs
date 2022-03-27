using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WannaDecryptor
{
    /// <summary>
    /// Contact_Us.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Contact_Us : Window
    {
        public Contact_Us()
        {
            InitializeComponent();
        }

        private string _WriteMessage = "";
        public string WriteMessage()
        {
            return _WriteMessage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _WriteMessage = "";
            _WriteMessage = new TextRange(Message_C.Document.ContentStart, Message_C.Document.ContentEnd).Text;
            this.Close();
        }
    }
}
