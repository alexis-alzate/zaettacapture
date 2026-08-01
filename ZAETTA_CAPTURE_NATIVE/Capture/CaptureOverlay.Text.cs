using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private const int ActiveTextPadding = 3;
        private const int ActiveTextBorderHitPadding = 6;
        private const int ActiveTextMinWidth = 28;
        private const int ActiveTextMinHeight = 28;

        private void BeginTextEdit(Point location)
        {
            CommitTextEdit();
            Point p = ClampToSelection(location);
            textEditing = true;
            movingActiveText = false;
            activeTextSize = 18;
            activeTextBounds = CreateInitialTextBounds(p);
            activeTextPoint = p;
            activeTextValue = "";
            UpdateActiveTextPoint();
            StartActiveTextCaret();
            Focus();
            Invalidate();
        }

        private Rectangle CreateInitialTextBounds(Point textPoint)
        {
            int maxWidth = Math.Max(ActiveTextMinWidth, selection.Right - textPoint.X - ActiveTextPadding);
            int width = Math.Min(42, maxWidth);
            int maxHeight = Math.Max(ActiveTextMinHeight, selection.Bottom - textPoint.Y - ActiveTextPadding);
            int height = Math.Min(ActiveTextMinHeight, maxHeight);
            return new Rectangle(textPoint.X, textPoint.Y, width, height);
        }

        private void UpdateActiveTextPoint()
        {
            activeTextPoint = new Point(activeTextBounds.Left + ActiveTextPadding, activeTextBounds.Top + ActiveTextPadding - 2);
        }

        private void UpdateActiveTextBoundsForContent()
        {
            if (!textEditing)
                return;

            using (Graphics g = CreateGraphics())
            using (Font font = new Font("Segoe UI", activeTextSize, FontStyle.Bold))
            {
                string preview = string.IsNullOrEmpty(activeTextValue) ? " " : activeTextValue;
                SizeF measured = g.MeasureString(preview, font);
                int maxWidth = Math.Max(ActiveTextMinWidth, selection.Right - activeTextBounds.Left);
                int maxHeight = Math.Max(ActiveTextMinHeight, selection.Bottom - activeTextBounds.Top);
                int width = Math.Min(maxWidth, Math.Max(ActiveTextMinWidth, (int)Math.Ceiling(measured.Width) + (ActiveTextPadding * 2) + 4));
                int height = Math.Min(maxHeight, Math.Max(ActiveTextMinHeight, (int)Math.Ceiling(measured.Height) + (ActiveTextPadding * 2)));
                activeTextBounds = new Rectangle(activeTextBounds.Location, new Size(width, height));
                UpdateActiveTextPoint();
            }
        }

        private void StartActiveTextCaret()
        {
            activeTextCaretVisible = true;
            if (activeTextCaretTimer == null)
            {
                activeTextCaretTimer = new Timer();
                activeTextCaretTimer.Interval = 520;
                activeTextCaretTimer.Tick += delegate
                {
                    if (!textEditing)
                    {
                        activeTextCaretTimer.Stop();
                        return;
                    }

                    activeTextCaretVisible = !activeTextCaretVisible;
                    Invalidate(activeTextBounds);
                };
            }
            activeTextCaretTimer.Stop();
            activeTextCaretTimer.Start();
        }

        private void StopActiveTextCaret()
        {
            activeTextCaretVisible = false;
            if (activeTextCaretTimer != null)
                activeTextCaretTimer.Stop();
        }

        private bool HitTestActiveTextBorder(Point point)
        {
            if (!textEditing || activeTextBounds.IsEmpty)
                return false;

            Rectangle outer = activeTextBounds;
            outer.Inflate(ActiveTextBorderHitPadding, ActiveTextBorderHitPadding);
            if (!outer.Contains(point))
                return false;

            Rectangle inner = activeTextBounds;
            inner.Inflate(-ActiveTextBorderHitPadding, -ActiveTextBorderHitPadding);
            return !inner.Contains(point);
        }

        private bool HitTestActiveText(Point point)
        {
            if (!textEditing || activeTextBounds.IsEmpty)
                return false;

            Rectangle hit = activeTextBounds;
            hit.Inflate(ActiveTextBorderHitPadding, ActiveTextBorderHitPadding);
            return hit.Contains(point);
        }

        private void MoveActiveTextTo(Point requestedTopLeft)
        {
            Point target = ClampBoundsTopLeft(requestedTopLeft, activeTextBounds.Size);
            if (target == activeTextBounds.Location)
                return;

            activeTextBounds = new Rectangle(target, activeTextBounds.Size);
            UpdateActiveTextPoint();
        }

        private void AdjustActiveTextSize(int delta)
        {
            activeTextSize = Math.Max(10, Math.Min(54, activeTextSize + delta));
            UpdateActiveTextBoundsForContent();
            Invalidate();
        }

        private void BeginLegacyTextEdit(Point location)
        {
            CommitTextEdit();
            Point p = ClampToSelection(location);
            activeTextBox = new TextBox();
            activeTextBox.BorderStyle = BorderStyle.FixedSingle;
            activeTextBox.Multiline = true;
            activeTextBox.AcceptsReturn = true;
            activeTextBox.WordWrap = true;
            activeTextBox.BackColor = Color.FromArgb(248, 252, 255);
            activeTextBox.ForeColor = Color.FromArgb(10, 18, 24);
            activeTextBox.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            activeTextBox.ShortcutsEnabled = false;
            activeTextBox.ContextMenuStrip = ContextMenus.Suppressed();
            activeTextBox.Left = p.X;
            activeTextBox.Top = p.Y;
            activeTextBox.Width = Math.Max(180, Math.Min(360, selection.Right - p.X - 8));
            activeTextBox.Height = 42;
            activeTextBox.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy(activeTextBox, e.Location);
            };
            activeTextBox.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                {
                    BeginInvoke(new Action(delegate
                    {
                        CommitTextEdit();
                        FinishRightCopy();
                    }));
                }
            };
            activeTextBox.KeyDown += ActiveTextBox_KeyDown;
            activeTextBox.TextChanged += delegate
            {
                using (Graphics g = activeTextBox.CreateGraphics())
                {
                    SizeF size = g.MeasureString(activeTextBox.Text + " ", activeTextBox.Font, activeTextBox.Width);
                    activeTextBox.Height = Math.Max(42, Math.Min(160, (int)size.Height + 18));
                }
            };
            Controls.Add(activeTextBox);
            activeTextBox.BringToFront();
            activeTextBox.Focus();
        }

        private void ActiveTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CommitTextEdit();
                CopyAndClose();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.L)
            {
                CommitTextEdit();
                ToggleSelectionLock();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                CancelTextEdit();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                CommitTextEdit();
                e.SuppressKeyPress = true;
            }
        }

        private void CommitTextEdit()
        {
            if (textEditing)
            {
                textEditing = false;
                movingActiveText = false;
                StopActiveTextCaret();
                string value = (activeTextValue ?? "").Trim();
                activeTextValue = "";
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ops.Add(new DrawOp { Tool = Tool.Text, A = activeTextPoint, Text = value, Color = color, Width = activeTextSize });
                    Invalidate();
                }
                activeTextBounds = Rectangle.Empty;
                return;
            }
            if (activeTextBox == null)
                return;
            TextBox box = activeTextBox;
            activeTextBox = null;
            string text = box.Text.Trim();
            Point p = new Point(box.Left, box.Top);
            Controls.Remove(box);
            box.Dispose();
            if (!string.IsNullOrWhiteSpace(text))
            {
                ops.Add(new DrawOp { Tool = Tool.Text, A = p, Text = text, Color = color, Width = 18 });
                Invalidate();
            }
        }

        private void CancelTextEdit()
        {
            if (textEditing)
            {
                textEditing = false;
                movingActiveText = false;
                StopActiveTextCaret();
                activeTextValue = "";
                activeTextBounds = Rectangle.Empty;
                Invalidate();
                return;
            }
            if (activeTextBox == null)
                return;
            TextBox box = activeTextBox;
            activeTextBox = null;
            Controls.Remove(box);
            box.Dispose();
            Invalidate();
        }
        private static string PromptText()
        {
            using (Form prompt = new Form())
            using (TextBox input = new TextBox())
            using (Button ok = new Button())
            {
                prompt.Text = "Texto";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.ClientSize = new Size(360, 92);
                input.Left = 14;
                input.Top = 16;
                input.Width = 330;
                ok.Text = "Agregar";
                ok.Left = 230;
                ok.Top = 52;
                ok.Width = 114;
                ok.DialogResult = DialogResult.OK;
                prompt.Controls.Add(input);
                prompt.Controls.Add(ok);
                prompt.AcceptButton = ok;
                return prompt.ShowDialog() == DialogResult.OK ? input.Text : "";
            }
        }
    }
}
