using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class HotkeyCaptureForm : Form
    {
        private readonly Label title;
        private readonly Label hint;

        public Keys SelectedKey { get; private set; }
        public uint SelectedModifiers { get; private set; }
        public string DisplayText { get; private set; }

        public HotkeyCaptureForm()
        {
            Text = "Definir atajo";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            KeyPreview = true;
            ClientSize = new Size(420, 150);
            BackColor = Color.FromArgb(8, 18, 25);

            title = new Label
            {
                Text = "Presione el nuevo atajo",
                AutoSize = false,
                Left = 24,
                Top = 24,
                Width = 372,
                Height = 32,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            hint = new Label
            {
                Text = "Ejemplos: Impr Pant, Suprimir, Ctrl + Shift + S. Use Esc para cancelar.",
                AutoSize = false,
                Left = 24,
                Top = 68,
                Width = 372,
                Height = 50,
                ForeColor = Color.FromArgb(205, 225, 235),
                Font = new Font("Segoe UI", 9, FontStyle.Regular)
            };

            Controls.Add(title);
            Controls.Add(hint);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if (key == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }

            if (key == Keys.ControlKey || key == Keys.ShiftKey || key == Keys.Menu)
                return true;

            uint modifiers = 0;
            if ((keyData & Keys.Control) == Keys.Control)
                modifiers |= HotKeyWindow.MOD_CONTROL;
            if ((keyData & Keys.Shift) == Keys.Shift)
                modifiers |= HotKeyWindow.MOD_SHIFT;
            if ((keyData & Keys.Alt) == Keys.Alt)
                modifiers |= HotKeyWindow.MOD_ALT;

            SelectedKey = key;
            SelectedModifiers = modifiers;
            DisplayText = FormatHotkey(key, modifiers);
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }

        private static string FormatHotkey(Keys key, uint modifiers)
        {
            List<string> parts = new List<string>();
            if ((modifiers & HotKeyWindow.MOD_CONTROL) == HotKeyWindow.MOD_CONTROL)
                parts.Add("Ctrl");
            if ((modifiers & HotKeyWindow.MOD_SHIFT) == HotKeyWindow.MOD_SHIFT)
                parts.Add("Shift");
            if ((modifiers & HotKeyWindow.MOD_ALT) == HotKeyWindow.MOD_ALT)
                parts.Add("Alt");
            parts.Add(KeyName(key));
            return string.Join(" + ", parts.ToArray());
        }

        private static string KeyName(Keys key)
        {
            if (key == Keys.PrintScreen)
                return "Impr Pant";
            if (key == Keys.Delete)
                return "Suprimir";
            if (key == Keys.Insert)
                return "Insertar";
            if (key == Keys.Space)
                return "Espacio";
            return key.ToString();
        }
    }
}
