using QRCodeMagic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

//Todo:Refractor
namespace QRCodeMagic.Services
{
    public static class GlobalSettings
    {
        public static int selectedengine;
        public static Key selectedKey;
        public static ModifierKeys selectedKeyType;
        public static bool isStarOn;
        public static bool isAutoRun;
        public static bool captureMode;
        public static bool fastCaptureMode;
        public static bool isdark;
        public static bool isChinese;
        public static bool hideToTray;
        public static bool ignoreDup;
        public static bool allowMultipleInstances;

        public static void ReadSettings()
        {
            selectedengine = Convert.ToInt32(IniHelper.GetKeyValue("main", "engine", "0", IniHelper.inipath));
            selectedKeyType = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), IniHelper.GetKeyValue("main", "EType", "Alt", IniHelper.inipath));
            selectedKey = (Key)Enum.Parse(typeof(Key), IniHelper.GetKeyValue("main", "EKey", "Z", IniHelper.inipath));
            isStarOn = Convert.ToBoolean(IniHelper.GetKeyValue("main", "IsStarOn", "true", IniHelper.inipath));
            isdark = Convert.ToBoolean(IniHelper.GetKeyValue("main", "isdark", "false", IniHelper.inipath));
            isChinese = Convert.ToBoolean(IniHelper.GetKeyValue("main", "isChinese", "true", IniHelper.inipath));
            captureMode = Convert.ToBoolean(IniHelper.GetKeyValue("main", "captureMode", "false", IniHelper.inipath));
            fastCaptureMode = Convert.ToBoolean(IniHelper.GetKeyValue("main", "fastCaptureMode", "false", IniHelper.inipath));
            hideToTray = Convert.ToBoolean(IniHelper.GetKeyValue("main", "hideToTray", "false", IniHelper.inipath));
            ignoreDup = Convert.ToBoolean(IniHelper.GetKeyValue("main", "ignoreDup", "false", IniHelper.inipath));
            allowMultipleInstances = Convert.ToBoolean(IniHelper.GetKeyValue("main", "allowMultipleInstances", "false", IniHelper.inipath));
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
            IniHelper.SetKeyValue("main", "allowMultipleInstances", allowMultipleInstances.ToString(), IniHelper.inipath);
        }
    }
}
