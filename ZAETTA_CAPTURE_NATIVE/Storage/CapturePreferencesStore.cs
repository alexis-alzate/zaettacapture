using System;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class CapturePreferencesStore
    {
        private static readonly string FilePath = Path.Combine(Paths.BaseDir, "capture-preferences.txt");
        private const string KeepLastSelectionPositionKey = "keepLastSelectionPosition";
        private const string OpenLockedKey = "openLocked";

        public static bool LoadKeepLastSelectionPosition()
        {
            return LoadBoolean(KeepLastSelectionPositionKey, true);
        }

        public static void SaveKeepLastSelectionPosition(bool enabled)
        {
            SaveBoolean(KeepLastSelectionPositionKey, enabled);
        }

        public static bool LoadOpenLocked()
        {
            return LoadBoolean(OpenLockedKey, false);
        }

        public static void SaveOpenLocked(bool enabled)
        {
            SaveBoolean(OpenLockedKey, enabled);
        }

        private static bool LoadBoolean(string key, bool defaultValue)
        {
            try
            {
                if (!File.Exists(FilePath))
                    return defaultValue;

                string[] lines = File.ReadAllLines(FilePath);
                foreach (string line in lines)
                {
                    int separator = line.IndexOf('=');
                    if (separator <= 0)
                        continue;

                    string name = line.Substring(0, separator).Trim();
                    if (!string.Equals(name, key, StringComparison.OrdinalIgnoreCase))
                        continue;

                    return ParseBoolean(line.Substring(separator + 1).Trim(), defaultValue);
                }

                if (key == KeepLastSelectionPositionKey && lines.Length == 1)
                    return ParseBoolean(lines[0].Trim(), defaultValue);

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private static void SaveBoolean(string key, bool enabled)
        {
            bool keepLastSelectionPosition = key == KeepLastSelectionPositionKey ? enabled : LoadKeepLastSelectionPosition();
            bool openLocked = key == OpenLockedKey ? enabled : LoadOpenLocked();

            Directory.CreateDirectory(Paths.BaseDir);
            File.WriteAllText(
                FilePath,
                KeepLastSelectionPositionKey + "=" + (keepLastSelectionPosition ? "1" : "0") + Environment.NewLine +
                OpenLockedKey + "=" + (openLocked ? "1" : "0") + Environment.NewLine
            );
        }

        private static bool ParseBoolean(string value, bool defaultValue)
        {
            if (string.Equals(value, "0", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            return defaultValue;
        }
    }
}
