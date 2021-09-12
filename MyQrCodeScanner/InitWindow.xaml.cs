using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WPFCaptureScreenShot;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Interop;

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
            ApplyHook();
            Thread t = new Thread(PrepareHotKey);
            t.Start();
            SelectedEngine=Convert.ToInt32(IniHelper.GetKeyValue("main", "engine", "0", IniHelper.inipath));
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

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            ThemeWindow w = new ThemeWindow();
            w.Show();
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
            m.PreScan();
            this.Close();
        }
        #endregion

        #region 摄像头识别
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CamWindow w=new CamWindow();
            Application.Current.MainWindow = w;
            w.Show();
            this.Close();
        }
        #endregion

        #region 本地图片识别
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var img = OpenImageFile();
            if (img != null)
            {
                PicWindow m = new PicWindow(img);
                this.Close();
                m.PreScan();
            }
        }

        private BitmapImage OpenImageFile()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "All documents (*.*)|*.*",
                Multiselect = false
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
                        MessageBox.Show(this, ex.Message, "内部错误");
                        return null;
                    }

                    return s;
                }
                else
                    return null;
            }
            else
                return null;
        }

        #endregion

        #region 扫描枪
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            PDAWindow w = new PDAWindow();
            Application.Current.MainWindow = w;
            w.Show();
            this.Close();
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

        private void ApplyHook()
        {
            hwnd = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(hwnd);
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        private void PrepareHotKey()
        {

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

        #region 引擎设置
        public Array Engines { get { return new string[3] { "Zbar多码模式（识别率高）", "Zxing单码模式（速度快，支持格式多）", "Zxing多码模式（支持格式多）" }; } }
        private int selectedengine;
        public int SelectedEngine
        {
            get { return selectedengine; }
            set
            {
                selectedengine = value;
                this.RaisePropertyChanged("SelectedEngine");
                Application.Current.Resources["Engine"] = selectedengine;
                IniHelper.SetKeyValue("main", "engine", selectedengine.ToString(), IniHelper.inipath);
            }
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
