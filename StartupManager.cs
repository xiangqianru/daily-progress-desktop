using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DailyProgressDesk
{
    public static class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "DailyProgressDesk";

        public static bool SetEnabled(bool enabled, out string error)
        {
            error = "";
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
                {
                    if (key == null) throw new InvalidOperationException("无法打开当前用户的启动项设置。");
                    if (enabled)
                        key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\"");
                    else
                        key.DeleteValue(ValueName, false);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static void RefreshEnabledPath(bool enabled)
        {
            if (!enabled) return;
            string ignored;
            SetEnabled(true, out ignored);
        }
    }
}
