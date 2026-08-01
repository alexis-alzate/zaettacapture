using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImageUnscaled(dimmedScreenshot, Point.Empty);

            Rectangle box = HasSelection() ? selection : Normalize(start, current);
            if ((selecting || HasSelection()) && box.Width > 0 && box.Height > 0)
            {
                g.DrawImage(screenshot, box, box, GraphicsUnit.Pixel);
                if (!selecting)
                {
                    foreach (DrawOp op in ops)
                        DrawOpOnOverlay(g, op);
                }
                if (drawing && tool != Tool.Pencil && tool != Tool.Highlight)
                    DrawOpOnOverlay(g, new DrawOp { Tool = tool, A = ClampToSelection(drawStart), B = ClampToSelection(current), Color = color, Width = tool == Tool.Pixelate ? pixelIntensity : drawWidth });
                DrawActiveTextEdit(g);
                if (!selecting)
                    DrawSelectedOpHandles(g);
                DrawSelectionBorder(g, box);
                DrawHandles(g, box);
                DrawSizeLabel(g, box);
            }
        }

        private void DrawOpOnOverlay(Graphics g, DrawOp op)
        {
            Color opColor = op.Tool == Tool.Highlight ? Color.FromArgb(170, Color.Yellow) : op.Color;
            int width = op.Tool == Tool.Highlight ? Math.Max(10, op.Width * 4) : op.Width;
            using (Pen pen = new Pen(opColor, width))
            {
                DrawingStyle.ConfigureLineCap(pen, op.Tool);
                if (op.Tool == Tool.Arrow)
                    g.DrawLine(pen, op.A, op.B);
                else if (op.Tool == Tool.Line)
                    g.DrawLine(pen, op.A, op.B);
                else if (op.Tool == Tool.Rect)
                    g.DrawRectangle(pen, Normalize(op.A, op.B));
                else if (op.Tool == Tool.Pencil || op.Tool == Tool.Highlight)
                {
                    if (op.Points != null && op.Points.Count > 1)
                        g.DrawLines(pen, op.Points.ToArray());
                }
                else if (op.Tool == Tool.Text)
                {
                    using (Font font = new Font("Segoe UI", Math.Max(10, op.Width), FontStyle.Bold))
                    using (SolidBrush brush = new SolidBrush(op.Color))
                        g.DrawString(op.Text ?? "", font, brush, op.A);
                }
                else if (op.Tool == Tool.Number)
                {
                    int size = Math.Max(24, op.Width);
                    Rectangle circle = new Rectangle(op.A.X - size / 2, op.A.Y - size / 2, size, size);
                    using (SolidBrush fill = new SolidBrush(op.Color))
                    using (Font font = new Font("Segoe UI", Math.Max(10, size / 2), FontStyle.Bold))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.FillEllipse(fill, circle);
                        g.DrawString(op.Text ?? "", font, Brushes.White, circle, format);
                    }
                }
                else if (op.Tool == Tool.Pixelate)
                    DrawPixelated(g, Normalize(op.A, op.B), op.Width);
            }
        }

        private void DrawActiveTextEdit(Graphics g)
        {
            if (!textEditing)
                return;

            string preview = activeTextValue ?? "";
            using (Font font = new Font("Segoe UI", activeTextSize, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(color))
            using (Pen caret = new Pen(Color.FromArgb(205, 245, 245, 245), 1))
            using (Pen shadow = new Pen(Color.FromArgb(90, 0, 0, 0), 2))
            using (Pen light = new Pen(Color.FromArgb(220, 245, 245, 245), 1))
            using (Pen dash = new Pen(Color.FromArgb(155, 35, 35, 35), 1))
            {
                Rectangle border = new Rectangle(activeTextBounds.Left, activeTextBounds.Top, Math.Max(1, activeTextBounds.Width - 1), Math.Max(1, activeTextBounds.Height - 1));
                shadow.Alignment = PenAlignment.Inset;
                light.Alignment = PenAlignment.Inset;
                dash.Alignment = PenAlignment.Inset;
                dash.DashPattern = new float[] { 2, 2 };

                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.DrawRectangle(shadow, border);
                g.DrawRectangle(light, border);
                g.DrawRectangle(dash, border);

                GraphicsState state = g.Save();
                g.SetClip(activeTextBounds);
                g.DrawString(preview, font, brush, activeTextPoint);
                SizeF size = g.MeasureString(preview.Length == 0 ? " " : preview, font);
                if (activeTextCaretVisible)
                {
                    float caretX = activeTextPoint.X + (preview.Length == 0 ? 1 : size.Width - 4);
                    caretX = Math.Min(activeTextBounds.Right - ActiveTextPadding, Math.Max(activeTextBounds.Left + ActiveTextPadding, caretX));
                    float top = Math.Max(activeTextBounds.Top + 3, activeTextPoint.Y + 3);
                    float bottom = Math.Min(activeTextBounds.Bottom - 3, activeTextPoint.Y + Math.Max(20, size.Height - 3));
                    g.DrawLine(caret, caretX, top, caretX, bottom);
                }
                g.Restore(state);
            }
        }

        private void DrawPixelated(Graphics g, Rectangle rect, int intensity)
        {
            rect.Intersect(selection);
            Pixelation.Draw(g, screenshot, rect, rect, intensity);
        }

        private Bitmap RenderCrop()
        {
            Bitmap crop = new Bitmap(selection.Width, selection.Height);
            using (Graphics g = Graphics.FromImage(crop))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(screenshot, new Rectangle(0, 0, crop.Width, crop.Height), selection, GraphicsUnit.Pixel);
                g.TranslateTransform(-selection.Left, -selection.Top);
                foreach (DrawOp op in ops)
                    DrawOpOnOverlay(g, op);
                g.ResetTransform();
            }
            return crop;
        }
        private void DrawSelectedOpHandles(Graphics g)
        {
            if (selectedOp == null)
                return;

            Rectangle bounds = GetOpBounds(selectedOp);
            bounds.Inflate(6, 6);
            using (Pen outline = new Pen(Color.FromArgb(190, 255, 255, 255), 1))
            {
                outline.DashPattern = new float[] { 3, 3 };
                g.DrawRectangle(outline, bounds);
            }

            if (!CanResizeOp(selectedOp))
                return;

            Rectangle[] handles = GetResizeHandleRects(selectedOp);
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(248, 248, 248)))
            using (Pen border = new Pen(Color.FromArgb(50, 50, 50), 1))
            {
                foreach (Rectangle handle in handles)
                {
                    g.FillRectangle(fill, handle);
                    g.DrawRectangle(border, handle);
                }
            }
        }

        private void DrawSizeLabel(Graphics g, Rectangle box)
        {
            string label = box.Width + " x " + box.Height;
            using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (SolidBrush bg = new SolidBrush(SelectionLabelBg))
            using (SolidBrush fg = new SolidBrush(Color.White))
            {
                SizeF size = g.MeasureString(label, font);
                RectangleF labelRect = new RectangleF(box.Left, Math.Max(0, box.Top - 27), size.Width + 18, 22);
                g.FillRectangle(bg, labelRect);
                g.DrawString(label, font, fg, labelRect.Left + 8, labelRect.Top + 3);
            }
        }

        private void DrawSelectionBorder(Graphics g, Rectangle box)
        {
            Rectangle inset = new Rectangle(box.Left, box.Top, Math.Max(1, box.Width - 1), Math.Max(1, box.Height - 1));
            using (Pen shadow = new Pen(SelectionShadow, 2))
            using (Pen light = new Pen(SelectionStroke, 1))
            using (Pen dash = new Pen(SelectionDash, 1))
            {
                shadow.Alignment = PenAlignment.Inset;
                light.Alignment = PenAlignment.Inset;
                dash.Alignment = PenAlignment.Inset;
                dash.DashPattern = new float[] { 2, 2 };
                g.DrawRectangle(shadow, inset);
                g.DrawRectangle(light, inset);
                g.DrawRectangle(dash, inset);
            }
        }

        private void DrawHandles(Graphics g, Rectangle box)
        {
            Point[] handles = new[]
            {
                new Point(box.Left, box.Top),
                new Point(box.Left + box.Width / 2, box.Top),
                new Point(box.Right, box.Top),
                new Point(box.Left, box.Top + box.Height / 2),
                new Point(box.Right, box.Top + box.Height / 2),
                new Point(box.Left, box.Bottom),
                new Point(box.Left + box.Width / 2, box.Bottom),
                new Point(box.Right, box.Bottom),
            };
            using (SolidBrush fill = new SolidBrush(SelectionHandle))
            using (Pen border = new Pen(SelectionHandleBorder, 2))
            {
                foreach (Point handle in handles)
                {
                    Rectangle dot = new Rectangle(handle.X - 3, handle.Y - 3, 7, 7);
                    g.FillRectangle(fill, dot);
                    g.DrawRectangle(border, dot);
                }
            }
        }
    }
}
