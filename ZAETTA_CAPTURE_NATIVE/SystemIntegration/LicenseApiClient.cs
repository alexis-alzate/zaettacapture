using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ZaettaCaptureNative
{
    internal sealed class TrialStartResult
    {
        public DateTime StartedAtUtc;
        public DateTime ExpiresAtUtc;
    }

    internal static class LicenseApiClient
    {
        private const string BaseUrl = "https://ocnoiraaqosfmbluccba.supabase.co/functions/v1";
        private static readonly HttpClient Http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(AppInfo.Name.Replace(" ", "-") + "/" + AppInfo.Version);
            return client;
        }

        public static TrialStartResult StartTrial(string deviceFingerprint, string fingerprintSource, string email)
        {
            var payload = new
            {
                deviceFingerprint = deviceFingerprint,
                fingerprintSource = fingerprintSource,
                email = email,
            };

            string requestJson = JsonSerializer.Serialize(payload);

            using (StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json"))
            using (HttpResponseMessage response = Http.PostAsync(BaseUrl + "/trial-start", content).GetAwaiter().GetResult())
            {
                string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = TryExtractError(body) ?? ("HTTP " + (int)response.StatusCode);
                    throw new InvalidOperationException(errorMessage);
                }

                using (JsonDocument document = JsonDocument.Parse(body))
                {
                    JsonElement root = document.RootElement;
                    return new TrialStartResult
                    {
                        StartedAtUtc = root.GetProperty("startedAt").GetDateTime(),
                        ExpiresAtUtc = root.GetProperty("expiresAt").GetDateTime(),
                    };
                }
            }
        }

        private static string TryExtractError(string body)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(body))
                {
                    if (document.RootElement.TryGetProperty("error", out JsonElement error))
                        return error.GetString();
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
