using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class ClipboardHelper
    {
        public static void SetImageWithRetry(Bitmap image)
        {
            Exception lastError = null;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetImage(image);
                    return;
                }
                catch (ExternalException ex)
                {
                    lastError = ex;
                    Thread.Sleep(80);
                }
            }

            image.Dispose();
            MessageBox.Show(
                "No se pudo copiar la captura al portapapeles. Intente de nuevo en unos segundos.",
                AppInfo.Name,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            if (lastError != null)
                return;
        }
    }
}
