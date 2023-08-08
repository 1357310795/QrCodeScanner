using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Interop;
using System.Diagnostics;
using Hardcodet.Wpf.TaskbarNotification;
using QRCodeMagic.Helpers;
using QRCodeMagic.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using HandyScreenshot.Core.Helpers;
using MonitorHelper = QRCodeMagic.Helpers.MonitorHelper;
using QRCodeMagic.Views;
using NHotkey.Wpf;
using NHotkey;
using HandyScreenshot.Core.Common;
using HandyScreenshot.Core.Interop;
using Teru.Code.Models;
using System.Linq;

namespace QRCodeMagic
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    [INotifyPropertyChanged]
    public partial class InitWindow : Window
    {
        #region Fields
        public nint hwnd;
        private bool isshortcut;
        #endregion

        #region Constructors
        public InitWindow()
        {
            InitializeComponent();
            WindowPlacementHelper.ReadPlacement(this);
            DataContext = this;
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PrepareHotKey();
            ScreenshotHelper.CaptureOK += ScreenshotHelper_CaptureOK;
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            GlobalSettings.SaveSettings();
            WindowPlacementHelper.SavePlacement(this);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && GlobalSettings.hideToTray)
            {
                var myTaskbarIcon = (TaskbarIcon)FindResource("Taskbar");
                myTaskbarIcon.ShowBalloonTip(LangHelper.GetStr("RunInBackTipTitle"), LangHelper.GetStr("RunInBackTipText"), BalloonIcon.Info);
                Hide();
            }
        }
        #endregion

        #region Main Function
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer", "https://github.com/1357310795/QRCodeMagic");
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow w = new SettingsWindow(this);
            w.Show();
        }

        private void PrepareHotKey()
        {
            try
            {
                HotkeyManager.Current.AddOrReplace("Capture", GlobalSettings.selectedKey, GlobalSettings.selectedKeyType, OnHotKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show(LangHelper.GetStr("Error") + "\n" + ex.Message + LangHelper.GetStr("ErrorHotkeyUsed"), LangHelper.GetStr("ErrorRegisterHotkeyFail"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void OnHotKey(object sender, HotkeyEventArgs e)
        {
            isshortcut = true;
            CaptureScreenStart();
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
                ScreenshotHelper.StartScreenshot();
        }

        private static void SetWindowRect(Window window, ReadOnlyRect rect)
        {
            NativeMethods.SetWindowPos(
                window.GetHandle(),
                (IntPtr)NativeMethods.HWND_TOPMOST,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                NativeMethods.SWP_NOZORDER);
        }
        private void ScreenshotHelper_CaptureOK(bool ok, HandyScreenshot.Core.Views.ClipWindow window)
        {
            //this.Dispatcher.Invoke(() => { window.Close(); });

            Application.Current.Dispatcher.Invoke(() =>
            {
                window.Content = new AreaScanUC();
                SetWindowRect(window, window.State.ScreenshotRect.ToReadOnlyRect());
            });

          
            var res = PicDecode(window.AreaImage);
            if (res.success)
            {
                //window.Result = ;
                Clipboard.SetText(res.result);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Content = new AreaScanSuccUC();
                });
            }
            else
            {
                //Result = res.result;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Content = new AreaScanFailUC();
                });
            }
            window.MouseLeftButtonDown += delegate { window.DragMove(); };
            window.Topmost = false;
            window.ResizeMode = System.Windows.ResizeMode.NoResize;

            //if (GlobalSettings.fastCaptureMode)
            //{
            //    var da = AnimationHelper.CubicBezierDoubleAnimation(
            //        TimeSpan.FromSeconds(2),
            //        TimeSpan.FromSeconds(0.8),
            //        1, 0, ".38,.02,.6,.99");
            //    da.Completed += window.CloseAnimation_Completed;
            //    this.BeginAnimation(Window.OpacityProperty, da);
            //}
        }

        private CommonResult PicDecode(BitmapSource image)
        {
            var t1 = DateTime.Now.Ticks;

            var res = ScanService.ScanCode(image);
            var t2 = DateTime.Now.Ticks;
            //Console.WriteLine("识别用时：" + ((t2 - t1) / 10000).ToString() + "ms");

            switch (res.status)
            {
                case result_status.error:
                    return new CommonResult(false, res.data[0].data);
                case result_status.ok:
                    return new CommonResult(true, res.data.Select(c => c.data).Aggregate((a, b) => a + "\n" + b));
                case result_status.nocode:
                    return new CommonResult(false, LangHelper.GetStr("NoCode"));
            }
            return new CommonResult(false, LangHelper.GetStr("SystemError"));
        }

        private void CaptureMainScreen()
        {
            Hide();
            Opacity = 0;
            Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(200);
            var t = CaptureScreenHelper.CaptureScreenToBitmap(0, 0,
                MonitorHelper.GetLogicalWidth(), MonitorHelper.GetLogicalHeight());
            PicWindow m = new PicWindow(t);
            m.isshortcut = isshortcut;
            m.PreScan();
            Opacity = 1;
            if (GlobalSettings.fastCaptureMode)
                Show();
            //t.Dispose();
        }

        #endregion

        #region Mode 2 - Camera
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CamWindow w = new CamWindow();
            //Application.Current.MainWindow = w;
            w.Show();
            Hide();
        }
        #endregion

        #region Mode 3 - Picture
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var img = OpenImageFile();
            if (img != null)
            {
                PicWindow m = new PicWindow(img);
                Hide();
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
                        MessageBox.Show(this, ex.Message, LangHelper.GetStr("InternalError"));
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
            Hide();
        }
        #endregion

        #region Engine
        public Array Engines
        {
            get
            {
                return new string[4] {
                    LangHelper.GetStr("ZbarMulti"),
                    LangHelper.GetStr("ZXingSingle"),
                    LangHelper.GetStr("ZXingMulti"),
                    LangHelper.GetStr("OpenCV")
                };
            }
        }

        public int SelectedEngine
        {
            get { return GlobalSettings.selectedengine; }
            set
            {
                GlobalSettings.selectedengine = value;
                OnPropertyChanged("SelectedEngine");
                Application.Current.Resources["Engine"] = GlobalSettings.selectedengine;
                IniHelper.SetKeyValue("main", "engine", GlobalSettings.selectedengine.ToString(), IniHelper.inipath);
            }
        }

        #endregion
    }
}
