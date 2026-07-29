using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZaettaCaptureNative
{
    internal static class DrawingStyle
    {
        public static void ConfigureLineCap(Pen pen, Tool tool)
        {
            pen.StartCap = LineCap.Round;
            if (tool == Tool.Arrow)
                pen.CustomEndCap = new AdjustableArrowCap(5.8f, 7.2f, true);
            else
                pen.EndCap = LineCap.Round;
        }
    }
}
