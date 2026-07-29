using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class ScreenshotService
    {
        public static ScreenshotCapture CaptureVirtualScreen()
        {
            Rectangle bounds = SystemInformation.VirtualScreen;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            return new ScreenshotCapture(bounds, screenshot);
        }
    }
}
