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
            StringBuilder res = new StringBuilder();
            for (int i = 0; i < l.Count; i++)
            {
                res.Append(l[i].data);
                if (i != l.Count - 1)
                    res.Append("\n");
            }
            return res.ToString();
        }
    }
    
}
