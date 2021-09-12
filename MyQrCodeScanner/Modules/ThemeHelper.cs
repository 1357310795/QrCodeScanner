using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyQrCodeScanner
{
    class ThemeHelper
    {
        static PaletteHelper _paletteHelper;

        public static void ApplyBase(object isDark)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme((bool)isDark ? Theme.Dark : Theme.Light);
            paletteHelper.SetTheme(theme);
            IniHelper.SetKeyValue("main", "isdark", ((bool)isDark).ToString(), IniHelper.inipath);
        }

        public static void ChangeHue(object obj)
        {
            var hue = StringToColor(obj.ToString());
            _paletteHelper = new PaletteHelper();
            _paletteHelper.ChangePrimaryColor(hue);
            IniHelper.SetKeyValue("main", "hue", hue.ToString(), IniHelper.inipath);
        }
        public static Color StringToColor(string colorStr)
        {
            TypeConverter cc = TypeDescriptor.GetConverter(typeof(Color));
            var result = (Color)cc.ConvertFromString(colorStr);
            return result;
        }
    }
}
