using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private void BeginTextEdit(Point location)
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
