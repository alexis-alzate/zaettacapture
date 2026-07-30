using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (Capture)
                return;

            bool hadDragState = movingOp != null || movingSelection || resizingOp != null || resizingSelection;
            movingOp = null;
            movingSelection = false;
            resizingOp = null;
            resizeHandleIndex = -1;
            resizingSelection = false;
            selectionResizeHandleIndex = -1;
            pendingRightCopy = false;

            if (hadDragState)
            {
                Cursor = tool == Tool.Text ? Cursors.IBeam : (tool == Tool.Move ? Cursors.SizeAll : Cursors.Cross);
                ShowToolbars();
                Invalidate();
            }
        }

        private void BeginRightCopy()
        {
            BeginRightCopy(this, PointToClient(Cursor.Position));
        }

        private void BeginRightCopy(Control owner, Point location)
        {
            pendingRightCopy = true;
            pendingRightOwner = owner;
            pendingRightLocation = location;
            Capture = true;
        }

        private void FinishRightCopy()
        {
            if (!pendingRightCopy)
                return;
            pendingRightCopy = false;
            Capture = false;
            if (selectionLocked)
                ShowCaptureContextMenu(pendingRightOwner ?? this, pendingRightLocation);
            else
                CopyAndClose();
            pendingRightOwner = null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();
            if (e.Button == MouseButtons.Left)
                leftButtonDown = true;
            if (e.Button == MouseButtons.Right && HasSelection())
            {
                BeginRightCopy(this, e.Location);
                return;
            }
            if (e.Button != MouseButtons.Left)
                return;
            if (!HasSelection())
            {
                selecting = true;
                start = e.Location;
                current = e.Location;
                HideToolbars();
                Invalidate();
                return;
            }
            selectionResizeHandleIndex = HitTestSelectionHandle(e.Location);
            if (selectionResizeHandleIndex >= 0)
            {
                resizingSelection = true;
                selectedOp = null;
                HideToolbars();
                Capture = true;
                Cursor = ResizeCursor(selectionResizeHandleIndex);
                Invalidate();
                return;
            }
            if (!selection.Contains(e.Location))
            {
                if (!selectionLocked)
                    Close();
                return;
            }
            if (tool == Tool.Move)
            {
                resizingOp = selectedOp;
                resizeHandleIndex = HitTestResizeHandle(resizingOp, e.Location);
                if (resizingOp != null && resizeHandleIndex >= 0)
                {
                    Capture = true;
                    Cursor = ResizeCursor(resizeHandleIndex);
                    return;
                }

                movingOp = HitTestOp(e.Location);
                if (movingOp != null)
                {
                    selectedOp = movingOp;
                    Rectangle bounds = GetOpBounds(movingOp);
                    moveOffset = new Point(e.X - bounds.Left, e.Y - bounds.Top);
                    Capture = true;
                    Cursor = Cursors.SizeAll;
                    Invalidate();
                }
                else
                {
                    selectedOp = null;
                    movingSelection = true;
                    selectionMoveOffset = new Point(e.X - selection.Left, e.Y - selection.Top);
                    HideToolbars();
                    Capture = true;
                    Cursor = Cursors.SizeAll;
                    Invalidate();
                }
                return;
            }
            selectedOp = null;
            if (tool == Tool.Text)
            {
                BeginTextEdit(e.Location);
                return;
            }
            if (tool == Tool.Number)
            {
                ops.Add(new DrawOp { Tool = Tool.Number, A = e.Location, Text = counterValue.ToString(), Color = color, Width = Math.Max(18, drawWidth * 5) });
                counterValue++;
                Invalidate();
                return;
            }
            drawing = true;
            drawStart = e.Location;
            current = e.Location;
            if (tool == Tool.Pencil || tool == Tool.Highlight)
            {
                DrawOp op = new DrawOp { Tool = tool, A = e.Location, B = e.Location, Color = color, Width = drawWidth, Points = new List<Point>() };
                op.Points.Add(ClampToSelection(e.Location));
                ops.Add(op);
            }
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            bool leftPressed = leftButtonDown || (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (!drawing || !leftPressed)
            {
                if (!drawing)
                {
                    DrawOp hoveredOp = HitTestOp(e.Location);
                    if (hoveredOp != null)
                    {
                        int delta = e.Delta > 0 ? 1 : -1;
                        if ((ModifierKeys & Keys.Control) == Keys.Control)
                            AdjustOpStrokeByWheel(hoveredOp, delta);
                        else
                            AdjustOpByWheel(hoveredOp, delta);
                        selectedOp = hoveredOp;
                        Invalidate();
                        return;
                    }
                }

                base.OnMouseWheel(e);
                return;
            }

            int wheelDelta = e.Delta > 0 ? 1 : -1;
            if (tool == Tool.Pixelate)
                AdjustPixelIntensity(wheelDelta * 2);
            else
                AdjustDrawWidth(wheelDelta);

            if ((tool == Tool.Pencil || tool == Tool.Highlight) && ops.Count > 0)
                ops[ops.Count - 1].Width = drawWidth;

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (resizingSelection && selectionResizeHandleIndex >= 0)
            {
                ResizeSelection(selectionResizeHandleIndex, e.Location);
                current = e.Location;
                Invalidate();
                return;
            }
            if (resizingOp != null && resizeHandleIndex >= 0)
            {
                ResizeOp(resizingOp, resizeHandleIndex, ClampToSelection(e.Location));
                Invalidate();
                return;
            }
            if (movingOp != null)
            {
                MoveOpTo(movingOp, new Point(e.X - moveOffset.X, e.Y - moveOffset.Y));
                Invalidate();
                return;
            }
            if (movingSelection)
            {
                MoveSelectionTo(new Point(e.X - selectionMoveOffset.X, e.Y - selectionMoveOffset.Y));
                Invalidate();
                return;
            }
            if (tool == Tool.Move && selectedOp != null)
            {
                int handle = HitTestResizeHandle(selectedOp, e.Location);
                Cursor = handle >= 0 ? ResizeCursor(handle) : (HitTestOp(e.Location) != null ? Cursors.SizeAll : Cursors.Default);
                return;
            }
            if (HasSelection() && !selecting && !drawing)
            {
                int selectionHandle = HitTestSelectionHandle(e.Location);
                if (selectionHandle >= 0)
                    Cursor = ResizeCursor(selectionHandle);
                else if (selection.Contains(e.Location))
                    Cursor = tool == Tool.Text ? Cursors.IBeam : (tool == Tool.Move ? Cursors.SizeAll : Cursors.Cross);
                else
                    Cursor = Cursors.Default;
            }
            if (drawing && (tool == Tool.Pencil || tool == Tool.Highlight) && ops.Count > 0)
            {
                ops[ops.Count - 1].Points.Add(ClampToSelection(e.Location));
                current = e.Location;
                Invalidate();
                return;
            }
            if (selecting || drawing)
            {
                current = e.Location;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                leftButtonDown = false;
            if (e.Button == MouseButtons.Right && pendingRightCopy)
            {
                FinishRightCopy();
                return;
            }
            if (resizingSelection)
            {
                resizingSelection = false;
                selectionResizeHandleIndex = -1;
                Capture = false;
                Cursor = tool == Tool.Text ? Cursors.IBeam : (tool == Tool.Move ? Cursors.SizeAll : Cursors.Cross);
                ShowToolbars();
                Invalidate();
                return;
            }
            if (resizingOp != null)
            {
                resizingOp = null;
                resizeHandleIndex = -1;
                Capture = false;
                Cursor = Cursors.SizeAll;
                Invalidate();
                return;
            }
            if (movingOp != null)
            {
                movingOp = null;
                Capture = false;
                Cursor = Cursors.SizeAll;
                Invalidate();
                return;
            }
            if (movingSelection)
            {
                movingSelection = false;
                Capture = false;
                Cursor = Cursors.SizeAll;
                ShowToolbars();
                Invalidate();
                return;
            }
            if (selecting)
            {
                selecting = false;
                current = e.Location;
                selection = Normalize(start, current);
                if (selection.Width < 10 || selection.Height < 10)
                {
                    Close();
                    return;
                }
                ShowToolbars();
                Invalidate();
                return;
            }
            if (!drawing)
                return;
            drawing = false;
            current = e.Location;
            if (tool == Tool.Pencil || tool == Tool.Highlight)
            {
                Invalidate();
                return;
            }
            Point end = ClampToSelection(current);
            ops.Add(new DrawOp { Tool = tool, A = ClampToSelection(drawStart), B = end, Color = color, Width = tool == Tool.Pixelate ? pixelIntensity : drawWidth });
            Invalidate();
        }
    }
}
