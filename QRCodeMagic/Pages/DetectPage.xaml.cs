using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QRCodeMagic.Pages
{
    /// <summary>
    /// DetectPage.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class DetectPage : Page
    {
        public DetectPage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        [ObservableProperty]
        private int selectedIndex = 0;
    }
}
