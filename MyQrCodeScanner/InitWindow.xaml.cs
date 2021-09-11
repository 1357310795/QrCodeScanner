using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
using WPFCaptureScreenShot;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Interop;
using MyQrCodeScanner.Modules;

namespace MyQrCodeScanner
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window, INotifyPropertyChanged
    {
        #region private field
        private IntPtr hwnd;
        #endregion

        #region 构造函数
        public InitWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region 主功能
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            hwnd = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(hwnd);
            if (hWndSource != null) hWndSource.AddHook(WndProc);
            EType type = EType.Alt;
            EKey eKey = EKey.Z;
            Enum.TryParse<EType>(IniHelper.GetKeyValue("main", "EType", "Alt", IniHelper.inipath), true, out type);
            Enum.TryParse<EKey>(IniHelper.GetKeyValue("main", "EKey", "Z", IniHelper.inipath), true, out eKey);
            SelectKey = eKey;
            SelectType = type;
            try
            {
                RegisterHotkey(new HotKeyModel() { SelectKey = eKey, SelectType = type });
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：\n" + ex.Message + "请检查快捷键是否被占用。", "注册热键失败", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/1357310795/QrCodeScanner");
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:
                    //Console.WriteLine("ok");
                    CaptureScreenStart();
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region 截图识别
        private delegate void NoArgDelegate();
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CaptureScreenStart();
        }

        private void CaptureScreenStart()
        {
            this.Hide();
            this.Opacity = 0;
            this.Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(200);
            var t = BitmapHelper.CaptureScreenToBitmap(0, 0,
                ScreenHelper.GetLogicalWidth(), ScreenHelper.GetLogicalHeight());
            PicWindow m = new PicWindow(t);
            m.Show();
            this.Close();
        }
        #endregion

        #region 摄像头识别
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CamWindow m=new CamWindow();
            m.Show();
            this.Close();
        }
        #endregion

        #region 本地图片识别
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "All documents (*.*)|*.*",
                Multiselect=false
            };
            if (dlg.ShowDialog(this).GetValueOrDefault(false))
            {
                var tmp = dlg.FileName;
                if (File.Exists(tmp))
                {
                    BitmapImage s;
                    try
                    {
                        s = new BitmapImage(new Uri(tmp));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message,"错误");
                        return;
                    }
                    
                    PicWindow m = new PicWindow(s);
                    m.Show();
                    this.Close();
                }
            }
            
        }

        #endregion

        #region 快捷键设置
        public Array Keys { get { return Enum.GetValues(typeof(EKey)); } }

        public Array Types { get { return Enum.GetValues(typeof(EType)); } }

        private EKey _selectKey;
        private EType _selectType;
        public EKey SelectKey
        {
            get { return _selectKey; }
            set { _selectKey = value; RaisePropertyChanged("SelectKey"); }
        }

        public EType SelectType
        {
            get { return _selectType; }
            set { _selectType = value; RaisePropertyChanged("SelectType"); }
        }
        
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            IniHelper.SetKeyValue("main", "EType", SelectType.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "EKey", SelectKey.ToString(), IniHelper.inipath);
            RegisterHotkey(new HotKeyModel()
                            {
                                SelectKey = SelectKey,
                                SelectType = SelectType
                            });
            MessageBox.Show("设置成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool RegisterHotkey(HotKeyModel h)
        {
            return HotKeyHelper.RegisterHotKey(h, hwnd);
        }


        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
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
