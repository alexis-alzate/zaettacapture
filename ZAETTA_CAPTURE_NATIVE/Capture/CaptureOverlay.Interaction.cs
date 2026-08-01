using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private DrawOp HitTestOp(Point point)
        {
            for (int i = ops.Count - 1; i >= 0; i--)
            {
                DrawOp op = ops[i];
                Rectangle bounds = GetOpBounds(op);
                bounds.Inflate(Math.Max(8, op.Width + 6), Math.Max(8, op.Width + 6));
                if (bounds.Contains(point))
                    return op;
            }
            return null;
        }

        private Rectangle GetOpBounds(DrawOp op)
        {
            using (Graphics g = CreateGraphics())
            {
                if (op.Tool == Tool.Text)
                {
                    if (string.IsNullOrWhiteSpace(op.Text))
                        return new Rectangle(op.A.X, op.A.Y, 1, 1);
                    using (Font font = new Font("Segoe UI", Math.Max(10, op.Width), FontStyle.Bold))
                    {
                        SizeF size = g.MeasureString(op.Text, font);
                        return new Rectangle(op.A.X, op.A.Y, Math.Max(1, (int)Math.Ceiling(size.Width)), Math.Max(1, (int)Math.Ceiling(size.Height)));
                    }
                }
            }

            if ((op.Tool == Tool.Pencil || op.Tool == Tool.Highlight) && op.Points != null && op.Points.Count > 0)
            {
                int left = op.Points[0].X;
                int right = op.Points[0].X;
                int top = op.Points[0].Y;
                int bottom = op.Points[0].Y;
                foreach (Point p in op.Points)
                {
                    left = Math.Min(left, p.X);
                    right = Math.Max(right, p.X);
                    top = Math.Min(top, p.Y);
                    bottom = Math.Max(bottom, p.Y);
                }
                return Rectangle.FromLTRB(left, top, Math.Max(left + 1, right), Math.Max(top + 1, bottom));
            }

            if (op.Tool == Tool.Number)
            {
                int size = Math.Max(24, op.Width);
                return new Rectangle(op.A.X - size / 2, op.A.Y - size / 2, size, size);
            }

            return Normalize(op.A, op.B);
        }

        private void MoveOpTo(DrawOp op, Point requestedTopLeft)
        {
            Rectangle bounds = GetOpBounds(op);
            Point target = ClampBoundsTopLeft(requestedTopLeft, bounds.Size);
            int dx = target.X - bounds.Left;
            int dy = target.Y - bounds.Top;
            OffsetOp(op, dx, dy);
        }

        private void MoveSelectionTo(Point requestedTopLeft)
        {
            Point target = ClampSelectionTopLeft(requestedTopLeft);
            int dx = target.X - selection.Left;
            int dy = target.Y - selection.Top;
            if (dx == 0 && dy == 0)
                return;

            selection = new Rectangle(target, selection.Size);
            foreach (DrawOp op in ops)
                OffsetOp(op, dx, dy);
        }

        private void BeginReselect(Point point)
        {
            CommitTextEdit();
            previousSelectionBeforeReselect = selection;
            selection = Rectangle.Empty;
            selectedOp = null;
            movingOp = null;
            movingSelection = false;
            resizingOp = null;
            resizeHandleIndex = -1;
            resizingSelection = false;
            selectionResizeHandleIndex = -1;
            pendingRightCopy = false;
            selecting = true;
            reselecting = true;
            start = point;
            current = point;
            HideToolbars();
            Capture = true;
            Cursor = Cursors.Cross;
            Invalidate();
        }

        private Point ClampSelectionTopLeft(Point requestedTopLeft)
        {
            int maxX = Math.Max(0, Width - Math.Max(1, selection.Width));
            int maxY = Math.Max(0, Height - Math.Max(1, selection.Height));
            return new Point(
                Math.Max(0, Math.Min(maxX, requestedTopLeft.X)),
                Math.Max(0, Math.Min(maxY, requestedTopLeft.Y))
            );
        }

        private Point ClampBoundsTopLeft(Point requestedTopLeft, Size size)
        {
            int maxX = Math.Max(selection.Left, selection.Right - Math.Max(1, size.Width));
            int maxY = Math.Max(selection.Top, selection.Bottom - Math.Max(1, size.Height));
            return new Point(
                Math.Max(selection.Left, Math.Min(maxX, requestedTopLeft.X)),
                Math.Max(selection.Top, Math.Min(maxY, requestedTopLeft.Y))
            );
        }

        private void OffsetOp(DrawOp op, int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return;
            op.A = new Point(op.A.X + dx, op.A.Y + dy);
            op.B = new Point(op.B.X + dx, op.B.Y + dy);
            if (op.Points != null)
            {
                for (int i = 0; i < op.Points.Count; i++)
                    op.Points[i] = new Point(op.Points[i].X + dx, op.Points[i].Y + dy);
            }
        }

        private bool CanResizeOp(DrawOp op)
        {
            return op != null && (op.Tool == Tool.Arrow || op.Tool == Tool.Line || op.Tool == Tool.Rect || op.Tool == Tool.Pixelate);
        }

        private int HitTestResizeHandle(DrawOp op, Point point)
        {
            if (!CanResizeOp(op))
                return -1;

            Rectangle[] handles = GetResizeHandleRects(op);
            for (int i = handles.Length - 1; i >= 0; i--)
            {
                Rectangle hit = handles[i];
                hit.Inflate(5, 5);
                if (hit.Contains(point))
                    return i;
            }
            return -1;
        }

        private Rectangle[] GetResizeHandleRects(DrawOp op)
        {
            if (op.Tool == Tool.Arrow || op.Tool == Tool.Line)
            {
                return new[]
                {
                    HandleRect(op.A),
                    HandleRect(op.B)
                };
            }

            Rectangle box = Normalize(op.A, op.B);
            Point[] points = new[]
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

            Rectangle[] rects = new Rectangle[points.Length];
            for (int i = 0; i < points.Length; i++)
                rects[i] = HandleRect(points[i]);
            return rects;
        }

        private Rectangle HandleRect(Point center)
        {
            return new Rectangle(center.X - 4, center.Y - 4, 9, 9);
        }

        private void ResizeOp(DrawOp op, int handleIndex, Point point)
        {
            if (!CanResizeOp(op))
                return;

            if (op.Tool == Tool.Arrow || op.Tool == Tool.Line)
            {
                if (handleIndex == 0)
                    op.A = point;
                else
                    op.B = point;
                return;
            }

            Rectangle box = Normalize(op.A, op.B);
            int left = box.Left;
            int right = box.Right;
            int top = box.Top;
            int bottom = box.Bottom;

            if (handleIndex == 0 || handleIndex == 3 || handleIndex == 5)
                left = point.X;
            if (handleIndex == 2 || handleIndex == 4 || handleIndex == 7)
                right = point.X;
            if (handleIndex == 0 || handleIndex == 1 || handleIndex == 2)
                top = point.Y;
            if (handleIndex == 5 || handleIndex == 6 || handleIndex == 7)
                bottom = point.Y;

            left = Math.Max(selection.Left, Math.Min(selection.Right - 1, left));
            right = Math.Max(selection.Left + 1, Math.Min(selection.Right, right));
            top = Math.Max(selection.Top, Math.Min(selection.Bottom - 1, top));
            bottom = Math.Max(selection.Top + 1, Math.Min(selection.Bottom, bottom));

            op.A = new Point(left, top);
            op.B = new Point(right, bottom);
        }

        private Cursor ResizeCursor(int handleIndex)
        {
            if (handleIndex == 0 || handleIndex == 7)
                return Cursors.SizeNWSE;
            if (handleIndex == 2 || handleIndex == 5)
                return Cursors.SizeNESW;
            if (handleIndex == 1 || handleIndex == 6)
                return Cursors.SizeNS;
            if (handleIndex == 3 || handleIndex == 4)
                return Cursors.SizeWE;
            return Cursors.SizeAll;
        }

        private int HitTestSelectionHandle(Point point)
        {
            if (!HasSelection())
                return -1;

            Rectangle[] handles = GetSelectionHandleRects();
            for (int i = handles.Length - 1; i >= 0; i--)
            {
                Rectangle hit = handles[i];
                hit.Inflate(7, 7);
                if (hit.Contains(point))
                    return i;
            }
            return -1;
        }

        private Rectangle[] GetSelectionHandleRects()
        {
            Rectangle box = selection;
            Point[] points = new[]
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

            Rectangle[] rects = new Rectangle[points.Length];
            for (int i = 0; i < points.Length; i++)
                rects[i] = HandleRect(points[i]);
            return rects;
        }

        private void ResizeSelection(int handleIndex, Point point)
        {
            point = new Point(
                Math.Max(0, Math.Min(Width, point.X)),
                Math.Max(0, Math.Min(Height, point.Y))
            );

            int left = selection.Left;
            int right = selection.Right;
            int top = selection.Top;
            int bottom = selection.Bottom;

            if (handleIndex == 0 || handleIndex == 3 || handleIndex == 5)
                left = point.X;
            if (handleIndex == 2 || handleIndex == 4 || handleIndex == 7)
                right = point.X;
            if (handleIndex == 0 || handleIndex == 1 || handleIndex == 2)
                top = point.Y;
            if (handleIndex == 5 || handleIndex == 6 || handleIndex == 7)
                bottom = point.Y;

            Rectangle resized = Normalize(new Point(left, top), new Point(right, bottom));
            if (resized.Width < 10 || resized.Height < 10)
                return;

            selection = resized;
        }
    }
}
