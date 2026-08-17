using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class LicenseStore
    {
        private static readonly string FilePath = Path.Combine(Paths.BaseDir, "license.txt");

        private const string EmailKey = "email";
        private const string TrialStartedAtKey = "trialStartedAt";
        private const string TrialExpiresAtKey = "trialExpiresAt";
        private const string LicenseKeyKey = "licenseKey";
        private const string LicenseStatusKey = "licenseStatus";

        public static string LoadEmail()
        {
            Dictionary<string, string> values = LoadAll();
            return values.TryGetValue(EmailKey, out string value) ? value : string.Empty;
        }

        public static DateTime? LoadTrialStartedAt()
        {
            return LoadDate(TrialStartedAtKey);
        }

        public static DateTime? LoadTrialExpiresAt()
        {
            return LoadDate(TrialExpiresAtKey);
        }

        public static string LoadLicenseKey()
        {
            Dictionary<string, string> values = LoadAll();
            return values.TryGetValue(LicenseKeyKey, out string value) ? value : string.Empty;
        }

        public static string LoadLicenseStatus()
        {
            Dictionary<string, string> values = LoadAll();
            return values.TryGetValue(LicenseStatusKey, out string value) ? value : string.Empty;
        }

        public static void SaveTrial(string email, DateTime startedAtUtc, DateTime expiresAtUtc)
        {
            Dictionary<string, string> values = LoadAll();
            values[EmailKey] = email ?? string.Empty;
            values[TrialStartedAtKey] = startedAtUtc.ToString("o", CultureInfo.InvariantCulture);
            values[TrialExpiresAtKey] = expiresAtUtc.ToString("o", CultureInfo.InvariantCulture);
            WriteAll(values);
        }

        public static void SaveLicense(string licenseKey, string status)
        {
            Dictionary<string, string> values = LoadAll();
            values[LicenseKeyKey] = licenseKey ?? string.Empty;
            values[LicenseStatusKey] = status ?? string.Empty;
            WriteAll(values);
        }

        private static DateTime? LoadDate(string key)
        {
            Dictionary<string, string> values = LoadAll();
            if (values.TryGetValue(key, out string value) &&
                DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                return parsed;
            }

            return null;
        }

        private static Dictionary<string, string> LoadAll()
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (!File.Exists(FilePath))
                    return values;

                foreach (string line in File.ReadAllLines(FilePath))
                {
                    int separator = line.IndexOf('=');
                    if (separator <= 0)
                        continue;

                    string name = line.Substring(0, separator).Trim();
                    string value = line.Substring(separator + 1).Trim();
                    values[name] = value;
                }
            }
            catch
            {
            }

            return values;
        }

        private static void WriteAll(Dictionary<string, string> values)
        {
            Directory.CreateDirectory(Paths.BaseDir);

            var lines = new List<string>();
            foreach (KeyValuePair<string, string> pair in values)
                lines.Add(pair.Key + "=" + pair.Value);

            File.WriteAllLines(FilePath, lines);
        }
    }
}
