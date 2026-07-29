using System.Collections.Generic;
using System.Drawing;

namespace ZaettaCaptureNative
{
    internal sealed class DrawOp
    {
        public Tool Tool;
        public Point A;
        public Point B;
        public string Text;
        public Color Color;
        public int Width;
        public List<Point> Points;
    }
}
