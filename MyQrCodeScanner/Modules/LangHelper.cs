using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQrCodeScanner.Modules
{
    public static class LangHelper
    {
        public static void ChangeLang(bool chn)
        {
            App.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary()
            {
                Source = new Uri((@"pack://application:,,,/MyQrCodeScanner;component/Resources/" + (!chn ? "en-us" : "zh-cn") + ".xaml"), UriKind.RelativeOrAbsolute)
            };
        }

        public static string GetStr(string key)
        {
            var dic = App.Current.Resources.MergedDictionaries[0];
            return dic[key] as string;
        }
    }
}
