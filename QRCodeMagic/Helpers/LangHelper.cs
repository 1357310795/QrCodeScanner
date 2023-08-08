using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeMagic.Helpers
{
    public static class LangHelper
    {
        public static void ChangeLang(bool chn)
        {
            System.Windows.Application.Current.Resources.MergedDictionaries[0] = new System.Windows.ResourceDictionary()
            {
                Source = new Uri(@"pack://application:,,,/QRCodeMagic;component/Resources/" + (!chn ? "en-us" : "zh-cn") + ".xaml", UriKind.RelativeOrAbsolute)
            };
        }

        public static string GetStr(string key)
        {
            var dic = System.Windows.Application.Current.Resources.MergedDictionaries[0];
            return dic[key] as string;
        }
    }
}
