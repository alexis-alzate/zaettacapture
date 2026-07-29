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
                if (activeTextBox != null)
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
                CopyAndClose();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.S)
            {
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
            if (activeTextBox == null && !e.Control && !e.Alt && !e.Shift && TryApplyToolShortcut(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
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
