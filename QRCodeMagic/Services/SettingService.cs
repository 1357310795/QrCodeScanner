using QRCodeMagic.Helpers;
using System;
using System.Windows.Input;

namespace QRCodeMagic.Services
{
    public enum SettingKey
    {
        Engine,
        HotKey,
        HotModifierKey,
        AutoRun,
        CaptureMode,
        FastMode,
        Theme,
        Language,
        HideToTray,
        IgnoreDup,
        AllowMultipleInstances,
    }
    public static class SettingService
    {
        public static T Get<T>(SettingKey key, T defaultValue)
        {
            var str = IniHelper.GetKeyValue("main", key.ToString(), defaultValue.ToString());
            object value = null;
            if (typeof(T).IsAssignableFrom(typeof(int)))
            {
                value = int.Parse(str);
            }
            if (typeof(T).IsAssignableFrom(typeof(double)))
            {
                value = double.Parse(str);
            }
            if (typeof(T).IsAssignableFrom(typeof(float)))
            {
                value = float.Parse(str);
            }
            if (typeof(T).IsAssignableFrom(typeof(string)))
            {
                value = str;
            }
            if (typeof(T).IsAssignableFrom(typeof(bool)))
            {
                value = bool.Parse(str);
            }
            if (typeof(T).IsEnum)
            {
                value = Enum.Parse(typeof(T), str);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static bool Set(SettingKey key, object value)
        {
            return IniHelper.SetKeyValue("main", key.ToString(), value.ToString());
        }
    }
}
