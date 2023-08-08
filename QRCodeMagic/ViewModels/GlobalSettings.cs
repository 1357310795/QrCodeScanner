using QRCodeMagic.Helpers;
using QRCodeMagic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QRCodeMagic.ViewModels
{
    public class GlobalSettings
    {
        public static int selectedengine;
        public static Key selectedKey;
        public static ModifierKeys selectedKeyType;
        public static bool isAutoRun;
        public static bool captureMode;
        public static bool fastMode;
        public static string theme;
        public static string language;
        public static bool hideToTray;
        public static bool ignoreDup;
        public static bool allowMultipleInstances;

        public static void Read()
        {
            selectedengine = SettingService.Get<int>(SettingKey.Engine, 0);
            selectedKey = SettingService.Get<Key>(SettingKey.HotKey, Key.Z);
            selectedKeyType = SettingService.Get<ModifierKeys>(SettingKey.HotModifierKey, ModifierKeys.Alt);

            theme = SettingService.Get<string>(SettingKey.Theme, "system");
            language = SettingService.Get<string>(SettingKey.Language, "zh-cn");
            captureMode = SettingService.Get<bool>(SettingKey.CaptureMode, false);
            fastMode = SettingService.Get<bool>(SettingKey.FastMode, false);
            hideToTray = SettingService.Get<bool>(SettingKey.HideToTray, false);
            ignoreDup = SettingService.Get<bool>(SettingKey.IgnoreDup, false);
            allowMultipleInstances = SettingService.Get<bool>(SettingKey.AllowMultipleInstances, false);
        }

        public static void Save()
        {
             SettingService.Set(SettingKey.Engine, selectedengine);
             SettingService.Set(SettingKey.HotKey, selectedKey);
             SettingService.Set(SettingKey.HotModifierKey, selectedKeyType);

             SettingService.Set(SettingKey.Theme, theme);
             SettingService.Set(SettingKey.Language, language);
             SettingService.Set(SettingKey.CaptureMode, captureMode);
             SettingService.Set(SettingKey.FastMode, fastMode);
             SettingService.Set(SettingKey.HideToTray, hideToTray);
             SettingService.Set(SettingKey.IgnoreDup, ignoreDup);
             SettingService.Set(SettingKey.AllowMultipleInstances, allowMultipleInstances);
        }
    }
}
