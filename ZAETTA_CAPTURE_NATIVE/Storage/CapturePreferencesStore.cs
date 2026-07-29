using System;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class CapturePreferencesStore
    {
        private static readonly string FilePath = Path.Combine(Paths.BaseDir, "capture-preferences.txt");

        public static bool LoadKeepLastSelectionPosition()
        {
            try
            {
                if (!File.Exists(FilePath))
                    return true;

                string value = File.ReadAllText(FilePath).Trim();
                if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                    return false;
                return true;
            }
            catch
            {
                return true;
            }
        }

        public static void SaveKeepLastSelectionPosition(bool enabled)
        {
            Directory.CreateDirectory(Paths.BaseDir);
            File.WriteAllText(FilePath, enabled ? "1" : "0");
        }
    }
}
