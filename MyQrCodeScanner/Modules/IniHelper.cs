using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyQrCodeScanner.Modules
{
    public class IniHelper
    {
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Ansi, EntryPoint = "GetPrivateProfileStringA", ExactSpelling = true, SetLastError = true)]
        public static extern int GetPrivateProfileString([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpApplicationName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpKeyName, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpDefault, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpReturnedString, int nSize, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpFileName);

        public static string GetKeyValue(string sectionName, string keyName, string defaultText, string filename)
        {
            int BufferSize = 255;
            string keyValue = Strings.Space(BufferSize);
            string text = "";
            int Rvalue = IniHelper.GetPrivateProfileString(ref sectionName, ref keyName, ref text, ref keyValue, BufferSize, ref filename);
            bool flag = Rvalue == 0;
            if (flag)
            {
                keyValue = defaultText;
            }
            else
            {
                keyValue = IniHelper.GetIniValue(keyValue);
            }
            return keyValue;
        }

        public static string GetIniValue(string msg)
        {
            int PosChr0 = msg.IndexOf('\0');
            bool flag = PosChr0 != -1;
            if (flag)
            {
                msg = msg.Substring(0, PosChr0);
            }
            return msg;
        }

        public static bool SetKeyValue(string Section, string Key, string Value, string iniFilePath)
        {
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
            long OpStation = IniHelper.WritePrivateProfileString(Section, Key, Value, iniFilePath);
            bool flag3 = OpStation == 0L;
            return !flag3;
        }

        public static string inipath = Environment.GetEnvironmentVariable("LocalAppData") + "\\QrCodeScanner\\Settings.ini";
    }
}
