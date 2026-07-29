using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class ZaettaButton : Button
    {
        public Color Fill { get; set; }
        public Color HoverFill { get; set; }
        public Color TextFill { get; set; }
        public int Radius { get; set; }
        public bool OutlineOnly { get; set; }
        public string IconKey { get; set; }
        private bool hovering;

        public ZaettaButton(string text, bool primary)
        {
            Text = text;
            Fill = primary ? Ui.Accent : Color.FromArgb(14, 17, 20);
            HoverFill = primary ? Ui.Accent2 : Color.FromArgb(34, 28, 19);
            TextFill = Color.White;
            Radius = 4;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(3, 8, 13);
            ForeColor = TextFill;
            Font = new Font("Segoe UI", 8, FontStyle.Bold);
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
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(3, 8, 13)))
                e.Graphics.FillRectangle(bg, ClientRectangle);

            RectangleF rect = new RectangleF(0.5f, 0.5f, Math.Max(1f, Width - 1f), Math.Max(1f, Height - 1f));
            Color fillColor = hovering ? HoverFill : Fill;
            using (SolidBrush brush = new SolidBrush(hovering ? HoverFill : Fill))
            {
                using (GraphicsPath path = Rounded(rect, Radius))
                {
                    if (!OutlineOnly)
                        e.Graphics.FillPath(brush, path);
                    else
                        using (Pen pen = new Pen(fillColor, 1))
                            e.Graphics.DrawPath(pen, path);

                    using (Pen edge = new Pen(hovering ? Color.FromArgb(120, 255, 219, 91) : Color.FromArgb(72, 214, 151, 31), 1))
                        e.Graphics.DrawPath(edge, path);
                }
            }

            if (!string.IsNullOrEmpty(IconKey))
            {
                DrawIcon(e.Graphics, IconKey, ClientRectangle, TextFill);
                return;
            }

            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                TextFill,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );
        }

        private static GraphicsPath Rounded(RectangleF rect, int radius)
        {
            float d = Math.Max(2f, Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height)));
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
            g.PixelOffsetMode = PixelOffsetMode.Half;
            float cx = bounds.Left + bounds.Width / 2f;
            float cy = bounds.Top + bounds.Height / 2f;
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
                    g.FillPolygon(brush, new[] { new PointF(cx, cy - 10), new PointF(cx - 3, cy - 6), new PointF(cx + 3, cy - 6) });
                    g.FillPolygon(brush, new[] { new PointF(cx, cy + 10), new PointF(cx - 3, cy + 6), new PointF(cx + 3, cy + 6) });
                    g.FillPolygon(brush, new[] { new PointF(cx - 10, cy), new PointF(cx - 6, cy - 3), new PointF(cx - 6, cy + 3) });
                    g.FillPolygon(brush, new[] { new PointF(cx + 10, cy), new PointF(cx + 6, cy - 3), new PointF(cx + 6, cy + 3) });
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
