using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class TrayContext : ApplicationContext
    {
        private readonly NotifyIcon tray;
        private readonly HotKeyWindow hotKeyWindow;
        private ToolStripMenuItem printScreenItem;
        private ToolStripMenuItem ctrlShiftSItem;
        private ToolStripMenuItem ctrlAltSItem;
        private ToolStripMenuItem customHotkeyItem;
        private ToolStripMenuItem repeatLastAreaItem;
        private ToolStripMenuItem keepLastSelectionPositionItem;
        private Rectangle lastSelection;
        private bool hasLastSelection;
        private bool keepLastSelectionPosition;
        private bool captureActive;

        public TrayContext()
        {
            tray = new NotifyIcon();
            tray.Icon = LoadTrayIcon();
            tray.Text = AppInfo.Name;
            tray.Visible = true;
            tray.ContextMenuStrip = BuildMenu();
            tray.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    StartCapture();
            };

            hasLastSelection = LastSelectionStore.TryLoad(out lastSelection);
            keepLastSelectionPosition = CapturePreferencesStore.LoadKeepLastSelectionPosition();
            if (keepLastSelectionPositionItem != null)
                keepLastSelectionPositionItem.Checked = keepLastSelectionPosition;

            hotKeyWindow = new HotKeyWindow(StartCapture);
            hotKeyWindow.Register(Keys.PrintScreen, 0);
            printScreenItem.Checked = true;
            if (repeatLastAreaItem != null)
                repeatLastAreaItem.Enabled = hasLastSelection;
        }

        private static Icon LoadTrayIcon()
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null)
                    return icon;
            }
            catch
            {
            }

            return SystemIcons.Application;
        }

        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Capturar ahora", null, delegate { StartCapture(); });
            repeatLastAreaItem = new ToolStripMenuItem("Repetir ultima area", null, delegate { StartCapture(true); });
            repeatLastAreaItem.Enabled = false;
            menu.Items.Add(repeatLastAreaItem);
            keepLastSelectionPositionItem = new ToolStripMenuItem("Mantener posicion del area seleccionada", null, delegate { ToggleKeepLastSelectionPosition(); });
            keepLastSelectionPositionItem.Checked = true;
            keepLastSelectionPositionItem.CheckOnClick = true;
            menu.Items.Add(keepLastSelectionPositionItem);
            var hotkeys = new ToolStripMenuItem("Atajo de captura");
            printScreenItem = new ToolStripMenuItem("Impr Pant", null, delegate { SetHotkey(Keys.PrintScreen, 0, printScreenItem); });
            ctrlShiftSItem = new ToolStripMenuItem("Ctrl + Shift + S", null, delegate { SetHotkey(Keys.S, HotKeyWindow.MOD_CONTROL | HotKeyWindow.MOD_SHIFT, ctrlShiftSItem); });
            ctrlAltSItem = new ToolStripMenuItem("Ctrl + Alt + S", null, delegate { SetHotkey(Keys.S, HotKeyWindow.MOD_CONTROL | HotKeyWindow.MOD_ALT, ctrlAltSItem); });
            customHotkeyItem = new ToolStripMenuItem("Definir otro atajo...", null, delegate { CaptureCustomHotkey(); });
            hotkeys.DropDownItems.Add(printScreenItem);
            hotkeys.DropDownItems.Add(ctrlShiftSItem);
            hotkeys.DropDownItems.Add(ctrlAltSItem);
            hotkeys.DropDownItems.Add("-");
            hotkeys.DropDownItems.Add(customHotkeyItem);
            menu.Items.Add(hotkeys);
            menu.Items.Add("Abrir historial", null, delegate { OpenHistory(); });
            menu.Items.Add("Acerca de", null, delegate { ShowAbout(); });
            menu.Items.Add("-");
            menu.Items.Add("Salir", null, delegate { ExitThread(); });
            return menu;
        }

        private void SetHotkey(Keys key, uint modifiers, ToolStripMenuItem selected)
        {
            if (!hotKeyWindow.Register(key, modifiers))
            {
                MessageBox.Show("Ese atajo esta ocupado por Windows u otra aplicacion.", AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            printScreenItem.Checked = false;
            ctrlShiftSItem.Checked = false;
            ctrlAltSItem.Checked = false;
            customHotkeyItem.Checked = false;
            selected.Checked = true;
        }

        private void CaptureCustomHotkey()
        {
            using (HotkeyCaptureForm dialog = new HotkeyCaptureForm())
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;
                if (!hotKeyWindow.Register(dialog.SelectedKey, dialog.SelectedModifiers))
                {
                    MessageBox.Show("Ese atajo esta ocupado por Windows u otra aplicacion.", AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                printScreenItem.Checked = false;
                ctrlShiftSItem.Checked = false;
                ctrlAltSItem.Checked = false;
                customHotkeyItem.Checked = true;
                customHotkeyItem.Text = "Personalizado: " + dialog.DisplayText;
            }
        }

        private void OpenHistory()
        {
            HistoryService.Open();
        }

        private void ToggleKeepLastSelectionPosition()
        {
            keepLastSelectionPosition = keepLastSelectionPositionItem.Checked;
            CapturePreferencesStore.SaveKeepLastSelectionPosition(keepLastSelectionPosition);
        }

        private void StartCapture()
        {
            StartCapture(false);
        }

        private void StartCapture(bool useLastSelection)
        {
            if (captureActive)
                return;

            captureActive = true;
            try
            {
                ScreenshotCapture capture = ScreenshotService.CaptureVirtualScreen();
                bool shouldUseLastSelection = hasLastSelection && (useLastSelection || keepLastSelectionPosition);
                CaptureOverlay overlay = shouldUseLastSelection
                    ? new CaptureOverlay(capture.Bounds, capture.Image, lastSelection)
                    : new CaptureOverlay(capture.Bounds, capture.Image);
                overlay.FormClosed += delegate
                {
                    if (overlay.HasCompletedSelection)
                    {
                        lastSelection = overlay.CurrentSelection;
                        hasLastSelection = true;
                        LastSelectionStore.Save(lastSelection);
                        if (repeatLastAreaItem != null)
                            repeatLastAreaItem.Enabled = true;
                    }
                    captureActive = false;
                };
                overlay.Show();
            }
            catch (Exception ex)
            {
                captureActive = false;
                MessageBox.Show("No se pudo iniciar la captura.\n\n" + ex.Message, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                AppInfo.AboutText,
                AppInfo.AboutTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        protected override void ExitThreadCore()
        {
            hotKeyWindow.Dispose();
            tray.Visible = false;
            tray.Dispose();
            base.ExitThreadCore();
        }
    }
}
