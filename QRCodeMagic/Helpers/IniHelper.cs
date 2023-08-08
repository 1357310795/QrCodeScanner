using Microsoft.VisualBasic;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QRCodeMagic.Helpers
{
    public class IniHelper
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        public static string GetKeyValue(string sectionName, string keyName, string defaultText, string filename = "DEFAULT")
        {
            if (filename == "DEFAULT") filename = inipath;
            int BufferSize = 255;
            StringBuilder keyValue = new StringBuilder(BufferSize);
            string text = "";
            uint Rvalue = GetPrivateProfileString(sectionName, keyName, text, keyValue, (uint)BufferSize, filename);
            bool flag = Rvalue == 0;
            if (flag)
            {
                return defaultText;
            }
            else
            {
                return keyValue.ToString();
            }
        }

        public static bool SetKeyValue(string Section, string Key, string Value, string iniFilePath = "DEFAULT")
        {
            if (iniFilePath == "DEFAULT") iniFilePath = inipath;
            string pat = Path.GetDirectoryName(iniFilePath);
            bool flag = !Directory.Exists(pat);
            if (flag)
            {
                Directory.CreateDirectory(pat);
            }
            bool flag2 = !File.Exists(iniFilePath);
            if (flag2)
            {
                File.Create(iniFilePath).Close();
            }
            bool r = WritePrivateProfileString(Section, Key, Value, iniFilePath);
            return r;
        }

        public static string inipath = Path.Combine(PathHelper.AppDataPath ,"Settings.ini");
    }
}
