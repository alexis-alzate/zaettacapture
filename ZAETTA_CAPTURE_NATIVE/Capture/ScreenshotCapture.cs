using System.Drawing;

namespace ZaettaCaptureNative
{
    internal sealed class ScreenshotCapture
    {
        public Rectangle Bounds { get; private set; }
        public Bitmap Image { get; private set; }

        public ScreenshotCapture(Rectangle bounds, Bitmap image)
        {
            Bounds = bounds;
            Image = image;
        }
    }
}
