using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class ColorSwatchButton : Button
    {
        public Color Swatch { get; set; }
        private bool hovering;

        public ColorSwatchButton()
        {
            Swatch = Color.FromArgb(255, 59, 48);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(3, 8, 13);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush baseBg = new SolidBrush(Color.FromArgb(3, 8, 13)))
                e.Graphics.FillRectangle(baseBg, ClientRectangle);
            Rectangle dot = new Rectangle(Width / 2 - 8, Height / 2 - 8, 16, 16);
            using (SolidBrush brush = new SolidBrush(Swatch))
            using (Pen ring = new Pen(hovering ? Color.White : Color.FromArgb(190, 255, 255, 255), 1))
            {
                e.Graphics.FillEllipse(brush, dot);
                e.Graphics.DrawEllipse(ring, dot);
            }
        }

        private static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            int d = Math.Max(2, Math.Min(radius * 2, Math.Min(rect.Width, rect.Height)));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawIcon(Graphics g, string key, Rectangle bounds, Color color)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int cx = bounds.Left + bounds.Width / 2;
            int cy = bounds.Top + bounds.Height / 2;
            RectangleF r = new RectangleF(cx - 6.5f, cy - 6.5f, 13f, 13f);
            using (Pen pen = new Pen(color, 1.8f))
            using (SolidBrush brush = new SolidBrush(color))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                if (key == "move")
                {
                    g.DrawLine(pen, cx, cy - 7, cx, cy + 7);
                    g.DrawLine(pen, cx - 7, cy, cx + 7, cy);
                    g.FillPolygon(brush, new[] { new Point(cx, cy - 10), new Point(cx - 3, cy - 6), new Point(cx + 3, cy - 6) });
                    g.FillPolygon(brush, new[] { new Point(cx, cy + 10), new Point(cx - 3, cy + 6), new Point(cx + 3, cy + 6) });
                    g.FillPolygon(brush, new[] { new Point(cx - 10, cy), new Point(cx - 6, cy - 3), new Point(cx - 6, cy + 3) });
                    g.FillPolygon(brush, new[] { new Point(cx + 10, cy), new Point(cx + 6, cy - 3), new Point(cx + 6, cy + 3) });
                }
                else if (key == "arrow")
                {
                    using (AdjustableArrowCap cap = new AdjustableArrowCap(4.8f, 6.2f, true))
                    {
                        pen.CustomEndCap = cap;
                        g.DrawLine(pen, cx - 7, cy + 5, cx + 7, cy - 5);
                    }
                }
                else if (key == "rect")
                {
                    g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                }
                else if (key == "text")
                {
                    using (Font font = new Font("Segoe UI", 11, FontStyle.Bold))
                        TextRenderer.DrawText(g, "T", font, bounds, color, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
                else if (key == "pixel")
                {
                    int s = 4;
                    g.FillRectangle(brush, cx - 7, cy - 6, s, s);
                    g.FillRectangle(brush, cx - 1, cy - 6, s, s);
                    g.FillRectangle(brush, cx + 5, cy - 6, s, s);
                    g.FillRectangle(brush, cx - 7, cy, s, s);
                    g.FillRectangle(brush, cx - 1, cy, s, s);
                    g.FillRectangle(brush, cx + 5, cy, s, s);
                    g.FillRectangle(brush, cx - 7, cy + 6, s, s);
                    g.FillRectangle(brush, cx - 1, cy + 6, s, s);
                    g.FillRectangle(brush, cx + 5, cy + 6, s, s);
                }
                else if (key == "more")
                {
                    g.FillEllipse(brush, cx - 7, cy - 2, 4, 4);
                    g.FillEllipse(brush, cx - 2, cy - 2, 4, 4);
                    g.FillEllipse(brush, cx + 3, cy - 2, 4, 4);
                }
            }
        }
    }
}
