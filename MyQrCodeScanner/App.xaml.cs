using MyQrCodeScanner.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MyQrCodeScanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Environment.CommandLine.Contains("/SetAutoRunOn"))
            {
                if (!Autorun.IsSelfRun())
                {
                    var res = Autorun.SetMeStart(true);
                    Console.Out.WriteLine(res.result);
                    if (res.success)
                        App.Current.Shutdown(114514);
                    else
                        App.Current.Shutdown(-1);
                }
                App.Current.Shutdown(114514);
                return;
            }

            if (Environment.CommandLine.Contains("/SetAutoRunOff"))
            {
                if (Autorun.IsSelfRun())
                {
                    var res = Autorun.SetMeStart(false);
                    Console.Out.WriteLine(res.result);
                    if (res.success)
                        App.Current.Shutdown(114514);
                    else
                        App.Current.Shutdown(-1);
                }
                App.Current.Shutdown(114514);
                return;
            }

            ApplyTheme();
            GlobalSettings.ReadSettings();

            this.MainWindow = new InitWindow();
            this.MainWindow.Show();
        }

        private void ApplyTheme()
        {
            var isdark = Convert.ToBoolean(IniHelper.GetKeyValue("main", "isdark", "false", IniHelper.inipath));
            if (isdark)
                ThemeHelper.ApplyBase(true);

            string hue = IniHelper.GetKeyValue("main", "hue", "", IniHelper.inipath);
            if (hue != "")
                ThemeHelper.ChangeHue(hue);
        }
    }
}
