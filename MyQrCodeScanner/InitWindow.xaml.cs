using System;
using System.Collections.Generic;
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

namespace MyQrCodeScanner
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        public InitWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CaptureScreen m = new CaptureScreen();
            this.Hide();
            this.Opacity = 0;
            this.Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(100);
            m.DoCapture();
            m.Show();
            this.Close();
        }

        private delegate void NoArgDelegate();
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CamWindow m=new CamWindow();
            m.Show();
            this.Close();
        }
    }
}
