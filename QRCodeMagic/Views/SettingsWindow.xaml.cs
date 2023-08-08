﻿using CommunityToolkit.Mvvm.ComponentModel;
using NHotkey.Wpf;
using QRCodeMagic.Helpers;
using QRCodeMagic.Services;
using QRCodeMagic.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;

namespace QRCodeMagic.Views
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    [INotifyPropertyChanged]
    public partial class SettingsWindow : Window
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
                this.OnPropertyChanged("PaletteVm");
            }
        }
        public bool IsAutoRun
        {
            get { return GlobalSettings.isAutoRun; }
            set
            {
                GlobalSettings.isAutoRun = value;
                this.OnPropertyChanged("IsAutoRun");
                ChangeAutoRun();
            }
        }
        public bool IsStarOn
        {
            get { return GlobalSettings.isStarOn; }
            set
            {
                GlobalSettings.isStarOn = value;
                this.OnPropertyChanged("IsStarOn");
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
                this.OnPropertyChanged("IsDark");
                //ThemeHelper.ApplyBase(value);
            }
        }
        public bool IsChinese
        {
            get { return GlobalSettings.isChinese; }
            set
            {
                GlobalSettings.isChinese = value;
                this.OnPropertyChanged("IsChinese");
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
        public bool AllowMultipleInstances
        {
            get { return GlobalSettings.allowMultipleInstances; }
            set
            {
                GlobalSettings.allowMultipleInstances = value;
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
            IsAutoRun = AutorunHelper.IsSelfRun();
        SkipAutoRunInit:
            ;
        }

        private void ChangeAutoRun()
        {
            if (IsAutoRun != AutorunHelper.IsSelfRun())
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
                    IsAutoRun = AutorunHelper.IsSelfRun();
                    return;
                }

                if (p == null)
                {
                    MessageBox.Show(LangHelper.GetStr("ApplyFail"));
                    IsAutoRun = AutorunHelper.IsSelfRun();
                }
                p.WaitForExit();
                if (p.ExitCode != 114514 || IsAutoRun != AutorunHelper.IsSelfRun())
                {
                    MessageBox.Show(LangHelper.GetStr("ApplyFail") + "\n" + p.StandardOutput.ReadToEnd());
                    IsAutoRun = AutorunHelper.IsSelfRun();
                }
            }
        }

        private void ChangeIsStarOn()
        {
            
        }

        #region HotKey
        public Array Keys { get { return Enum.GetValues(typeof(Key)); } }
        public Array KeyTypes { get { return Enum.GetValues(typeof(ModifierKeys)); } }

        public Key SelectedKey
        {
            get { return GlobalSettings.selectedKey; }
            set { GlobalSettings.selectedKey = value; OnPropertyChanged("SelectedKey"); }
        }
        public ModifierKeys SelectedKeyType
        {
            get { return GlobalSettings.selectedKeyType; }
            set { GlobalSettings.selectedKeyType = value; OnPropertyChanged("SelectedKeyType"); }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            IniHelper.SetKeyValue("main", "EType", SelectedKeyType.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "EKey", SelectedKey.ToString(), IniHelper.inipath);

            HotkeyManager.Current.AddOrReplace("Capture", GlobalSettings.selectedKey, GlobalSettings.selectedKeyType, ((InitWindow)App.Current.MainWindow).OnHotKey);
            if (true)
                MessageBox.Show(LangHelper.GetStr("ApplySuccess"), LangHelper.GetStr("Info"), MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(LangHelper.GetStr("CheckHotKey"), LangHelper.GetStr("ErrorRegisterHotkeyFail"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        #endregion

    }
}
