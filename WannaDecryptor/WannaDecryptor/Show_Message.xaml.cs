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
    /// Show_Message.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Show_Message : Window
    {
        public Show_Message(string input)
        {
            InitializeComponent();
            Message_C.Document.Blocks.Clear();
            Message_C.Document.Blocks.Add(new Paragraph(new Run(input)));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
