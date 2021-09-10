using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MyQrCodeScanner
{
    /// <summary>
    /// GoPanel.xaml 的交互逻辑
    /// </summary>
    public partial class GoPanel : UserControl, INotifyPropertyChanged
    {
        public GoPanel()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private string codetype;

        public string CodeType
        {
            get { return codetype; }
            set
            {
                codetype = value;
                this.OnPropertyChanged("CodeType");
            }
        }

        private string data;

        public string Data
        {
            get { return data; }
            set
            {
                data = value;
                if (CheckURI(data))
                {
                    text_panel.Visibility = Visibility.Collapsed;
                    uri_panel.Visibility = Visibility.Visible;
                }
                else
                {
                    text_panel.Visibility = Visibility.Visible;
                    uri_panel.Visibility = Visibility.Collapsed;
                }
                this.OnPropertyChanged("Data");
            }
        }

        private bool CheckURI(string s)
        {
            try
            {
                var t = new Uri(s);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }


        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Data);
            ((Button)sender).Content = "已复制到剪切板";
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Data);
        }

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion

    }
}
