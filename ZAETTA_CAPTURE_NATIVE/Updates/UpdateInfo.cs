using System;

namespace ZaettaCaptureNative
{
    internal sealed class UpdateInfo
    {
        public string Product { get; set; }
        public string Version { get; set; }
        public string ReleasedAt { get; set; }
        public string DownloadUrl { get; set; }
        public string Sha256 { get; set; }
        public long FileSizeBytes { get; set; }
        public string[] Notes { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Version)
                    && !string.IsNullOrEmpty(DownloadUrl);
            }
        }
    }
}
