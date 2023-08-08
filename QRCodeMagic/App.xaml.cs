using QRCodeMagic.Helpers;
using QRCodeMagic.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QRCodeMagic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MutexHelper mutex = new MutexHelper();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Environment.CommandLine.Contains("/SetAutoRunOn"))
            {
                if (!AutorunHelper.IsSelfRun())
                {
                    var res = AutorunHelper.SetMeStart(true);
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
                if (AutorunHelper.IsSelfRun())
                {
                    var res = AutorunHelper.SetMeStart(false);
                    Console.Out.WriteLine(res.result);
                    if (res.success)
                        App.Current.Shutdown(114514);
                    else
                        App.Current.Shutdown(-1);
                }
                App.Current.Shutdown(114514);
                return;
            }

            GlobalSettings.ReadSettings();
            LangHelper.ChangeLang(GlobalSettings.isChinese);
            if (!GlobalSettings.allowMultipleInstances)
            {
                if (mutex.IsRunning)
                {
                    MessageBox.Show(LangHelper.GetStr("MutexAlertText"));
                    Environment.Exit(0);
                    return;
                }
            }

            this.MainWindow = new InitWindow();
            this.MainWindow.Show();
        }
    }
}
