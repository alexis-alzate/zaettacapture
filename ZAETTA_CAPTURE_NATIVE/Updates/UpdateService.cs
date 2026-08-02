using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ZaettaCaptureNative
{
    internal static class UpdateService
    {
        private static readonly string[] ManifestUrls = new[]
        {
            "https://www.zaettasoftware.com/latest.json",
            "https://zaettacapture.vercel.app/latest.json",
            "https://raw.githubusercontent.com/alexis-alzate/zaettacapture/main/website/latest.json"
        };

        public static UpdateInfo CheckForUpdate()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            string json = DownloadManifest();
            UpdateInfo info = ParseManifest(json);

            if (info == null || !info.IsValid)
                return null;

            Version current;
            Version remote;
            if (!Version.TryParse(AppInfo.Version, out current) || !Version.TryParse(info.Version, out remote))
                return null;

            return remote > current ? info : null;
        }

        private static string DownloadManifest()
        {
            Exception lastError = null;

            foreach (string url in ManifestUrls)
            {
                try
                {
                    return DownloadStringFollowingRedirects(url);
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    StartupDiagnostics.Log(new WebException("Fallo consultando manifest de actualizacion: " + url, ex));
                }
            }

            throw new WebException("No se pudo conectar a ningun servidor de actualizaciones.", lastError);
        }

        public static string GetInstallerDownloadPath(UpdateInfo info)
        {
            string version = SanitizeFilePart(info.Version);
            string fileName = "ZaettaCaptureSetup-" + version + ".exe";
            return Path.Combine(Paths.UpdatesDir, fileName);
        }

        public static bool VerifySha256(string filePath, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash))
                return true;

            using (FileStream stream = File.OpenRead(filePath))
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                string actual = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return string.Equals(actual, expectedHash.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
            }
        }

        private static string DownloadStringFollowingRedirects(string url)
        {
            string currentUrl = url;

            for (int attempt = 0; attempt < 5; attempt++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(currentUrl);
                request.Method = "GET";
                request.AllowAutoRedirect = false;
                request.UserAgent = AppInfo.Name + "/" + AppInfo.Version;
                request.Accept = "application/json,text/plain,*/*";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    int status = (int)response.StatusCode;
                    if (status == 301 || status == 302 || status == 303 || status == 307 || status == 308)
                    {
                        string location = response.Headers["Location"];
                        if (string.IsNullOrWhiteSpace(location))
                            throw new WebException("El servidor redirigio sin entregar una URL de destino.");

                        currentUrl = BuildRedirectUrl(currentUrl, location);
                        continue;
                    }

                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        return reader.ReadToEnd();
                }
            }

            throw new WebException("Demasiadas redirecciones al consultar el manifest de actualizacion.");
        }

        private static string BuildRedirectUrl(string baseUrl, string location)
        {
            Uri redirect;
            if (Uri.TryCreate(location, UriKind.Absolute, out redirect))
                return redirect.ToString();

            Uri baseUri = new Uri(baseUrl);
            return new Uri(baseUri, location).ToString();
        }

        private static UpdateInfo ParseManifest(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            UpdateInfo info = new UpdateInfo();
            info.Product = ExtractString(json, "product");
            info.Version = ExtractString(json, "version");
            info.ReleasedAt = ExtractString(json, "releasedAt");
            info.DownloadUrl = ExtractString(json, "downloadUrl");
            info.Sha256 = ExtractString(json, "sha256");
            info.FileSizeBytes = ExtractLong(json, "fileSizeBytes");
            info.Notes = ExtractStringArray(json, "notes");
            return info;
        }

        private static string ExtractString(string json, string propertyName)
        {
            Match match = Regex.Match(
                json,
                "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\"(?<value>(?:\\\\.|[^\"])*)\"",
                RegexOptions.IgnoreCase
            );

            return match.Success ? UnescapeJsonString(match.Groups["value"].Value) : null;
        }

        private static long ExtractLong(string json, string propertyName)
        {
            Match match = Regex.Match(
                json,
                "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*(?<value>\\d+)",
                RegexOptions.IgnoreCase
            );

            long value;
            return match.Success && long.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                ? value
                : 0;
        }

        private static string[] ExtractStringArray(string json, string propertyName)
        {
            Match arrayMatch = Regex.Match(
                json,
                "\"" + Regex.Escape(propertyName) + "\"\\s*:\\s*\\[(?<value>.*?)\\]",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            );

            if (!arrayMatch.Success)
                return new string[0];

            List<string> values = new List<string>();
            MatchCollection itemMatches = Regex.Matches(arrayMatch.Groups["value"].Value, "\"(?<value>(?:\\\\.|[^\"])*)\"");
            foreach (Match item in itemMatches)
                values.Add(UnescapeJsonString(item.Groups["value"].Value));

            return values.ToArray();
        }

        private static string UnescapeJsonString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder output = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c != '\\' || i + 1 >= value.Length)
                {
                    output.Append(c);
                    continue;
                }

                char escaped = value[++i];
                switch (escaped)
                {
                    case '"':
                    case '\\':
                    case '/':
                        output.Append(escaped);
                        break;
                    case 'b':
                        output.Append('\b');
                        break;
                    case 'f':
                        output.Append('\f');
                        break;
                    case 'n':
                        output.Append('\n');
                        break;
                    case 'r':
                        output.Append('\r');
                        break;
                    case 't':
                        output.Append('\t');
                        break;
                    case 'u':
                        if (i + 4 < value.Length)
                        {
                            string hex = value.Substring(i + 1, 4);
                            int code;
                            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                            {
                                output.Append((char)code);
                                i += 4;
                            }
                        }
                        break;
                    default:
                        output.Append(escaped);
                        break;
                }
            }

            return output.ToString();
        }

        private static string SanitizeFilePart(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "latest";

            foreach (char c in Path.GetInvalidFileNameChars())
                value = value.Replace(c, '-');

            return value;
        }
    }
}
