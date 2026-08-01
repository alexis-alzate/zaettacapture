using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class SettingsForm : Form
    {
        private readonly CheckBox keepSelection;
        private readonly CheckBox openLocked;
        private readonly Label hotkeyValue;
        private Keys selectedKey;
        private uint selectedModifiers;

        public bool KeepLastSelectionPosition
        {
            get { return keepSelection.Checked; }
        }

        public bool OpenLocked
        {
            get { return openLocked.Checked; }
        }

        public Keys SelectedKey
        {
            get { return selectedKey; }
        }

        public uint SelectedModifiers
        {
            get { return selectedModifiers; }
        }

        public SettingsForm(bool keepLastSelectionPosition, bool startOpenLocked, Keys hotkeyKey, uint hotkeyModifiers)
        {
            selectedKey = hotkeyKey;
            selectedModifiers = hotkeyModifiers;

            Text = "Opciones - " + AppInfo.Name;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            ClientSize = new Size(520, 360);
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Opciones";
            title.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            title.ForeColor = Ui.Text;
            title.SetBounds(24, 18, 360, 38);

            TabControl tabs = new TabControl();
            tabs.SetBounds(24, 72, 472, 210);
            tabs.Font = new Font("Segoe UI", 9f);

            TabPage general = new TabPage("General");
            general.BackColor = Ui.Bg;
            general.ForeColor = Ui.Text;

            keepSelection = BuildCheckBox("Mantener posicion del area seleccionada", keepLastSelectionPosition, 18, 24);
            openLocked = BuildCheckBox("Abrir capturas con candado", startOpenLocked, 18, 62);
            Label startup = BuildMutedLabel("Inicio con Windows activo automaticamente.", 20, 110, 390, 22);
            Label tray = BuildMutedLabel("Zaetta queda en bandeja despues de instalar o actualizar.", 20, 136, 410, 22);
            general.Controls.Add(keepSelection);
            general.Controls.Add(openLocked);
            general.Controls.Add(startup);
            general.Controls.Add(tray);

            TabPage hotkeys = new TabPage("Atajo");
            hotkeys.BackColor = Ui.Bg;
            hotkeys.ForeColor = Ui.Text;

            Label hotkeyTitle = new Label();
            hotkeyTitle.Text = "Atajo de captura";
            hotkeyTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            hotkeyTitle.ForeColor = Ui.Text;
            hotkeyTitle.SetBounds(20, 24, 220, 26);

            hotkeyValue = BuildMutedLabel(FormatHotkey(selectedKey, selectedModifiers), 20, 58, 260, 24);
            ZaettaButton changeHotkey = new ZaettaButton("Cambiar", true);
            changeHotkey.TextFill = Color.FromArgb(12, 12, 10);
            changeHotkey.SetBounds(308, 50, 120, 34);
            changeHotkey.Click += delegate { CaptureHotkey(); };

            ZaettaButton printScreen = new ZaettaButton("Impr Pant", false);
            printScreen.SetBounds(20, 112, 120, 34);
            printScreen.Click += delegate { SetHotkey(Keys.PrintScreen, 0); };

            ZaettaButton ctrlShift = new ZaettaButton("Ctrl Shift S", false);
            ctrlShift.SetBounds(150, 112, 120, 34);
            ctrlShift.Click += delegate { SetHotkey(Keys.S, HotKeyWindow.MOD_CONTROL | HotKeyWindow.MOD_SHIFT); };

            ZaettaButton ctrlAlt = new ZaettaButton("Ctrl Alt S", false);
            ctrlAlt.SetBounds(280, 112, 120, 34);
            ctrlAlt.Click += delegate { SetHotkey(Keys.S, HotKeyWindow.MOD_CONTROL | HotKeyWindow.MOD_ALT); };

            hotkeys.Controls.Add(hotkeyTitle);
            hotkeys.Controls.Add(hotkeyValue);
            hotkeys.Controls.Add(changeHotkey);
            hotkeys.Controls.Add(printScreen);
            hotkeys.Controls.Add(ctrlShift);
            hotkeys.Controls.Add(ctrlAlt);

            tabs.TabPages.Add(general);
            tabs.TabPages.Add(hotkeys);

            ZaettaButton cancel = new ZaettaButton("Cancelar", false);
            cancel.SetBounds(226, 306, 120, 36);
            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            ZaettaButton save = new ZaettaButton("Guardar", true);
            save.TextFill = Color.FromArgb(12, 12, 10);
            save.SetBounds(364, 306, 132, 36);
            save.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(title);
            Controls.Add(tabs);
            Controls.Add(cancel);
            Controls.Add(save);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private static CheckBox BuildCheckBox(string text, bool isChecked, int x, int y)
        {
            CheckBox box = new CheckBox();
            box.Text = text;
            box.Checked = isChecked;
            box.ForeColor = Ui.Text;
            box.BackColor = Ui.Bg;
            box.SetBounds(x, y, 390, 28);
            return box;
        }

        private static Label BuildMutedLabel(string text, int x, int y, int width, int height)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Color.FromArgb(210, 226, 232);
            label.BackColor = Ui.Bg;
            label.SetBounds(x, y, width, height);
            return label;
        }

        private void CaptureHotkey()
        {
            using (HotkeyCaptureForm dialog = new HotkeyCaptureForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                SetHotkey(dialog.SelectedKey, dialog.SelectedModifiers);
            }
        }

        private void SetHotkey(Keys key, uint modifiers)
        {
            selectedKey = key;
            selectedModifiers = modifiers;
            hotkeyValue.Text = FormatHotkey(selectedKey, selectedModifiers);
        }

        private static string FormatHotkey(Keys key, uint modifiers)
        {
            System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
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
