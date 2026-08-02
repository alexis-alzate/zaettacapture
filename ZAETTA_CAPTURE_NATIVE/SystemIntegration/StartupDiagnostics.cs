using System;
using System.IO;

namespace ZaettaCaptureNative
{
    internal static class StartupDiagnostics
    {
        public static void Log(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(Paths.BaseDir);
                File.AppendAllText(
                    Path.Combine(Paths.BaseDir, "startup-error.log"),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + ex + Environment.NewLine + Environment.NewLine
                );
            }
            catch
            {
            }
        }
    }
}
