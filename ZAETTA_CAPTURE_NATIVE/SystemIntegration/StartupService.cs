using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ZaettaCaptureNative
{
    internal static class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
                {
                    if (key == null)
                        return false;

                    string value = key.GetValue(AppInfo.Name) as string;
                    if (string.IsNullOrWhiteSpace(value))
                        return false;

                    return value.IndexOf(Application.ExecutablePath, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (key == null)
                    throw new InvalidOperationException("No se pudo abrir el inicio de Windows.");

                if (enabled)
                    key.SetValue(AppInfo.Name, "\"" + Application.ExecutablePath + "\"");
                else
                    key.DeleteValue(AppInfo.Name, false);
            }
        }
    }
}
