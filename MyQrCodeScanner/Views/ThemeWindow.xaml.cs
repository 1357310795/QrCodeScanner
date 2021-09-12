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
    /// ThemeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ThemeWindow : Window
    {
        public ThemeWindow()
        {
            InitializeComponent();
            this.DataContext = new SkinViewModel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ((SkinViewModel)this.DataContext).OnLoaded();
        }
    }
}
