using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQrCodeScanner
{
    public class stringhelper
    {
        public static string ResListToString(List<CodeWithLocation> l)
        {
            string res = "";
            for (int i = 0; i < l.Count; i++)
            {
                res += l[i].data;
                if (i != l.Count - 1)
                    res += "\n";
            }
            return res;
        }
    }
    
}
