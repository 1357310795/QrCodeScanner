using CommunityToolkit.Mvvm.ComponentModel;
using QRCodeMagic.Helpers;
using System;
using System.ComponentModel;
using System.Windows;

namespace QRCodeMagic.Views
{
    /// <summary>
    /// Interaction logic for ResultWindow.xaml
    /// </summary>
    [INotifyPropertyChanged]
    public partial class ResultWindow : Window
    {
        #region Constructors
        public ResultWindow(string res,string type,bool isdialog1,bool cancontinue)
        {
            InitializeComponent();
            this.DataContext = this;
            isdailog = isdialog1;
            Data = res;
            if (!CheckURI(res))
                grid1.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Star);
            CodeType = type;
            if (!cancontinue)
                grid1.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Star);
        }
        #endregion

        #region Fields
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
                this.OnPropertyChanged("Data");
            }
        }

        private bool cont;
        private bool isdailog;
        #endregion

        #region Main Function
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
            text1.SelectAll();
            text1.Copy();
            ButtonCopy.Style = (Style)Application.Current.Resources["MaterialDesignRaisedButton"];
            ButtonCopyText.Text = LangHelper.GetStr("Copied");
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            cont = true;
            this.Close();
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(text1.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show(LangHelper.GetStr("Error") + "：\n" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            this.Close(); 
            //Application.Current.Shutdown();
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(text1.Text);
            this.Activate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isdailog)
                this.DialogResult = cont;
        }
        #endregion
    }
}
