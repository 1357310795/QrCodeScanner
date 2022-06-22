using MyQrCodeScanner.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Principal;
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

namespace MyQrCodeScanner.Views
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        #region Constructors
        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public SettingsWindow(InitWindow initWindow)
        {
            InitializeComponent();
            mw = initWindow;
            this.DataContext = this;
        }
        #endregion

        private SkinViewModel paletteVm;
        public SkinViewModel PaletteVm
        {
            get { return paletteVm; }
            set
            {
                paletteVm = value;
                this.RaisePropertyChanged("PaletteVm");
            }
        }
        public bool IsAutoRun
        {
            get { return GlobalSettings.isAutoRun; }
            set
            {
                GlobalSettings.isAutoRun = value;
                this.RaisePropertyChanged("IsAutoRun");
                ChangeAutoRun();
            }
        }
        public bool IsStarOn
        {
            get { return GlobalSettings.isStarOn; }
            set
            {
                GlobalSettings.isStarOn = value;
                this.RaisePropertyChanged("IsStarOn");
                ChangeIsStarOn();
            }
        }
        public bool CaptureMode
        {
            get { return GlobalSettings.captureMode; }
            set
            {
                GlobalSettings.captureMode = value;
            }
        }
        public bool FastCaptureMode
        {
            get { return GlobalSettings.fastCaptureMode; }
            set
            {
                GlobalSettings.fastCaptureMode = value;
            }
        }
        public bool HideToTray
        {
            get { return GlobalSettings.hideToTray; }
            set
            {
                GlobalSettings.hideToTray = value;
            }
        }
        public bool IsDark
        {
            get { return GlobalSettings.isdark; }
            set
            {
                GlobalSettings.isdark = value;
                this.RaisePropertyChanged("IsDark");
                ThemeHelper.ApplyBase(value);
            }
        }
        public bool IgnoreDup
        {
            get { return GlobalSettings.ignoreDup; }
            set
            {
                GlobalSettings.ignoreDup = value;
            }
        }

        private InitWindow mw;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PaletteVm = new SkinViewModel();
            PaletteVm.OnLoaded();
            
            bool win11 = false;
            ManagementClass manag = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection managCollection = manag.GetInstances();
            foreach (ManagementObject m in managCollection)
            {
                if (m["Name"].ToString().Contains("Windows 11"))
                    win11 = true;
            }
            if (win11)
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                string computerName = id.Name;
                WindowsPrincipal principal = new WindowsPrincipal(id);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    ToggleAutoRun.IsEnabled = false;
                    TextAutoRun.ToolTip = "在当前系统环境下，此选项不可用。请尝试以管理员权限运行程序。";
                    goto SkipAutoRunInit;
                }

            }
            IsAutoRun = Autorun.IsSelfRun();
        SkipAutoRunInit:
            ;
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
                mw.sa.Start();
            }
            else
                mw.sa.Pause();
        }

        #region HotKey
        public Array Keys { get { return Enum.GetValues(typeof(EKey)); } }
        public Array KeyTypes { get { return Enum.GetValues(typeof(EType)); } }

        public EKey SelectedKey
        {
            get { return GlobalSettings.selectedKey; }
            set { GlobalSettings.selectedKey = value; RaisePropertyChanged("SelectedKey"); }
        }
        public EType SelectedKeyType
        {
            get { return GlobalSettings.selectedKeyType; }
            set { GlobalSettings.selectedKeyType = value; RaisePropertyChanged("SelectedKeyType"); }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            IniHelper.SetKeyValue("main", "EType", SelectedKeyType.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "EKey", SelectedKey.ToString(), IniHelper.inipath);

            var res = HotKeyHelper.RegisterHotKey(new HotKeyModel()
            {
                SelectKey = SelectedKey,
                SelectType = SelectedKeyType
            }, mw.hwnd);
            if (res)
                MessageBox.Show("设置成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("注册热键失败！请检查快捷键是否被占用。", "注册热键失败", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
