using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZaettaCaptureNative
{
    internal static class Pixelation
    {
        public const int MinIntensity = 4;
        public const int MaxIntensity = 70;
        public const int DefaultIntensity = 12;

        public static int ClampIntensity(int value)
        {
            return Math.Max(MinIntensity, Math.Min(MaxIntensity, value));
        }

        public static int ToPercent(int intensity)
        {
            int clamped = ClampIntensity(intensity);
            float range = MaxIntensity - MinIntensity;
            if (range <= 0)
                return 100;

            return (int)Math.Round(((clamped - MinIntensity) / range) * 100f);
        }

        public static void Draw(Graphics g, Bitmap source, Rectangle sourceRect, Rectangle destRect, int intensity)
        {
            sourceRect.Intersect(new Rectangle(0, 0, source.Width, source.Height));
            if (sourceRect.Width < 4 || sourceRect.Height < 4 || destRect.Width < 4 || destRect.Height < 4)
                return;

            int blockSize = ClampIntensity(intensity);
            using (Bitmap crop = source.Clone(sourceRect, source.PixelFormat))
            using (Bitmap small = new Bitmap(crop, Math.Max(1, crop.Width / blockSize), Math.Max(1, crop.Height / blockSize)))
            using (Bitmap big = new Bitmap(small, destRect.Width, destRect.Height))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(big, destRect);
            }
        }
    }
}
