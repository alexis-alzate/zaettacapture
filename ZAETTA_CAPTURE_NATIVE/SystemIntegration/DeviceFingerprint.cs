using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace ZaettaCaptureNative
{
    internal static class DeviceFingerprint
    {
        public const string SourceHardware = "hardware";
        public const string SourceMachineGuidFallback = "machine_guid_fallback";

        private static readonly string[] PlaceholderValues =
        {
            "",
            "TO BE FILLED BY O.E.M.",
            "DEFAULT STRING",
            "NONE",
            "NOT SPECIFIED",
            "SYSTEM SERIAL NUMBER",
            "N/A",
            "0",
            "00000000",
        };

        public static (string Fingerprint, string Source) Compute()
        {
            string biosSerial = ReadWmiProperty("Win32_BIOS", "SerialNumber", null);
            string boardSerial = ReadWmiProperty("Win32_BaseBoard", "SerialNumber", null);
            string diskSerial = ReadWmiProperty("Win32_DiskDrive", "SerialNumber", "Index = 0");

            string[] hardwareParts = new[] { biosSerial, boardSerial, diskSerial }
                .Where(IsUsableIdentifier)
                .ToArray();

            if (hardwareParts.Length > 0)
            {
                return (Hash(string.Join("|", hardwareParts)), SourceHardware);
            }

            string machineGuid = ReadMachineGuid();
            if (IsUsableIdentifier(machineGuid))
            {
                return (Hash(machineGuid), SourceMachineGuidFallback);
            }

            // Ultimo recurso si ni el hardware ni el registro respondieron.
            return (Hash(Environment.MachineName + "|" + Environment.UserName), SourceMachineGuidFallback);
        }

        private static string ReadWmiProperty(string wmiClass, string property, string condition)
        {
            try
            {
                string query = condition == null
                    ? $"SELECT {property} FROM {wmiClass}"
                    : $"SELECT {property} FROM {wmiClass} WHERE {condition}";

                using (var searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementBaseObject item in results)
                    {
                        using (item)
                        {
                            return item[property]?.ToString()?.Trim();
                        }
                    }
                }
            }
            catch
            {
                // WMI puede no estar disponible en maquinas virtuales o entornos restringidos.
            }

            return null;
        }

        private static string ReadMachineGuid()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return key?.GetValue("MachineGuid") as string;
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool IsUsableIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            string normalized = value.Trim().ToUpperInvariant();
            return !PlaceholderValues.Contains(normalized) && normalized.Any(char.IsLetterOrDigit);
        }

        private static string Hash(string value)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }
}
