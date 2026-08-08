using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class SettingsForm : Form
    {
        private readonly CheckBox keepSelection;
        private readonly CheckBox openLocked;
        private readonly Label hotkeyValue;
        private readonly Panel generalPanel;
        private readonly Panel hotkeyPanel;
        private readonly Panel newsPanel;
        private readonly Button generalNav;
        private readonly Button hotkeyNav;
        private readonly Button newsNav;
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
            ClientSize = new Size(620, 390);
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Opciones";
            title.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            title.ForeColor = Ui.Text;
            title.BackColor = Ui.Bg;
            title.SetBounds(28, 22, 220, 38);

            Label subtitle = new Label();
            subtitle.Text = "Ajustes de captura, atajo y novedades.";
            subtitle.ForeColor = Ui.Muted;
            subtitle.BackColor = Ui.Bg;
            subtitle.SetBounds(30, 58, 330, 22);

            Panel side = new Panel();
            side.BackColor = Color.FromArgb(10, 13, 16);
            side.SetBounds(24, 98, 146, 202);

            generalNav = BuildNavButton("General", 10, 12);
            generalNav.Click += delegate { ShowSection(0); };
            hotkeyNav = BuildNavButton("Atajo", 10, 58);
            hotkeyNav.Click += delegate { ShowSection(1); };
            newsNav = BuildNavButton("Novedades", 10, 104);
            newsNav.Click += delegate { ShowSection(2); };
            side.Controls.Add(generalNav);
            side.Controls.Add(hotkeyNav);
            side.Controls.Add(newsNav);

            generalPanel = BuildContentPanel();
            keepSelection = BuildCheckBox("Mantener la ultima area", keepLastSelectionPosition, 24, 28);
            Label keepText = BuildMutedLabel("Las capturas nuevas arrancan con el ultimo rectangulo usado.", 48, 57, 330, 20);
            openLocked = BuildCheckBox("Abrir capturas con candado", startOpenLocked, 24, 94);
            Label lockText = BuildMutedLabel("El area queda protegida hasta que la desbloquees.", 48, 123, 330, 20);
            Label startup = BuildInfoLine("Inicio con Windows", "Activo automaticamente.", 24, 164);
            Label tray = BuildInfoLine("Bandeja", "La app queda lista despues de instalar o actualizar.", 24, 190);
            generalPanel.Controls.Add(keepSelection);
            generalPanel.Controls.Add(keepText);
            generalPanel.Controls.Add(openLocked);
            generalPanel.Controls.Add(lockText);
            generalPanel.Controls.Add(startup);
            generalPanel.Controls.Add(tray);

            hotkeyPanel = BuildContentPanel();
            Label hotkeyTitle = BuildSectionTitle("Atajo de captura", 24, 24);
            hotkeyValue = BuildHotkeyValue(FormatHotkey(selectedKey, selectedModifiers), 24, 58);
            ZaettaButton changeHotkey = new ZaettaButton("Cambiar", true);
            changeHotkey.TextFill = Color.FromArgb(12, 12, 10);
            changeHotkey.SetBounds(298, 52, 118, 34);
            changeHotkey.Click += delegate { CaptureHotkey(); };
            Label presets = BuildMutedLabel("Presets rapidos", 24, 112, 180, 22);
            ZaettaButton printScreen = new ZaettaButton("Impr Pant", false);
            printScreen.SetBounds(24, 144, 118, 34);
            printScreen.Click += delegate { SetHotkey(Keys.PrintScreen, 0); };
            ZaettaButton ctrlShift = new ZaettaButton("Ctrl Shift S", false);
            ctrlShift.SetBounds(154, 144, 118, 34);
            ctrlShift.Click += delegate { SetHotkey(Keys.S, HotKeyWindow.MOD_CONTROL | HotKeyWindow.MOD_SHIFT); };
            ZaettaButton ctrlAlt = new ZaettaButton("Ctrl Alt S", false);
            ctrlAlt.SetBounds(284, 144, 118, 34);
            ctrlAlt.Click += delegate { SetHotkey(Keys.S, HotKeyWindow.MOD_CONTROL | HotKeyWindow.MOD_ALT); };
            hotkeyPanel.Controls.Add(hotkeyTitle);
            hotkeyPanel.Controls.Add(hotkeyValue);
            hotkeyPanel.Controls.Add(changeHotkey);
            hotkeyPanel.Controls.Add(presets);
            hotkeyPanel.Controls.Add(printScreen);
            hotkeyPanel.Controls.Add(ctrlShift);
            hotkeyPanel.Controls.Add(ctrlAlt);

            newsPanel = BuildContentPanel();
            Label newsTitle = BuildSectionTitle("Novedades", 24, 24);
            Label installedVersion = BuildHotkeyValue("Version instalada: " + AppInfo.Version, 24, 58);
            installedVersion.Size = new Size(362, 32);
            Label newsText = BuildMutedLabel("Consulta los cambios de esta version o revisa el historial completo de Zaetta Capture.", 24, 104, 362, 38);
            ZaettaButton currentRelease = new ZaettaButton("Ver cambios de esta version", true);
            currentRelease.TextFill = Color.FromArgb(12, 12, 10);
            currentRelease.SetBounds(24, 150, 210, 34);
            currentRelease.Click += delegate { OpenReleasePage(CurrentReleaseUrl()); };
            ZaettaButton releaseHistory = new ZaettaButton("Historial", false);
            releaseHistory.SetBounds(246, 150, 140, 34);
            releaseHistory.Click += delegate { OpenReleasePage("https://github.com/alexis-alzate/zaettacapture/releases"); };
            newsPanel.Controls.Add(newsTitle);
            newsPanel.Controls.Add(installedVersion);
            newsPanel.Controls.Add(newsText);
            newsPanel.Controls.Add(currentRelease);
            newsPanel.Controls.Add(releaseHistory);

            ZaettaButton cancel = new ZaettaButton("Cancelar", false);
            cancel.SetBounds(330, 326, 120, 36);
            cancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            ZaettaButton save = new ZaettaButton("Guardar", true);
            save.TextFill = Color.FromArgb(12, 12, 10);
            save.SetBounds(464, 326, 132, 36);
            save.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(title);
            Controls.Add(subtitle);
            Controls.Add(side);
            Controls.Add(generalPanel);
            Controls.Add(hotkeyPanel);
            Controls.Add(newsPanel);
            Controls.Add(cancel);
            Controls.Add(save);
            AcceptButton = save;
            CancelButton = cancel;
            ShowSection(0);
        }

        private static Panel BuildContentPanel()
        {
            Panel panel = new Panel();
            panel.BackColor = Ui.Panel;
            panel.SetBounds(186, 98, 410, 202);
            return panel;
        }

        private static Button BuildNavButton(string text, int x, int y)
        {
            Button button = new Button();
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            button.ForeColor = Ui.Text;
            button.BackColor = Color.FromArgb(14, 17, 20);
            button.Cursor = Cursors.Hand;
            button.SetBounds(x, y, 126, 34);
            return button;
        }

        private void ShowSection(int section)
        {
            generalPanel.Visible = section == 0;
            hotkeyPanel.Visible = section == 1;
            newsPanel.Visible = section == 2;
            SetNavState(generalNav, section == 0);
            SetNavState(hotkeyNav, section == 1);
            SetNavState(newsNav, section == 2);
        }

        private static string CurrentReleaseUrl()
        {
            return "https://github.com/alexis-alzate/zaettacapture/releases/tag/v" + AppInfo.Version;
        }

        private static void OpenReleasePage(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir la pagina de novedades.\n\n" + ex.Message,
                    AppInfo.Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private static void SetNavState(Button button, bool selected)
        {
            button.BackColor = selected ? Color.FromArgb(34, 28, 19) : Color.FromArgb(14, 17, 20);
            button.ForeColor = selected ? Ui.Accent2 : Ui.Text;
        }

        private static CheckBox BuildCheckBox(string text, bool isChecked, int x, int y)
        {
            CheckBox box = new CheckBox();
            box.Text = text;
            box.Checked = isChecked;
            box.ForeColor = Ui.Text;
            box.BackColor = Ui.Panel;
            box.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            box.SetBounds(x, y, 330, 28);
            return box;
        }

        private static Label BuildSectionTitle(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Ui.Text;
            label.BackColor = Ui.Panel;
            label.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            label.SetBounds(x, y, 250, 28);
            return label;
        }

        private static Label BuildHotkeyValue(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Ui.Accent2;
            label.BackColor = Color.FromArgb(12, 15, 18);
            label.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Padding = new Padding(12, 0, 0, 0);
            label.SetBounds(x, y, 250, 32);
            return label;
        }

        private static Label BuildMutedLabel(string text, int x, int y, int width, int height)
        {
            Label label = new Label();
            label.Text = text;
            label.ForeColor = Color.FromArgb(206, 215, 218);
            label.BackColor = Ui.Panel;
            label.SetBounds(x, y, width, height);
            return label;
        }

        private static Label BuildInfoLine(string name, string value, int x, int y)
        {
            Label label = new Label();
            label.Text = name + ": " + value;
            label.ForeColor = Ui.Muted;
            label.BackColor = Ui.Panel;
            label.SetBounds(x, y, 350, 20);
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
