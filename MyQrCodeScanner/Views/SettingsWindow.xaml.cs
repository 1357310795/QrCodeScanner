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
        public bool IsChinese
        {
            get { return GlobalSettings.isChinese; }
            set
            {
                GlobalSettings.isChinese = value;
                this.RaisePropertyChanged("IsChinese");
                LangHelper.ChangeLang(value);
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

        public bool IsInputEnter
        {
            get { return GlobalSettings.isInputEnter; }
            set
            {
                GlobalSettings.isInputEnter = value;
            }
        }

        private InitWindow mw;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PaletteVm = new SkinViewModel();
            PaletteVm.OnLoaded();
            
            
            if (OSVersionHelper.IsWindows11OrGreater)
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                string computerName = id.Name;
                WindowsPrincipal principal = new WindowsPrincipal(id);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    ToggleAutoRun.IsEnabled = false;
                    TextAutoRun.ToolTip = LangHelper.GetStr("Win11Issue");
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
                    MessageBox.Show(LangHelper.GetStr("ApplyFail") + "\n" + ex.Message);
                    IsAutoRun = Autorun.IsSelfRun();
                    return;
                }

                if (p == null)
                {
                    MessageBox.Show(LangHelper.GetStr("ApplyFail"));
                    IsAutoRun = Autorun.IsSelfRun();
                }
                p.WaitForExit();
                if (p.ExitCode != 114514 || IsAutoRun != Autorun.IsSelfRun())
                {
                    MessageBox.Show(LangHelper.GetStr("ApplyFail") + "\n" + p.StandardOutput.ReadToEnd());
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
                MessageBox.Show(LangHelper.GetStr("ApplySuccess"), LangHelper.GetStr("Info"), MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(LangHelper.GetStr("CheckHotKey"), LangHelper.GetStr("ErrorRegisterHotkeyFail"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
