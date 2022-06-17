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
using System.Security.Principal;

namespace MyQrCodeScanner
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private IntPtr hwnd;
        private bool isAutoRun;
        public bool IsAutoRun
        {
            get { return isAutoRun; }
            set
            {
                isAutoRun = value;
                this.RaisePropertyChanged("IsAutoRun");
                ChangeAutoRun();
            }
        }
        private bool isStarOn;
        public bool IsStarOn
        {
            get { return isStarOn; }
            set
            {
                isStarOn = value;
                this.RaisePropertyChanged("IsStarOn");
                ChangeIsStarOn();
            }
        }

        StarAnimation sa;

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
            Thread t = new Thread(PrepareHotKey);
            t.Start();

            if (Environment.OSVersion.Version.Major == 11)
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                string computerName = id.Name;
                WindowsPrincipal principal = new WindowsPrincipal(id);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    ToggleAutoRun.IsEnabled = false;
                    ToggleAutoRun.ToolTip = "在当前系统环境下，此选项不可用。请尝试以管理员权限运行程序。";
                    goto SkipAutoRunInit;
                }
                
            }
            IsAutoRun = Autorun.IsSelfRun();
            SkipAutoRunInit :

            sa = new StarAnimation(mygrid, cv1, this);
            sa.SetStarNumber(40);
            sa.SetStarSpeed(60);
            sa.Init();

            ReadSettings();
            RegisterHotkey(new HotKeyModel()
            {
                SelectKey = SelectKey,
                SelectType = SelectType
            });

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveSettings();
        }
        #endregion

        #region Main Function
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer", "https://github.com/1357310795/QrCodeScanner");
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

        private void ChangeAutoRun()
        {
            if (IsAutoRun != Autorun.IsSelfRun())
            {
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.Verb = "runas";
                processInfo.LoadUserProfile = true;
                processInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                processInfo.Arguments = "/SetAutoRun" + (IsAutoRun ? "On" : "Off");
                processInfo.RedirectStandardOutput = true;
                processInfo.UseShellExecute = false;
                Process p = null;
                try
                {
                    p = Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("设置失败\n" + ex.Message);
                    IsAutoRun = Autorun.IsSelfRun();
                    return;
                }

                if (p == null)
                {
                    MessageBox.Show("设置失败");
                    IsAutoRun = Autorun.IsSelfRun();
                }
                p.WaitForExit();
                if (p.ExitCode != 114514 || IsAutoRun != Autorun.IsSelfRun())
                {
                    MessageBox.Show("设置失败\n" + p.StandardOutput.ReadToEnd());
                    IsAutoRun = Autorun.IsSelfRun();
                }
            }
        }

        private void ChangeIsStarOn()
        {
            if (IsStarOn)
            {
                sa.Start();
            }
            else
                sa.Pause();
        }

        private void ReadSettings()
        {
            SelectedEngine = Convert.ToInt32(IniHelper.GetKeyValue("main", "engine", "0", IniHelper.inipath));
            SelectType = (EType)Enum.Parse(typeof(EType), IniHelper.GetKeyValue("main", "EType", "Alt", IniHelper.inipath));
            SelectKey = (EKey)Enum.Parse(typeof(EKey), IniHelper.GetKeyValue("main", "EKey", "Z", IniHelper.inipath));
            IsStarOn = Convert.ToBoolean(IniHelper.GetKeyValue("main", "IsStarOn", "true", IniHelper.inipath));
        }

        private void SaveSettings()
        {
            IniHelper.SetKeyValue("main", "engine", SelectedEngine.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "IsStarOn", IsStarOn.ToString(), IniHelper.inipath);
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
            this.Hide();
            this.Opacity = 0;
            this.Dispatcher.Invoke(DispatcherPriority.Render, (NoArgDelegate)delegate { });
            Thread.Sleep(200);
            var t = BitmapHelper.CaptureScreenToBitmap(0, 0,
                ScreenHelper.GetLogicalWidth(), ScreenHelper.GetLogicalHeight());
            PicWindow m = new PicWindow(t);
            m.PreScan();
            this.Opacity = 1;
            //m.Focus();
            //this.Hide();
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
            this.Close();
        }
        #endregion

        #region HotKey
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

        #region Engine
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
