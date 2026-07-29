using System;
using System.Drawing;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private static bool CanResizeStroke(DrawOp op)
        {
            if (op == null)
                return false;
            return op.Tool == Tool.Arrow
                || op.Tool == Tool.Rect
                || op.Tool == Tool.Line
                || op.Tool == Tool.Pencil
                || op.Tool == Tool.Highlight;
        }

        private static void AdjustOpWidth(DrawOp op, int delta)
        {
            if (op == null)
                return;
            op.Width = Math.Max(2, Math.Min(12, op.Width + delta));
        }

        private void AdjustOpByWheel(DrawOp op, int delta)
        {
            if (op == null)
                return;

            if (op.Tool == Tool.Text)
            {
                op.Width = Math.Max(10, Math.Min(54, op.Width + (delta * 2)));
                return;
            }

            if (op.Tool == Tool.Number)
            {
                op.Width = Math.Max(18, Math.Min(90, op.Width + (delta * 4)));
                return;
            }

            if (op.Tool == Tool.Pixelate)
            {
                op.Width = Pixelation.ClampIntensity(op.Width + (delta * 2));
                pixelIntensity = op.Width;
                return;
            }

            if (op.Tool == Tool.Rect)
            {
                ScaleBoxOp(op, delta > 0 ? 1.08f : 0.92f);
                return;
            }

            if (op.Tool == Tool.Arrow || op.Tool == Tool.Line)
                ScaleTwoPointOp(op, delta > 0 ? 1.08f : 0.92f);
            else if (op.Tool == Tool.Pencil || op.Tool == Tool.Highlight)
                ScalePointListOp(op, delta > 0 ? 1.08f : 0.92f);
        }

        private void AdjustOpStrokeByWheel(DrawOp op, int delta)
        {
            if (!CanResizeStroke(op))
                return;

            AdjustOpWidth(op, delta);
            drawWidth = Math.Max(2, Math.Min(12, op.Width));
        }

        private void ScaleBoxOp(DrawOp op, float factor)
        {
            Rectangle box = Normalize(op.A, op.B);
            if (box.Width <= 0 || box.Height <= 0)
                return;

            float cx = box.Left + box.Width / 2f;
            float cy = box.Top + box.Height / 2f;
            int newWidth = Math.Max(10, (int)Math.Round(box.Width * factor));
            int newHeight = Math.Max(10, (int)Math.Round(box.Height * factor));
            int left = (int)Math.Round(cx - newWidth / 2f);
            int top = (int)Math.Round(cy - newHeight / 2f);
            int right = left + newWidth;
            int bottom = top + newHeight;

            if (left < selection.Left)
            {
                right += selection.Left - left;
                left = selection.Left;
            }
            if (top < selection.Top)
            {
                bottom += selection.Top - top;
                top = selection.Top;
            }
            if (right > selection.Right)
            {
                left -= right - selection.Right;
                right = selection.Right;
            }
            if (bottom > selection.Bottom)
            {
                top -= bottom - selection.Bottom;
                bottom = selection.Bottom;
            }

            left = Math.Max(selection.Left, left);
            top = Math.Max(selection.Top, top);
            right = Math.Min(selection.Right, Math.Max(left + 10, right));
            bottom = Math.Min(selection.Bottom, Math.Max(top + 10, bottom));

            op.A = new Point(left, top);
            op.B = new Point(right, bottom);
        }

        private void ScaleTwoPointOp(DrawOp op, float factor)
        {
            float cx = (op.A.X + op.B.X) / 2f;
            float cy = (op.A.Y + op.B.Y) / 2f;
            op.A = ClampToSelection(ScalePoint(op.A, cx, cy, factor));
            op.B = ClampToSelection(ScalePoint(op.B, cx, cy, factor));
        }

        private void ScalePointListOp(DrawOp op, float factor)
        {
            if (op.Points == null || op.Points.Count == 0)
                return;

            Rectangle bounds = GetOpBounds(op);
            float cx = bounds.Left + bounds.Width / 2f;
            float cy = bounds.Top + bounds.Height / 2f;
            for (int i = 0; i < op.Points.Count; i++)
                op.Points[i] = ClampToSelection(ScalePoint(op.Points[i], cx, cy, factor));
            op.A = op.Points[0];
            op.B = op.Points[op.Points.Count - 1];
        }

        private static Point ScalePoint(Point point, float cx, float cy, float factor)
        {
            return new Point(
                (int)Math.Round(cx + ((point.X - cx) * factor)),
                (int)Math.Round(cy + ((point.Y - cy) * factor))
            );
        }
    }
}
