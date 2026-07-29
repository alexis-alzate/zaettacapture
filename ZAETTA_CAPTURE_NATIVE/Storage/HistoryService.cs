using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class HistoryService
    {
        public static string Save(Bitmap image)
        {
            Directory.CreateDirectory(Paths.HistoryDir);
            string path = Path.Combine(Paths.HistoryDir, BuildCaptureFileName());
            image.Save(path, ImageFormat.Png);
            return path;
        }

        public static void Open()
        {
            Directory.CreateDirectory(Paths.HistoryDir);
            Process.Start(Paths.HistoryDir);
        }

        public static string BuildCaptureFileName()
        {
            return "Zaetta_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        }
    }
}
