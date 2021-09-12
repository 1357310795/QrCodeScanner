using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

            ApplyTheme();
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
