using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyQrCodeScanner
{
    public class HotKeyHelper
    {
        public static bool RegisterHotKey(HotKeyModel hotKeyModel, IntPtr hWnd)
        {
            var fsModifierKey = new ModifierKeys();

            HotKeyManager.UnregisterHotKey(hWnd, HotKeyManager.COPY_ID);

            // 注册热键
            if (hotKeyModel.SelectType == EType.Alt)
                fsModifierKey = ModifierKeys.Alt;
            else if (hotKeyModel.SelectType == EType.Ctrl)
                fsModifierKey = ModifierKeys.Control;
            else if (hotKeyModel.SelectType == EType.Shift)
                fsModifierKey = ModifierKeys.Shift;

            var result = HotKeyManager.RegisterHotKey(hWnd, HotKeyManager.COPY_ID, fsModifierKey, (int)hotKeyModel.SelectKey);
            if (result) 
            {
                //ini
            }
            return result;
        }
    }
}
