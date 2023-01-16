using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQrCodeScanner.Modules
{
    public static class GlobalSettings
    {
        public static int selectedengine;
        public static EKey selectedKey;
        public static EType selectedKeyType;
        public static bool isStarOn;
        public static bool isAutoRun;
        public static bool captureMode;
        public static bool fastCaptureMode;
        public static bool isdark;
        public static bool isChinese;
        public static bool hideToTray;
        public static bool ignoreDup;

        public static void ReadSettings()
        {
            selectedengine = Convert.ToInt32(IniHelper.GetKeyValue("main", "engine", "0", IniHelper.inipath));
            selectedKeyType = (EType)Enum.Parse(typeof(EType), IniHelper.GetKeyValue("main", "EType", "Alt", IniHelper.inipath));
            selectedKey = (EKey)Enum.Parse(typeof(EKey), IniHelper.GetKeyValue("main", "EKey", "Z", IniHelper.inipath));
            isStarOn = Convert.ToBoolean(IniHelper.GetKeyValue("main", "IsStarOn", "true", IniHelper.inipath));
            isdark = Convert.ToBoolean(IniHelper.GetKeyValue("main", "isdark", "false", IniHelper.inipath));
            isChinese = Convert.ToBoolean(IniHelper.GetKeyValue("main", "isChinese", "true", IniHelper.inipath));
            captureMode = Convert.ToBoolean(IniHelper.GetKeyValue("main", "captureMode", "false", IniHelper.inipath));
            fastCaptureMode = Convert.ToBoolean(IniHelper.GetKeyValue("main", "fastCaptureMode", "false", IniHelper.inipath));
            hideToTray = Convert.ToBoolean(IniHelper.GetKeyValue("main", "hideToTray", "false", IniHelper.inipath));
            ignoreDup = Convert.ToBoolean(IniHelper.GetKeyValue("main", "ignoreDup", "false", IniHelper.inipath));
        }

        public static void SaveSettings()
        {
            IniHelper.SetKeyValue("main", "engine", selectedengine.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "IsStarOn", isStarOn.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "captureMode", captureMode.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "fastCaptureMode", fastCaptureMode.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "hideToTray", hideToTray.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "ignoreDup", ignoreDup.ToString(), IniHelper.inipath);
            IniHelper.SetKeyValue("main", "isChinese", isChinese.ToString(), IniHelper.inipath);
        }
    }
}
