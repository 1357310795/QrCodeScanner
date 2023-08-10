using CommunityToolkit.Mvvm.ComponentModel;
using QRCodeMagic.Helpers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QRCodeMagic.Views
{
    public delegate void ClosePanelEventHandler(Object sender);

    [INotifyPropertyChanged]
    public partial class GoPanel : UserControl
    {
        #region 构造函数
        public GoPanel()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region 属性
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

        #endregion

        #region 主功能
        private void ButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Data);
            ((Button)sender).Content = LangHelper.GetStr("CopiedToClipBoard");
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Data);
        }
        #endregion

        #region 打开、关闭动画相关
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            text1.Focus();
            var da = AnimationHelper.CubicBezierDoubleAnimation(TimeSpan.FromSeconds(0.25), 0, 1, "0,.72,.57,1.2");
            ScaleTransform s = new ScaleTransform(1, 1);
            this.RenderTransform = s;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            s.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            s.BeginAnimation(ScaleTransform.ScaleYProperty, da);
        }

        private bool autoclose = true;
        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            OnClose();
        }

        public event ClosePanelEventHandler ClosePanel;
        private void RaiseClosePanel()
        {
            //Console.WriteLine("关闭panel");
            ClosePanel?.Invoke(this);
        }

        public void OnClose()
        {
            var da = AnimationHelper.CubicBezierDoubleAnimation(TimeSpan.FromSeconds(0.15), 1, 0, ".61,.01,.87,.62");
            da.Completed += Ani_Completed;
            ScaleTransform s = new ScaleTransform(1, 1);
            this.RenderTransform = s;
            this.RenderTransformOrigin = new Point(0.5, 0.5);
            s.BeginAnimation(ScaleTransform.ScaleXProperty, da);
            s.BeginAnimation(ScaleTransform.ScaleYProperty, da);
        }

        private void Ani_Completed(object sender, EventArgs e)
        {
            RaiseClosePanel();
        }
        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            Console.WriteLine("TextBox_GotMouseCapture");
            autoclose = false;
            ButtonClose.Visibility = Visibility.Visible;
        }
        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (autoclose) OnClose();
        }
        #endregion
    }
}
