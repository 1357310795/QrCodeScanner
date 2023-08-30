using ModernWpf.Controls.Primitives;
using ModernWpf.SampleApp;
using QRCodeMagic.Services;
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

namespace QRCodeMagic
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationRootPage page = new NavigationRootPage();
            MainContent.Content = page;
            LoadingText.Visibility = Visibility.Collapsed;

            var res = ScanService.IntiEngine();
            if (!res.success)
            {
                MessageBox.Show(res.result);
            }
        }
    }
}
