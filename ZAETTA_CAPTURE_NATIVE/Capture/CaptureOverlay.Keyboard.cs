using System;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Activate();
            Focus();
            if (HasSelection())
            {
                ShowToolbars();
                Invalidate();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (activeTextBox != null || textEditing)
                {
                    CancelTextEdit();
                    e.SuppressKeyPress = true;
                    return;
                }
                Close();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                CommitTextEdit();
                CopyAndClose();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.S)
            {
                CommitTextEdit();
                SaveImage();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
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
            if (textEditing)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    CommitTextEdit();
                    e.SuppressKeyPress = true;
                    return;
                }
                if (e.KeyCode == Keys.Back)
                {
                    if (activeTextValue.Length > 0)
                        activeTextValue = activeTextValue.Substring(0, activeTextValue.Length - 1);
                    UpdateActiveTextBoundsForContent();
                    StartActiveTextCaret();
                    Invalidate();
                    e.SuppressKeyPress = true;
                    return;
                }
            }
            if (activeTextBox == null && !textEditing && !e.Control && !e.Alt && !e.Shift && TryApplyToolShortcut(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (textEditing)
            {
                if (!char.IsControl(e.KeyChar))
                {
                    activeTextValue += e.KeyChar;
                    UpdateActiveTextBoundsForContent();
                    StartActiveTextCaret();
                    Invalidate();
                }
                e.Handled = true;
                return;
            }
            base.OnKeyPress(e);
        }

        private bool TryApplyToolShortcut(Keys key)
        {
            Tool selected;
            if (!ToolShortcuts.TryGet(key, true, out selected))
                return false;

            SetTool(selected);
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CONTEXTMENU)
                return;
            base.WndProc(ref m);
        }
    }
}
