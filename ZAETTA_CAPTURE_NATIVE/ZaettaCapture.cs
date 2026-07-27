using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class Ui
    {
        public static readonly Color Bg = Color.FromArgb(7, 16, 25);
        public static readonly Color Panel = Color.FromArgb(16, 29, 41);
        public static readonly Color Panel2 = Color.FromArgb(23, 42, 58);
        public static readonly Color Text = Color.FromArgb(246, 251, 255);
        public static readonly Color Muted = Color.FromArgb(158, 179, 198);
        public static readonly Color Accent = Color.FromArgb(21, 173, 216);
        public static readonly Color Accent2 = Color.FromArgb(32, 196, 244);
    }

    internal static class ContextMenus
    {
        public static ContextMenuStrip Suppressed()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Opening += delegate(object sender, System.ComponentModel.CancelEventArgs e)
            {
                e.Cancel = true;
            };
            return menu;
        }
    }

    internal static class DrawingStyle
    {
        public static void ConfigureLineCap(Pen pen, Tool tool)
        {
            pen.StartCap = LineCap.Round;
            if (tool == Tool.Arrow)
                pen.CustomEndCap = new AdjustableArrowCap(5.8f, 7.2f, true);
            else
                pen.EndCap = LineCap.Round;
        }
    }

    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color bg = Color.FromArgb(7, 16, 25);
        private readonly Color accent = Color.FromArgb(23, 42, 58);
        private readonly Color border = Color.FromArgb(32, 196, 244);

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(42, 64, 78)))
                e.Graphics.DrawLine(pen, 8, e.Item.Height / 2, e.Item.Width - 8, e.Item.Height / 2);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
            using (SolidBrush brush = new SolidBrush(e.Item.Selected ? accent : bg))
                e.Graphics.FillRectangle(brush, rect);
            if (e.Item.Selected)
            {
                using (Pen pen = new Pen(border, 1))
                    e.Graphics.DrawRectangle(pen, 1, 1, rect.Width - 3, rect.Height - 3);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(36, 69, 86)))
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }
    }

    internal sealed class ZaettaButton : Button
    {
        public Color Fill { get; set; }
        public Color HoverFill { get; set; }
        public Color TextFill { get; set; }
        public int Radius { get; set; }
        public bool OutlineOnly { get; set; }
        private bool hovering;

        public ZaettaButton(string text, bool primary)
        {
            Text = text;
            Fill = primary ? Ui.Accent : Color.FromArgb(12, 23, 31);
            HoverFill = primary ? Ui.Accent2 : Color.FromArgb(20, 38, 49);
            TextFill = Color.White;
            Radius = 2;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(3, 8, 13);
            ForeColor = TextFill;
            Font = new Font("Segoe UI", 8, FontStyle.Bold);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.None;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(3, 8, 13)))
                e.Graphics.FillRectangle(bg, ClientRectangle);
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (SolidBrush brush = new SolidBrush(hovering ? HoverFill : Fill))
            {
                if (!OutlineOnly)
                    e.Graphics.FillRectangle(brush, rect);
                else
                    using (Pen pen = new Pen(hovering ? HoverFill : Fill, 1))
                        e.Graphics.DrawRectangle(pen, rect);
            }
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                TextFill,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            );
        }

        private static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            int d = radius * 2;
            rect.Width -= 1;
            rect.Height -= 1;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class ColorSwatchButton : Button
    {
        public Color Swatch { get; set; }
        private bool hovering;

        public ColorSwatchButton()
        {
            Swatch = Color.FromArgb(255, 59, 48);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(3, 8, 13);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (SolidBrush baseBg = new SolidBrush(Color.FromArgb(3, 8, 13)))
                e.Graphics.FillRectangle(baseBg, ClientRectangle);
            Rectangle dot = new Rectangle(Width / 2 - 8, Height / 2 - 8, 16, 16);
            using (SolidBrush brush = new SolidBrush(Swatch))
            using (Pen ring = new Pen(hovering ? Color.White : Color.FromArgb(190, 255, 255, 255), 1))
            {
                e.Graphics.FillEllipse(brush, dot);
                e.Graphics.DrawEllipse(ring, dot);
            }
        }

        private static GraphicsPath Rounded(Rectangle rect, int radius)
        {
            int d = radius * 2;
            rect.Width -= 1;
            rect.Height -= 1;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class FloatingToolbarPanel : Panel
    {
        public FloatingToolbarPanel()
        {
            BackColor = Color.FromArgb(244, 3, 8, 13);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.FromArgb(90, 255, 255, 255), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayContext());
        }
    }

    internal sealed class TrayContext : ApplicationContext
    {
        private readonly NotifyIcon tray;
        private readonly HotKeyWindow hotKeyWindow;
        private ToolStripMenuItem printScreenItem;
        private ToolStripMenuItem ctrlShiftSItem;
        private ToolStripMenuItem ctrlAltSItem;
        private ToolStripMenuItem customHotkeyItem;

        public TrayContext()
        {
            tray = new NotifyIcon();
            tray.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            tray.Text = "Zaetta Capture";
            tray.Visible = true;
            tray.ContextMenuStrip = BuildMenu();
            tray.MouseClick += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                    StartCapture();
            };

            hotKeyWindow = new HotKeyWindow(StartCapture);
            hotKeyWindow.Register(Keys.PrintScreen, 0);
            printScreenItem.Checked = true;
        }

        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Capturar ahora", null, delegate { StartCapture(); });
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
                MessageBox.Show("Ese atajo esta ocupado por Windows u otra aplicacion.", "Zaetta Capture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show("Ese atajo esta ocupado por Windows u otra aplicacion.", "Zaetta Capture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            Directory.CreateDirectory(Paths.HistoryDir);
            System.Diagnostics.Process.Start(Paths.HistoryDir);
        }

        private void StartCapture()
        {
            Rectangle bounds = Screen.FromPoint(Cursor.Position).Bounds;
            Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            var overlay = new CaptureOverlay(bounds, screenshot);
            overlay.Show();
        }

        private void ShowAbout()
        {
            MessageBox.Show(
                "Zaetta Capture\n\nVersion 1.0\n\nDesarrollador:\nVictor Alexis Alzate Cortes",
                "Acerca de Zaetta Capture",
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

    internal sealed class HotKeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        private readonly Action action;
        private readonly SynchronizationContext syncContext;
        private readonly KeyboardShortcutHook keyboardHook;
        private int currentId = 100;
        private bool registered;
        private Keys activeKey;
        private uint activeModifiers;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotKeyWindow(Action action)
        {
            this.action = action;
            this.syncContext = SynchronizationContext.Current;
            this.keyboardHook = new KeyboardShortcutHook(TriggerAction);
            CreateHandle(new CreateParams());
        }

        public bool Register(Keys key, uint modifiers)
        {
            keyboardHook.Clear();
            if (registered)
            {
                UnregisterHotKey(Handle, currentId);
                registered = false;
            }

            activeKey = key;
            activeModifiers = modifiers;

            if (key == Keys.PrintScreen && modifiers == 0)
            {
                keyboardHook.SetShortcut(key, modifiers);
                return true;
            }

            registered = RegisterHotKey(Handle, currentId, modifiers, (uint)key);
            if (!registered && key == Keys.PrintScreen)
            {
                keyboardHook.SetShortcut(key, modifiers);
                return true;
            }
            return registered;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                TriggerAction();
                return;
            }
            base.WndProc(ref m);
        }

        private void TriggerAction()
        {
            if (syncContext != null)
                syncContext.Post(delegate { action(); }, null);
            else
                action();
        }

        public void Dispose()
        {
            keyboardHook.Dispose();
            if (registered)
                UnregisterHotKey(Handle, currentId);
            DestroyHandle();
        }
    }

    internal sealed class KeyboardShortcutHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int HC_ACTION = 0;

        private readonly Action action;
        private readonly LowLevelKeyboardProc proc;
        private IntPtr hookId;
        private Keys key;
        private uint modifiers;
        private bool enabled;
        private DateTime lastTrigger = DateTime.MinValue;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public KeyboardShortcutHook(Action action)
        {
            this.action = action;
            proc = HookCallback;
        }

        public void SetShortcut(Keys key, uint modifiers)
        {
            this.key = key;
            this.modifiers = modifiers;
            enabled = true;
            EnsureHook();
        }

        public void Clear()
        {
            enabled = false;
        }

        private void EnsureHook()
        {
            if (hookId != IntPtr.Zero)
                return;
            hookId = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (enabled && nCode == HC_ACTION && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys pressed = (Keys)vkCode;
                if (pressed == key && ModifiersMatch())
                {
                    DateTime now = DateTime.Now;
                    if ((now - lastTrigger).TotalMilliseconds > 350)
                    {
                        lastTrigger = now;
                        action();
                    }
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private bool ModifiersMatch()
        {
            bool ctrl = IsPressed(Keys.ControlKey) || IsPressed(Keys.LControlKey) || IsPressed(Keys.RControlKey);
            bool shift = IsPressed(Keys.ShiftKey) || IsPressed(Keys.LShiftKey) || IsPressed(Keys.RShiftKey);
            bool alt = IsPressed(Keys.Menu) || IsPressed(Keys.LMenu) || IsPressed(Keys.RMenu);

            if (((modifiers & HotKeyWindow.MOD_CONTROL) == HotKeyWindow.MOD_CONTROL) != ctrl)
                return false;
            if (((modifiers & HotKeyWindow.MOD_SHIFT) == HotKeyWindow.MOD_SHIFT) != shift)
                return false;
            if (((modifiers & HotKeyWindow.MOD_ALT) == HotKeyWindow.MOD_ALT) != alt)
                return false;
            return true;
        }

        private static bool IsPressed(Keys key)
        {
            return (GetAsyncKeyState(key) & 0x8000) != 0;
        }

        public void Dispose()
        {
            if (hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookId);
                hookId = IntPtr.Zero;
            }
        }
    }

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

    internal static class ClipboardHelper
    {
        public static void SetImageWithRetry(Bitmap image)
        {
            Exception lastError = null;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetImage(image);
                    return;
                }
                catch (ExternalException ex)
                {
                    lastError = ex;
                    System.Threading.Thread.Sleep(80);
                }
            }

            image.Dispose();
            MessageBox.Show(
                "No se pudo copiar la captura al portapapeles. Intente de nuevo en unos segundos.",
                "Zaetta Capture",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            if (lastError != null)
                return;
        }
    }

    internal static class Paths
    {
        public static readonly string BaseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "Zaetta Capture"
        );
        public static readonly string HistoryDir = Path.Combine(BaseDir, "Historial");
    }

    internal sealed class CaptureOverlay : Form
    {
        private const int WM_CONTEXTMENU = 0x007B;
        private readonly Bitmap screenshot;
        private readonly Bitmap dimmedScreenshot;
        private readonly List<DrawOp> ops = new List<DrawOp>();
        private Rectangle selection;
        private Point start;
        private Point current;
        private Point drawStart;
        private bool selecting;
        private bool drawing;
        private Tool tool = Tool.Arrow;
        private Color color = Color.FromArgb(255, 59, 48);
        private int drawWidth = 4;
        private int counterValue = 1;
        private Panel bottomToolbar;
        private Panel sideToolbar;
        private TextBox activeTextBox;
        private DrawOp selectedOp;
        private DrawOp movingOp;
        private DrawOp resizingOp;
        private int resizeHandleIndex = -1;
        private bool resizingSelection;
        private int selectionResizeHandleIndex = -1;
        private Point moveOffset;
        private bool pendingRightCopy;
        private readonly ToolTip tips = new ToolTip();
        private static readonly Color SelectionStroke = Color.FromArgb(245, 245, 245);
        private static readonly Color SelectionShadow = Color.FromArgb(72, 24, 24, 24);
        private static readonly Color SelectionDash = Color.FromArgb(42, 42, 42);
        private static readonly Color SelectionHandle = Color.FromArgb(235, 245, 245, 245);
        private static readonly Color SelectionHandleBorder = Color.FromArgb(95, 95, 95);
        private static readonly Color SelectionLabelBg = Color.FromArgb(232, 20, 20, 20);

        public CaptureOverlay(Rectangle screenBounds, Bitmap screenshot)
        {
            this.screenshot = screenshot;
            this.dimmedScreenshot = BuildDimmedScreenshot(screenshot);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = screenBounds.Location;
            Size = screenBounds.Size;
            TopMost = true;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = Cursors.Cross;
            KeyPreview = true;
            ContextMenuStrip = ContextMenus.Suppressed();
            ConfigureTips(tips);
            BackColor = Color.Black;
            Opacity = 1;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Activate();
            Focus();
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

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);
            if (Capture)
                return;

            bool hadDragState = movingOp != null || resizingOp != null || resizingSelection;
            movingOp = null;
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
            pendingRightCopy = true;
            Capture = true;
        }

        private void FinishRightCopy()
        {
            if (!pendingRightCopy)
                return;
            pendingRightCopy = false;
            Capture = false;
            CopyAndClose();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && HasSelection())
            {
                BeginRightCopy();
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
            ops.Add(new DrawOp { Tool = tool, A = ClampToSelection(drawStart), B = end, Color = color, Width = drawWidth });
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImageUnscaled(dimmedScreenshot, Point.Empty);

            Rectangle box = HasSelection() ? selection : Normalize(start, current);
            if ((selecting || HasSelection()) && box.Width > 0 && box.Height > 0)
            {
                g.DrawImage(screenshot, box, box, GraphicsUnit.Pixel);
                foreach (DrawOp op in ops)
                    DrawOpOnOverlay(g, op);
                if (drawing && tool != Tool.Pencil && tool != Tool.Highlight)
                    DrawOpOnOverlay(g, new DrawOp { Tool = tool, A = ClampToSelection(drawStart), B = ClampToSelection(current), Color = color, Width = drawWidth });
                DrawSelectedOpHandles(g);
                DrawSelectionBorder(g, box);
                DrawHandles(g, box);
                DrawSizeLabel(g, box);
            }
        }

        private void ShowToolbars()
        {
            HideToolbars();
            bottomToolbar = new FloatingToolbarPanel { Width = 506, Height = 34 };
            bottomToolbar.Left = Math.Max(8, Math.Min(selection.Left, ClientSize.Width - bottomToolbar.Width - 8));
            bottomToolbar.Top = selection.Bottom + 8 < ClientSize.Height - bottomToolbar.Height
                ? selection.Bottom + 8
                : Math.Max(8, selection.Top - bottomToolbar.Height - 8);
            bottomToolbar.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            bottomToolbar.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            Controls.Add(bottomToolbar);
            bottomToolbar.BringToFront();

            ZaettaButton brand = AddToolButton(bottomToolbar, "Z", 5, 4, 28, false, delegate { ShowAbout(); }, "Acerca de Zaetta Capture.");
            brand.OutlineOnly = false;
            brand.Fill = Color.FromArgb(3, 8, 13);
            brand.HoverFill = Color.FromArgb(10, 22, 30);
            brand.TextFill = Color.FromArgb(32, 196, 244);
            brand.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            brand.Invalidate();
            ZaettaButton toolButton = AddToolButton(bottomToolbar, ToolName(tool), 38, 4, 72, false, delegate { }, "Herramienta activa.");
            toolButton.TextFill = Color.FromArgb(238, 246, 250);
            toolButton.Invalidate();
            ColorSwatchButton colorButton = AddColorButton(bottomToolbar, 116, 4, 28);
            tips.SetToolTip(colorButton, "Cambiar color de flechas, marcos, texto y resaltador.");
            colorButton.Click += delegate { ShowColorMenu(colorButton); };
            AddToolButton(bottomToolbar, "-", 150, 4, 28, false, delegate { Thinner(); }, "Disminuir grosor del trazo.");
            AddToolButton(bottomToolbar, "+", 184, 4, 28, false, delegate { Thicker(); }, "Aumentar grosor del trazo.");
            AddToolButton(bottomToolbar, "Undo", 218, 4, 42, false, delegate { Undo(); }, "Deshacer el ultimo cambio.");
            ZaettaButton moreBottom = AddToolButton(bottomToolbar, "...", 266, 4, 34, false, delegate { }, "Ver mas herramientas.");
            moreBottom.Click += delegate { ShowToolsMenu(moreBottom); };
            AddToolButton(bottomToolbar, "Copiar", 306, 4, 68, false, delegate { CopyAndClose(); }, "Copiar al portapapeles y cerrar la captura.");
            AddToolButton(bottomToolbar, "Guardar", 380, 4, 70, true, delegate { SaveImage(); }, "Guardar la captura como archivo PNG.");
            ZaettaButton close = AddToolButton(bottomToolbar, "X", 456, 4, 44, false, delegate { Close(); }, "Cancelar y cerrar sin copiar.");
            close.Fill = Color.FromArgb(120, 32, 42);
            close.HoverFill = Color.FromArgb(170, 48, 58);
            close.Invalidate();

            sideToolbar = new FloatingToolbarPanel { Width = 36, Height = 188 };
            sideToolbar.Left = selection.Right + 6 < ClientSize.Width - sideToolbar.Width
                ? selection.Right + 6
                : Math.Max(8, selection.Left - sideToolbar.Width - 6);
            sideToolbar.Top = Math.Max(8, Math.Min(selection.Top, ClientSize.Height - sideToolbar.Height - 8));
            sideToolbar.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            sideToolbar.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            Controls.Add(sideToolbar);
            sideToolbar.BringToFront();
            AddToolButton(sideToolbar, "P", 4, 4, 28, tool == Tool.Move, delegate { SetTool(Tool.Move); }, "Mover cualquier anotacion agregada.");
            AddToolButton(sideToolbar, "->", 4, 34, 28, tool == Tool.Arrow, delegate { SetTool(Tool.Arrow); }, "Dibujar flechas para senalar puntos importantes.");
            AddToolButton(sideToolbar, "[]", 4, 64, 28, tool == Tool.Rect, delegate { SetTool(Tool.Rect); }, "Dibujar marcos para resaltar areas.");
            AddToolButton(sideToolbar, "T", 4, 94, 28, tool == Tool.Text, delegate { SetTool(Tool.Text); }, "Agregar texto editable dentro de la captura.");
            AddToolButton(sideToolbar, "Px", 4, 124, 28, tool == Tool.Pixelate, delegate { SetTool(Tool.Pixelate); }, "Pixelar informacion sensible.");
            ZaettaButton moreSide = AddToolButton(sideToolbar, "...", 4, 154, 28, false, delegate { }, "Ver herramientas adicionales.");
            moreSide.Click += delegate { ShowToolsMenu(moreSide); };
        }

        private ZaettaButton AddToolButton(Panel parent, string text, int x, int y, int width, bool primary, EventHandler click, string tooltip)
        {
            ZaettaButton button = new ZaettaButton(text, primary);
            button.Left = x;
            button.Top = y;
            button.Width = width;
            button.Height = 26;
            button.Click += click;
            tips.SetToolTip(button, tooltip);
            button.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            button.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            parent.Controls.Add(button);
            return button;
        }

        private static void ConfigureTips(ToolTip tooltip)
        {
            tooltip.InitialDelay = 350;
            tooltip.ReshowDelay = 100;
            tooltip.AutoPopDelay = 6500;
            tooltip.ShowAlways = true;
        }

        private ColorSwatchButton AddColorButton(Panel parent, int x, int y, int size)
        {
            ColorSwatchButton button = new ColorSwatchButton();
            button.Left = x;
            button.Top = y;
            button.Width = size;
            button.Height = 26;
            button.Swatch = color;
            button.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            button.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            parent.Controls.Add(button);
            return button;
        }

        private void HideToolbars()
        {
            if (bottomToolbar != null)
            {
                Controls.Remove(bottomToolbar);
                bottomToolbar.Dispose();
                bottomToolbar = null;
            }
            if (sideToolbar != null)
            {
                Controls.Remove(sideToolbar);
                sideToolbar.Dispose();
                sideToolbar = null;
            }
        }

        private void SetTool(Tool selected)
        {
            tool = selected;
            Cursor = tool == Tool.Text ? Cursors.IBeam : (tool == Tool.Move ? Cursors.SizeAll : Cursors.Cross);
            ShowToolbars();
            Invalidate();
        }

        private string ToolName(Tool selected)
        {
            if (selected == Tool.Move) return "Mover";
            if (selected == Tool.Arrow) return "Flecha";
            if (selected == Tool.Rect) return "Marco";
            if (selected == Tool.Line) return "Linea";
            if (selected == Tool.Pencil) return "Lapiz";
            if (selected == Tool.Highlight) return "Resaltar";
            if (selected == Tool.Pixelate) return "Pixelar";
            if (selected == Tool.Number) return "Numero";
            if (selected == Tool.Text) return "Texto";
            return "Herramienta";
        }

        private void ShowToolsMenu(Control anchor)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(7, 16, 25);
            menu.ForeColor = Color.FromArgb(238, 246, 250);
            menu.ShowImageMargin = true;
            menu.ImageScalingSize = new Size(14, 14);
            menu.ShowItemToolTips = true;
            menu.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            menu.Padding = new Padding(2, 3, 2, 3);
            menu.Renderer = new DarkMenuRenderer();
            AddToolMenuItem(menu, Tool.Move);
            AddToolMenuItem(menu, Tool.Arrow);
            AddToolMenuItem(menu, Tool.Rect);
            AddToolMenuItem(menu, Tool.Line);
            AddToolMenuItem(menu, Tool.Pencil);
            AddToolMenuItem(menu, Tool.Highlight);
            AddToolMenuItem(menu, Tool.Pixelate);
            AddToolMenuItem(menu, Tool.Number);
            AddToolMenuItem(menu, Tool.Text);
            menu.Show(anchor, new Point(0, anchor.Height + 4));
        }

        private void ShowColorMenu(Control anchor)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(7, 16, 25);
            menu.ForeColor = Color.FromArgb(238, 246, 250);
            menu.ShowImageMargin = true;
            menu.ImageScalingSize = new Size(14, 14);
            menu.ShowItemToolTips = true;
            menu.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            menu.Padding = new Padding(2, 3, 2, 3);
            menu.Renderer = new DarkMenuRenderer();
            AddColorMenuItem(menu, "Zaetta", Color.FromArgb(0, 255, 210));
            AddColorMenuItem(menu, "Rojo", Color.FromArgb(255, 59, 48));
            AddColorMenuItem(menu, "Amarillo", Color.FromArgb(255, 204, 0));
            AddColorMenuItem(menu, "Verde", Color.FromArgb(52, 199, 89));
            AddColorMenuItem(menu, "Azul", Color.FromArgb(32, 196, 244));
            AddColorMenuItem(menu, "Blanco", Color.White);
            menu.Show(anchor, new Point(0, anchor.Height + 4));
        }

        private void AddColorMenuItem(ContextMenuStrip menu, string name, Color selected)
        {
            string label = (color.ToArgb() == selected.ToArgb() ? "> " : "  ") + name;
            ToolStripMenuItem item = new ToolStripMenuItem(label);
            item.Image = BuildColorIcon(selected);
            item.Padding = new Padding(4, 2, 10, 2);
            item.ToolTipText = "Usar color " + name + " en las anotaciones.";
            item.Click += delegate
            {
                color = selected;
                ShowToolbars();
                Invalidate();
            };
            menu.Items.Add(item);
        }

        private void AddToolMenuItem(ContextMenuStrip menu, Tool selected)
        {
            string label = (tool == selected ? "> " : "  ") + ToolName(selected);
            ToolStripMenuItem item = new ToolStripMenuItem(label);
            item.Image = BuildToolIcon(selected, tool == selected ? color : Color.FromArgb(210, 235, 244));
            item.Padding = new Padding(4, 2, 10, 2);
            item.ToolTipText = ToolDescription(selected);
            item.Click += delegate { SetTool(selected); };
            menu.Items.Add(item);
        }

        private Bitmap BuildColorIcon(Color selected)
        {
            Bitmap icon = new Bitmap(14, 14);
            using (Graphics g = Graphics.FromImage(icon))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                using (SolidBrush brush = new SolidBrush(selected))
                using (Pen ring = new Pen(Color.FromArgb(220, 255, 255, 255), 1))
                {
                    g.FillEllipse(brush, new Rectangle(2, 2, 10, 10));
                    g.DrawEllipse(ring, new Rectangle(2, 2, 10, 10));
                }
            }
            return icon;
        }

        private Bitmap BuildToolIcon(Tool selected, Color iconColor)
        {
            Bitmap icon = new Bitmap(14, 14);
            using (Graphics g = Graphics.FromImage(icon))
            using (Pen pen = new Pen(iconColor, 2))
            using (SolidBrush brush = new SolidBrush(iconColor))
            using (Font font = new Font("Segoe UI", 7, FontStyle.Bold))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                DrawingStyle.ConfigureLineCap(pen, selected);

                if (selected == Tool.Move)
                {
                    g.DrawLine(pen, 3, 7, 11, 7);
                    g.DrawLine(pen, 7, 3, 7, 11);
                    g.FillPolygon(brush, new[] { new Point(3, 7), new Point(5, 5), new Point(5, 9) });
                    g.FillPolygon(brush, new[] { new Point(11, 7), new Point(9, 5), new Point(9, 9) });
                }
                else if (selected == Tool.Arrow)
                    g.DrawLine(pen, 2, 11, 11, 3);
                else if (selected == Tool.Rect)
                    g.DrawRectangle(pen, new Rectangle(2, 3, 10, 8));
                else if (selected == Tool.Line)
                    g.DrawLine(pen, 2, 11, 12, 3);
                else if (selected == Tool.Pencil)
                    g.DrawLine(pen, 3, 11, 11, 3);
                else if (selected == Tool.Highlight)
                {
                    using (Pen wide = new Pen(Color.FromArgb(160, Color.Yellow), 5))
                        g.DrawLine(wide, 2, 9, 12, 5);
                    g.DrawLine(pen, 2, 9, 12, 5);
                }
                else if (selected == Tool.Pixelate)
                {
                    g.FillRectangle(brush, 2, 2, 4, 4);
                    g.FillRectangle(brush, 8, 2, 4, 4);
                    g.FillRectangle(brush, 2, 8, 4, 4);
                    g.FillRectangle(brush, 8, 8, 4, 4);
                }
                else if (selected == Tool.Number)
                    g.DrawString("1", font, brush, new PointF(4, 1));
                else if (selected == Tool.Text)
                    g.DrawString("T", font, brush, new PointF(3, 1));
            }
            return icon;
        }

        private string ToolDescription(Tool selected)
        {
            if (selected == Tool.Move) return "Mover cualquier anotacion agregada.";
            if (selected == Tool.Arrow) return "Dibujar una flecha de senalizacion.";
            if (selected == Tool.Rect) return "Marcar una zona con un rectangulo.";
            if (selected == Tool.Line) return "Dibujar una linea recta.";
            if (selected == Tool.Pencil) return "Dibujar libremente sobre la captura.";
            if (selected == Tool.Highlight) return "Resaltar informacion sin taparla.";
            if (selected == Tool.Pixelate) return "Ocultar informacion sensible con pixelado.";
            if (selected == Tool.Number) return "Agregar marcadores numerados.";
            if (selected == Tool.Text) return "Agregar texto sobre la imagen.";
            return "Seleccionar herramienta.";
        }

        private void CycleColor()
        {
            Color[] colors = new[]
            {
                Color.FromArgb(0, 255, 210),
                Color.FromArgb(255, 59, 48),
                Color.FromArgb(255, 204, 0),
                Color.FromArgb(52, 199, 89),
                Color.FromArgb(32, 196, 244),
            };
            int index = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].ToArgb() == color.ToArgb())
                {
                    index = i;
                    break;
                }
            }
            color = colors[(index + 1) % colors.Length];
            ShowToolbars();
            Invalidate();
        }

        private void Thinner()
        {
            drawWidth = Math.Max(2, drawWidth - 1);
        }

        private void Thicker()
        {
            drawWidth = Math.Min(12, drawWidth + 1);
        }

        private void DrawOpOnOverlay(Graphics g, DrawOp op)
        {
            Color opColor = op.Tool == Tool.Highlight ? Color.FromArgb(170, Color.Yellow) : op.Color;
            int width = op.Tool == Tool.Highlight ? Math.Max(10, op.Width * 4) : op.Width;
            using (Pen pen = new Pen(opColor, width))
            {
                DrawingStyle.ConfigureLineCap(pen, op.Tool);
                if (op.Tool == Tool.Arrow)
                    g.DrawLine(pen, op.A, op.B);
                else if (op.Tool == Tool.Line)
                    g.DrawLine(pen, op.A, op.B);
                else if (op.Tool == Tool.Rect)
                    g.DrawRectangle(pen, Normalize(op.A, op.B));
                else if (op.Tool == Tool.Pencil || op.Tool == Tool.Highlight)
                {
                    if (op.Points != null && op.Points.Count > 1)
                        g.DrawLines(pen, op.Points.ToArray());
                }
                else if (op.Tool == Tool.Text)
                {
                    using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                    using (SolidBrush brush = new SolidBrush(op.Color))
                        g.DrawString(op.Text ?? "", font, brush, op.A);
                }
                else if (op.Tool == Tool.Number)
                {
                    int size = Math.Max(24, op.Width);
                    Rectangle circle = new Rectangle(op.A.X - size / 2, op.A.Y - size / 2, size, size);
                    using (SolidBrush fill = new SolidBrush(op.Color))
                    using (Font font = new Font("Segoe UI", Math.Max(10, size / 2), FontStyle.Bold))
                    using (StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        g.FillEllipse(fill, circle);
                        g.DrawString(op.Text ?? "", font, Brushes.White, circle, format);
                    }
                }
                else if (op.Tool == Tool.Pixelate)
                    DrawPixelated(g, Normalize(op.A, op.B));
            }
        }

        private void DrawPixelated(Graphics g, Rectangle rect)
        {
            rect.Intersect(selection);
            if (rect.Width < 4 || rect.Height < 4)
                return;
            using (Bitmap crop = screenshot.Clone(rect, screenshot.PixelFormat))
            using (Bitmap small = new Bitmap(crop, Math.Max(1, crop.Width / 12), Math.Max(1, crop.Height / 12)))
            using (Bitmap big = new Bitmap(small, rect.Width, rect.Height))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(big, rect);
            }
        }

        private Bitmap RenderCrop()
        {
            Bitmap crop = new Bitmap(selection.Width, selection.Height);
            using (Graphics g = Graphics.FromImage(crop))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(screenshot, new Rectangle(0, 0, crop.Width, crop.Height), selection, GraphicsUnit.Pixel);
                g.TranslateTransform(-selection.Left, -selection.Top);
                foreach (DrawOp op in ops)
                    DrawOpOnOverlay(g, op);
                g.ResetTransform();
            }
            return crop;
        }

        private void CopyAndClose()
        {
            if (!HasSelection())
                return;
            CommitTextEdit();
            using (Bitmap result = RenderCrop())
            {
                Directory.CreateDirectory(Paths.HistoryDir);
                result.Save(Path.Combine(Paths.HistoryDir, "Zaetta_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png"));
                ClipboardHelper.SetImageWithRetry((Bitmap)result.Clone());
            }
            Close();
        }

        private void SaveImage()
        {
            if (!HasSelection())
                return;
            CommitTextEdit();
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG (*.png)|*.png";
                dialog.FileName = "Zaetta_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap result = RenderCrop())
                        result.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void Undo()
        {
            if (ops.Count == 0)
                return;
            ops.RemoveAt(ops.Count - 1);
            Invalidate();
        }

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
                    BeginRightCopy();
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
            using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
            {
                if (op.Tool == Tool.Text)
                {
                    if (string.IsNullOrWhiteSpace(op.Text))
                        return new Rectangle(op.A.X, op.A.Y, 1, 1);
                    SizeF size = g.MeasureString(op.Text, font);
                    return new Rectangle(op.A.X, op.A.Y, Math.Max(1, (int)Math.Ceiling(size.Width)), Math.Max(1, (int)Math.Ceiling(size.Height)));
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
            return op != null && (op.Tool == Tool.Arrow || op.Tool == Tool.Line || op.Tool == Tool.Rect);
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

        private void DrawSelectedOpHandles(Graphics g)
        {
            if (!CanResizeOp(selectedOp))
                return;

            Rectangle bounds = GetOpBounds(selectedOp);
            bounds.Inflate(6, 6);
            using (Pen outline = new Pen(Color.FromArgb(190, 255, 255, 255), 1))
            {
                outline.DashPattern = new float[] { 3, 3 };
                g.DrawRectangle(outline, bounds);
            }

            Rectangle[] handles = GetResizeHandleRects(selectedOp);
            using (SolidBrush fill = new SolidBrush(Color.FromArgb(248, 248, 248)))
            using (Pen border = new Pen(Color.FromArgb(50, 50, 50), 1))
            {
                foreach (Rectangle handle in handles)
                {
                    g.FillRectangle(fill, handle);
                    g.DrawRectangle(border, handle);
                }
            }
        }

        private void DrawSizeLabel(Graphics g, Rectangle box)
        {
            string label = box.Width + " x " + box.Height;
            using (Font font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (SolidBrush bg = new SolidBrush(SelectionLabelBg))
            using (SolidBrush fg = new SolidBrush(Color.White))
            {
                SizeF size = g.MeasureString(label, font);
                RectangleF labelRect = new RectangleF(box.Left, Math.Max(0, box.Top - 27), size.Width + 18, 22);
                g.FillRectangle(bg, labelRect);
                g.DrawString(label, font, fg, labelRect.Left + 8, labelRect.Top + 3);
            }
        }

        private void DrawSelectionBorder(Graphics g, Rectangle box)
        {
            Rectangle inset = new Rectangle(box.Left, box.Top, Math.Max(1, box.Width - 1), Math.Max(1, box.Height - 1));
            using (Pen shadow = new Pen(SelectionShadow, 2))
            using (Pen light = new Pen(SelectionStroke, 1))
            using (Pen dash = new Pen(SelectionDash, 1))
            {
                shadow.Alignment = PenAlignment.Inset;
                light.Alignment = PenAlignment.Inset;
                dash.Alignment = PenAlignment.Inset;
                dash.DashPattern = new float[] { 2, 2 };
                g.DrawRectangle(shadow, inset);
                g.DrawRectangle(light, inset);
                g.DrawRectangle(dash, inset);
            }
        }

        private void DrawHandles(Graphics g, Rectangle box)
        {
            Point[] handles = new[]
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
            using (SolidBrush fill = new SolidBrush(SelectionHandle))
            using (Pen border = new Pen(SelectionHandleBorder, 2))
            {
                foreach (Point handle in handles)
                {
                    Rectangle dot = new Rectangle(handle.X - 3, handle.Y - 3, 7, 7);
                    g.FillRectangle(fill, dot);
                    g.DrawRectangle(border, dot);
                }
            }
        }

        private void ShowAbout()
        {
            MessageBox.Show("Zaetta Capture\n\nVersion 1.0\n\nDesarrollador:\nVictor Alexis Alzate Cortes", "Acerca de Zaetta Capture");
        }

        private bool HasSelection()
        {
            return selection.Width > 0 && selection.Height > 0;
        }

        private Point ClampToSelection(Point p)
        {
            return new Point(
                Math.Max(selection.Left, Math.Min(selection.Right, p.X)),
                Math.Max(selection.Top, Math.Min(selection.Bottom, p.Y))
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dimmedScreenshot.Dispose();
                screenshot.Dispose();
                HideToolbars();
            }
            base.Dispose(disposing);
        }

        private static Bitmap BuildDimmedScreenshot(Bitmap source)
        {
            Bitmap dimmed = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(dimmed))
            {
                g.DrawImageUnscaled(source, Point.Empty);
                using (SolidBrush shade = new SolidBrush(Color.FromArgb(112, 0, 0, 0)))
                    g.FillRectangle(shade, new Rectangle(Point.Empty, dimmed.Size));
            }
            return dimmed;
        }

        private static Rectangle Normalize(Point a, Point b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
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

    internal enum Tool
    {
        Move,
        Arrow,
        Rect,
        Line,
        Pencil,
        Highlight,
        Text,
        Pixelate,
        Number
    }

    internal static class ToolShortcuts
    {
        public static bool TryGet(Keys key, bool includeFullOverlayTools, out Tool tool)
        {
            tool = Tool.Arrow;
            switch (key)
            {
                case Keys.P:
                case Keys.M:
                    tool = Tool.Move;
                    return includeFullOverlayTools;
                case Keys.F:
                case Keys.A:
                    tool = Tool.Arrow;
                    return true;
                case Keys.R:
                    tool = Tool.Rect;
                    return true;
                case Keys.L:
                    tool = Tool.Line;
                    return includeFullOverlayTools;
                case Keys.D:
                    tool = Tool.Pencil;
                    return includeFullOverlayTools;
                case Keys.H:
                    tool = Tool.Highlight;
                    return includeFullOverlayTools;
                case Keys.T:
                    tool = Tool.Text;
                    return true;
                case Keys.X:
                    tool = Tool.Pixelate;
                    return true;
                case Keys.N:
                    tool = Tool.Number;
                    return includeFullOverlayTools;
                default:
                    return false;
            }
        }
    }

    internal sealed class DrawOp
    {
        public Tool Tool;
        public Point A;
        public Point B;
        public string Text;
        public Color Color;
        public int Width;
        public List<Point> Points;
    }

    internal sealed class EditorForm : Form
    {
        private const int WM_CONTEXTMENU = 0x007B;
        private readonly Bitmap original;
        private readonly List<DrawOp> ops = new List<DrawOp>();
        private Tool tool = Tool.Arrow;
        private Color color = Color.FromArgb(255, 59, 48);
        private Point start;
        private Point current;
        private bool drawing;
        private bool pendingRightCopy;
        private Panel toolbar;
        private readonly ToolTip tips = new ToolTip();

        public EditorForm(Bitmap image)
        {
            original = image;
            Text = "Zaetta Capture";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(Math.Min(image.Width + 36, Screen.PrimaryScreen.WorkingArea.Width - 80), Math.Min(image.Height + 100, Screen.PrimaryScreen.WorkingArea.Height - 80));
            MinimumSize = new Size(520, 360);
            BackColor = Ui.Bg;
            DoubleBuffered = true;
            KeyPreview = true;
            ContextMenuStrip = ContextMenus.Suppressed();
            ConfigureTips(tips);
            BuildToolbar();
        }

        private void BuildToolbar()
        {
            toolbar = new Panel();
            toolbar.Height = 64;
            toolbar.Dock = DockStyle.Top;
            toolbar.BackColor = Ui.Bg;
            toolbar.Padding = new Padding(10, 10, 10, 10);
            Controls.Add(toolbar);

            Label brand = new Label();
            brand.Text = "Z";
            brand.Left = 14;
            brand.Top = 16;
            brand.Width = 28;
            brand.Height = 30;
            brand.ForeColor = Ui.Accent2;
            brand.BackColor = Ui.Bg;
            brand.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            brand.TextAlign = ContentAlignment.MiddleCenter;
            toolbar.Controls.Add(brand);

            Label title = new Label();
            title.Text = "Zaetta Capture";
            title.Left = 48;
            title.Top = 13;
            title.Width = 150;
            title.Height = 20;
            title.ForeColor = Ui.Text;
            title.BackColor = Ui.Bg;
            title.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            toolbar.Controls.Add(title);

            Label hint = new Label();
            hint.Text = "Ctrl+C copia | Esc cancela";
            hint.Left = 48;
            hint.Top = 34;
            hint.Width = 180;
            hint.Height = 18;
            hint.ForeColor = Ui.Muted;
            hint.BackColor = Ui.Bg;
            hint.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            toolbar.Controls.Add(hint);

            AddButton("Flecha", 250, delegate { tool = Tool.Arrow; }, false, "Dibujar flechas de senalizacion.");
            AddButton("Marco", 340, delegate { tool = Tool.Rect; }, false, "Dibujar marcos para resaltar zonas.");
            AddButton("Texto", 430, delegate { tool = Tool.Text; }, false, "Agregar texto sobre la imagen.");
            AddButton("Pixelar", 520, delegate { tool = Tool.Pixelate; }, false, "Pixelar informacion sensible.");
            AddButton("Deshacer", 624, delegate { Undo(); }, false, "Deshacer el ultimo cambio.");
            AddButton("Copiar", 740, delegate { CopyAndClose(); }, true, "Copiar al portapapeles y cerrar.");
            AddButton("Guardar", 840, delegate { SaveImage(); }, false, "Guardar la imagen como PNG.");
        }

        private void AddButton(string text, int x, EventHandler action, bool primary, string tooltip)
        {
            ZaettaButton button = new ZaettaButton(text, primary);
            button.Left = x;
            button.Top = 14;
            button.Width = primary ? 92 : 80;
            button.Height = 36;
            button.Click += action;
            tips.SetToolTip(button, tooltip);
            toolbar.Controls.Add(button);
        }

        private static void ConfigureTips(ToolTip tooltip)
        {
            tooltip.InitialDelay = 350;
            tooltip.ReshowDelay = 100;
            tooltip.AutoPopDelay = 6500;
            tooltip.ShowAlways = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.SuppressKeyPress = true;
                return;
            }
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Undo();
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
            if (!e.Control && !e.Alt && !e.Shift && TryApplyToolShortcut(e.KeyCode))
            {
                e.SuppressKeyPress = true;
                return;
            }
            base.OnKeyDown(e);
        }

        private bool TryApplyToolShortcut(Keys key)
        {
            Tool selected;
            if (!ToolShortcuts.TryGet(key, false, out selected))
                return false;

            tool = selected;
            Cursor = tool == Tool.Text ? Cursors.IBeam : Cursors.Cross;
            Invalidate();
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CONTEXTMENU)
                return;
            base.WndProc(ref m);
        }

        private void BeginRightCopy()
        {
            pendingRightCopy = true;
            Capture = true;
        }

        private void FinishRightCopy()
        {
            if (!pendingRightCopy)
                return;
            pendingRightCopy = false;
            Capture = false;
            CopyAndClose();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                BeginRightCopy();
                return;
            }
            if (e.Y < toolbar.Bottom)
                return;
            if (tool == Tool.Text)
            {
                string value = PromptText();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    ops.Add(new DrawOp { Tool = Tool.Text, A = ImagePoint(e.Location), Text = value.Trim(), Color = color, Width = 18 });
                    Invalidate();
                }
                return;
            }

            drawing = true;
            start = ImagePoint(e.Location);
            current = start;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!drawing)
                return;
            current = ImagePoint(e.Location);
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && pendingRightCopy)
            {
                FinishRightCopy();
                return;
            }
            if (!drawing)
                return;
            drawing = false;
            current = ImagePoint(e.Location);
            ops.Add(new DrawOp { Tool = tool, A = start, B = current, Color = color, Width = 4 });
            Invalidate();
        }

        private void Undo()
        {
            if (ops.Count == 0)
                return;
            ops.RemoveAt(ops.Count - 1);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Rectangle imageRect = ImageRect();
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            using (SolidBrush panel = new SolidBrush(Ui.Panel))
            {
                e.Graphics.FillRectangle(panel, new Rectangle(10, toolbar.Bottom + 10, ClientSize.Width - 20, ClientSize.Height - toolbar.Bottom - 20));
            }
            e.Graphics.DrawImage(original, imageRect);
            e.Graphics.SetClip(imageRect);
            foreach (DrawOp op in ops)
            {
                DrawOperation(e.Graphics, op, imageRect);
            }
            if (drawing)
            {
                DrawOperation(e.Graphics, new DrawOp { Tool = tool, A = start, B = current, Color = color, Width = 4 }, imageRect);
            }
            e.Graphics.ResetClip();
            using (Pen border = new Pen(Color.FromArgb(190, 210, 222)))
            {
                e.Graphics.DrawRectangle(border, imageRect);
            }
        }

        private void DrawOperation(Graphics g, DrawOp op, Rectangle imageRect)
        {
            Point a = ToScreenPoint(op.A, imageRect);
            Point b = ToScreenPoint(op.B, imageRect);
            using (Pen pen = new Pen(op.Color, op.Width))
            {
                DrawingStyle.ConfigureLineCap(pen, op.Tool);
                if (op.Tool == Tool.Arrow)
                    g.DrawLine(pen, a, b);
                else if (op.Tool == Tool.Rect)
                    g.DrawRectangle(pen, Normalize(a, b));
                else if (op.Tool == Tool.Text)
                {
                    using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                    using (SolidBrush brush = new SolidBrush(op.Color))
                    {
                        g.DrawString(op.Text ?? "", font, brush, a);
                    }
                }
                else if (op.Tool == Tool.Pixelate)
                {
                    PixelateOnScreen(g, Normalize(a, b), op);
                }
            }
        }

        private void PixelateOnScreen(Graphics g, Rectangle rect, DrawOp op)
        {
            if (rect.Width < 4 || rect.Height < 4)
                return;
            Rectangle source = Normalize(op.A, op.B);
            source.Intersect(new Rectangle(0, 0, original.Width, original.Height));
            if (source.Width < 4 || source.Height < 4)
                return;

            using (Bitmap crop = original.Clone(source, original.PixelFormat))
            using (Bitmap small = new Bitmap(crop, Math.Max(1, crop.Width / 12), Math.Max(1, crop.Height / 12)))
            using (Bitmap big = new Bitmap(small, rect.Width, rect.Height))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(big, rect);
            }
        }

        private Bitmap RenderImage()
        {
            Bitmap result = new Bitmap(original);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle imageRect = new Rectangle(0, 0, original.Width, original.Height);
                foreach (DrawOp op in ops)
                {
                    DrawOperationOnBitmap(g, op, imageRect);
                }
            }
            return result;
        }

        private void DrawOperationOnBitmap(Graphics g, DrawOp op, Rectangle imageRect)
        {
            using (Pen pen = new Pen(op.Color, op.Width))
            {
                DrawingStyle.ConfigureLineCap(pen, op.Tool);
                if (op.Tool == Tool.Arrow)
                    g.DrawLine(pen, op.A, op.B);
                else if (op.Tool == Tool.Rect)
                    g.DrawRectangle(pen, Normalize(op.A, op.B));
                else if (op.Tool == Tool.Text)
                {
                    using (Font font = new Font("Segoe UI", 16, FontStyle.Bold))
                    using (SolidBrush brush = new SolidBrush(op.Color))
                        g.DrawString(op.Text ?? "", font, brush, op.A);
                }
                else if (op.Tool == Tool.Pixelate)
                {
                    Rectangle rect = Normalize(op.A, op.B);
                    rect.Intersect(imageRect);
                    if (rect.Width >= 4 && rect.Height >= 4)
                    {
                        using (Bitmap crop = resultClone(original, rect))
                        using (Bitmap small = new Bitmap(crop, Math.Max(1, rect.Width / 12), Math.Max(1, rect.Height / 12)))
                        using (Bitmap big = new Bitmap(small, rect.Width, rect.Height))
                        {
                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g.DrawImage(big, rect);
                        }
                    }
                }
            }
        }

        private static Bitmap resultClone(Bitmap source, Rectangle rect)
        {
            return source.Clone(rect, source.PixelFormat);
        }

        private void CopyAndClose()
        {
            using (Bitmap result = RenderImage())
            {
                SaveToHistory(result);
                ClipboardHelper.SetImageWithRetry((Bitmap)result.Clone());
            }
            Close();
        }

        private void SaveToHistory(Bitmap result)
        {
            Directory.CreateDirectory(Paths.HistoryDir);
            string file = Path.Combine(Paths.HistoryDir, "Zaetta_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            result.Save(file, System.Drawing.Imaging.ImageFormat.Png);
        }

        private void SaveImage()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "PNG (*.png)|*.png";
                dialog.FileName = "Zaetta_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap result = RenderImage())
                    {
                        result.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            }
        }

        private Rectangle ImageRect()
        {
            int top = toolbar.Bottom + 22;
            int width = ClientSize.Width - 44;
            int height = ClientSize.Height - top - 24;
            float ratio = Math.Min(width / (float)original.Width, height / (float)original.Height);
            int imageW = Math.Max(1, (int)(original.Width * ratio));
            int imageH = Math.Max(1, (int)(original.Height * ratio));
            return new Rectangle((ClientSize.Width - imageW) / 2, top + (height - imageH) / 2, imageW, imageH);
        }

        private Point ImagePoint(Point screenPoint)
        {
            Rectangle rect = ImageRect();
            float scaleX = original.Width / (float)rect.Width;
            float scaleY = original.Height / (float)rect.Height;
            int x = Math.Max(0, Math.Min(original.Width - 1, (int)((screenPoint.X - rect.Left) * scaleX)));
            int y = Math.Max(0, Math.Min(original.Height - 1, (int)((screenPoint.Y - rect.Top) * scaleY)));
            return new Point(x, y);
        }

        private Point ToScreenPoint(Point imagePoint, Rectangle imageRect)
        {
            float scaleX = imageRect.Width / (float)original.Width;
            float scaleY = imageRect.Height / (float)original.Height;
            return new Point(imageRect.Left + (int)(imagePoint.X * scaleX), imageRect.Top + (int)(imagePoint.Y * scaleY));
        }

        private static Rectangle Normalize(Point a, Point b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
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
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                original.Dispose();
            base.Dispose(disposing);
        }
    }
}
