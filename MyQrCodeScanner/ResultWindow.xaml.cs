using MaterialDesignThemes.Wpf;
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

namespace MyQrCodeScanner
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        private bool cont;
        private bool isdailog;
        public ResultWindow(string res,bool isdialog1)
        {
            InitializeComponent();
            isdailog = isdialog1;
            t1.Text = res;
            CheckURI(res);
            Snackbar1.MessageQueue= new SnackbarMessageQueue();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(t1.Text);
            Snackbar1.MessageQueue.Enqueue("已复制到剪切板！");
        }

        private void CheckURI(string s)
        {
            try
            {
                var t = new Uri(s);
            }
            catch (Exception ex)
            {
                uributton.Visibility= Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            t1.SelectAll();
            t1.Copy();
            Snackbar1.MessageQueue.Enqueue("已复制到剪切板！");
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            cont = true;
            this.Close();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isdailog)
                this.DialogResult = cont;
        }

        private void uributton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(t1.Text);
            Application.Current.Shutdown();
        }

        
    }
}
