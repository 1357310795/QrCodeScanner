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
using System.Diagnostics;
using MyQrCodeScanner.Views;
using MyQrCodeScanner.Modules;
using Hardcodet.Wpf.TaskbarNotification;

namespace MyQrCodeScanner
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        public IntPtr hwnd;
        public StarAnimation sa;
        private bool isshortcut;
        #endregion

        #region Constructors
        public InitWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyHook();
            PrepareHotKey();

            sa = new StarAnimation(mygrid, cv1, this);
            sa.SetStarNumber(40);
            sa.SetStarSpeed(60);
            sa.Init();
            if (GlobalSettings.isStarOn)
                sa.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GlobalSettings.SaveSettings();
            Application.Current.Shutdown();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized && GlobalSettings.hideToTray)
            {
                var myTaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
                myTaskbarIcon.ShowBalloonTip("程序将在后台运行", "若要退出：右击托盘区图标点击退出程序", BalloonIcon.Info);
                this.Hide();
            }
        }
        #endregion

        #region Main Function
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://github.com/1357310795/QrCodeScanner");
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
        {
            switch (msg)
            {
                case HotKeyManager.WM_HOTKEY:
                    isshortcut = true;
                    CaptureScreenStart();
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow w = new SettingsWindow(this);
            w.Show();
        }

        private void ApplyHook()
        {
            hwnd = ((HwndSource)PresentationSource.FromVisual(this)).Handle;
            HwndSource hWndSource = HwndSource.FromHwnd(hwnd);
            if (hWndSource != null) hWndSource.AddHook(WndProc);
        }

        private void PrepareHotKey()
        {
            try
            {
                HotKeyHelper.RegisterHotKey(new HotKeyModel() { SelectKey = GlobalSettings.selectedKey, SelectType = GlobalSettings.selectedKeyType }, this.hwnd);
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：\n" + ex.Message + "请检查快捷键是否被占用。", "注册热键失败", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion

        #region Mode 1 - Capture
        private delegate void NoArgDelegate();
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CaptureScreenStart();
        }

        private void CaptureScreenStart()
        {
            if (GlobalSettings.captureMode)
                CaptureMainScreen();
            else
                HandyScreenshot.Helpers.ScreenshotHelper.StartScreenshot();
        }

        private void CaptureMainScreen()
        {
            this.Hide();
            this.Opacity = 0;
            this.Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(200);
            var t = BitmapHelper.CaptureScreenToBitmap(0, 0,
                ScreenHelper.GetLogicalWidth(), ScreenHelper.GetLogicalHeight());
            PicWindow m = new PicWindow(t);
            m.isshortcut = isshortcut;
            m.PreScan();
            this.Opacity = 1;
            if (GlobalSettings.fastCaptureMode)
                this.Show();
            t.Dispose();
        }
        #endregion

        #region Mode 2 - Camera
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CamWindow w=new CamWindow();
            //Application.Current.MainWindow = w;
            w.Show();
            this.Hide();
        }
        #endregion

        #region Mode 3 - Picture
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var img = OpenImageFile();
            if (img != null)
            {
                PicWindow m = new PicWindow(img);
                this.Hide();
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

        #region Mode 4 - PDA
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            PDAWindow w = new PDAWindow();
            Application.Current.MainWindow = w;
            w.Show();
            this.Hide();
        }
        #endregion

        #region Engine
        public Array Engines { get { return new string[3] { "Zbar多码模式（识别率高）", "Zxing单码模式（速度快，支持格式多）", "Zxing多码模式（支持格式多）" }; } }

        public int SelectedEngine
        {
            get { return GlobalSettings.selectedengine; }
            set
            {
                GlobalSettings.selectedengine = value;
                this.RaisePropertyChanged("SelectedEngine");
                Application.Current.Resources["Engine"] = GlobalSettings.selectedengine;
                IniHelper.SetKeyValue("main", "engine", GlobalSettings.selectedengine.ToString(), IniHelper.inipath);
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
