using System;
using System.IO;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class CapturePreferencesStore
    {
        private static readonly string FilePath = Path.Combine(Paths.BaseDir, "capture-preferences.txt");
        private const string KeepLastSelectionPositionKey = "keepLastSelectionPosition";
        private const string OpenLockedKey = "openLocked";
        private const string HotkeyKeyKey = "hotkeyKey";
        private const string HotkeyModifiersKey = "hotkeyModifiers";

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

        public static HotkeyPreference LoadHotkey()
        {
            return new HotkeyPreference
            {
                Key = LoadKey(HotkeyKeyKey, Keys.PrintScreen),
                Modifiers = LoadUInt(HotkeyModifiersKey, 0)
            };
        }

        public static void SaveHotkey(Keys key, uint modifiers)
        {
            bool keepLastSelectionPosition = LoadKeepLastSelectionPosition();
            bool openLocked = LoadOpenLocked();
            WritePreferences(keepLastSelectionPosition, openLocked, key, modifiers);
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
            HotkeyPreference hotkey = LoadHotkey();

            WritePreferences(keepLastSelectionPosition, openLocked, hotkey.Key, hotkey.Modifiers);
        }

        private static void WritePreferences(bool keepLastSelectionPosition, bool openLocked, Keys hotkeyKey, uint hotkeyModifiers)
        {
            Directory.CreateDirectory(Paths.BaseDir);
            File.WriteAllText(
                FilePath,
                KeepLastSelectionPositionKey + "=" + (keepLastSelectionPosition ? "1" : "0") + Environment.NewLine +
                OpenLockedKey + "=" + (openLocked ? "1" : "0") + Environment.NewLine +
                HotkeyKeyKey + "=" + hotkeyKey + Environment.NewLine +
                HotkeyModifiersKey + "=" + hotkeyModifiers + Environment.NewLine
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

        private static Keys LoadKey(string key, Keys defaultValue)
        {
            string value = LoadString(key, string.Empty);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            try
            {
                return (Keys)Enum.Parse(typeof(Keys), value, true);
            }
            catch
            {
                return defaultValue;
            }
        }

        private static uint LoadUInt(string key, uint defaultValue)
        {
            string value = LoadString(key, string.Empty);
            uint result;
            return uint.TryParse(value, out result) ? result : defaultValue;
        }

        private static string LoadString(string key, string defaultValue)
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
                    if (string.Equals(name, key, StringComparison.OrdinalIgnoreCase))
                        return line.Substring(separator + 1).Trim();
                }
            }
            catch
            {
            }

            return defaultValue;
        }
    }
}
