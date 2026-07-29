using System;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class Paths
    {
        public static readonly string BaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            AppInfo.Name
        );
        public static readonly string HistoryDir = Path.Combine(BaseDir, "Historial");
    }
}
