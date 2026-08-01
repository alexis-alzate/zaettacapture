using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ZaettaCaptureInstaller
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length > 0 && string.Equals(args[0], "/uninstall", StringComparison.OrdinalIgnoreCase))
            {
                InstallerForm.RunUninstallFromTemp();
                return;
            }
            if (args.Length > 0 && string.Equals(args[0], "/uninstall-temp", StringComparison.OrdinalIgnoreCase))
            {
                InstallerForm.Uninstall(true);
                return;
            }
            Application.Run(new InstallerForm());
        }
    }

    internal sealed class InstallerForm : Form
    {
        private readonly SmoothProgress progress;
        private readonly Label status;
        private readonly FlatButton installButton;
        private readonly Label detail;
        private readonly Image backgroundGlow;
        private bool completed;

        private const string AppName = "Zaetta Capture";
        private const string ResourceName = "ZaettaApp";
        private const string LogoResourceName = "ZaettaLogo";
        private const string Publisher = "Victor Alexis Alzate Cortes";
        private const string Version = "1.0.5";

        [DllImport("shell32.dll")]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        public InstallerForm()
        {
            Text = "Instalador - Zaetta Capture";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ClientSize = new Size(560, 330);
            BackColor = Color.FromArgb(7, 14, 21);
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            backgroundGlow = LoadLogoGlowImage(260);

            Label title = new Label();
            title.Text = "aetta Capture";
            title.ForeColor = Color.White;
            title.BackColor = BackColor;
            title.Font = new Font("Segoe UI", 25, FontStyle.Bold);
            title.SetBounds(138, 52, 374, 42);
            Controls.Add(title);

            PictureBox titleLogo = new PictureBox();
            titleLogo.BackColor = BackColor;
            titleLogo.Image = LoadLogoMarkImage(64);
            titleLogo.SizeMode = PictureBoxSizeMode.Zoom;
            titleLogo.SetBounds(72, 44, 64, 64);
            Controls.Add(titleLogo);

            Label subtitle = new Label();
            subtitle.Text = "Instalador local, rapido y limpio.";
            subtitle.ForeColor = Color.FromArgb(165, 184, 199);
            subtitle.BackColor = BackColor;
            subtitle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            subtitle.SetBounds(140, 99, 360, 24);
            Controls.Add(subtitle);

            Panel card = new Panel();
            card.BackColor = Color.FromArgb(14, 29, 40);
            card.SetBounds(28, 148, 504, 92);
            Controls.Add(card);

            status = new Label();
            status.Text = "Listo para instalar";
            status.ForeColor = Color.White;
            status.BackColor = card.BackColor;
            status.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            status.SetBounds(18, 14, 460, 26);
            card.Controls.Add(status);

            detail = new Label();
            detail.Text = "Se instalara la aplicacion, accesos directos e inicio con Windows.";
            detail.ForeColor = Color.FromArgb(165, 184, 199);
            detail.BackColor = card.BackColor;
            detail.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            detail.SetBounds(18, 40, 460, 20);
            card.Controls.Add(detail);

            progress = new SmoothProgress();
            progress.SetBounds(18, 66, 468, 12);
            card.Controls.Add(progress);

            installButton = new FlatButton("Instalar", true);
            installButton.SetBounds(370, 268, 150, 36);
            installButton.Click += delegate
            {
                if (completed)
                    Close();
                else
                    Install();
            };
            Controls.Add(installButton);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            if (backgroundGlow == null)
                return;

            using (ImageAttributes opacity = new ImageAttributes())
            {
                ColorMatrix matrix = new ColorMatrix();
                matrix.Matrix33 = 0.07f;
                opacity.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                Rectangle glowRect = new Rectangle(336, -54, 250, 250);
                e.Graphics.DrawImage(backgroundGlow, glowRect, 0, 0, backgroundGlow.Width, backgroundGlow.Height, GraphicsUnit.Pixel, opacity);
            }
        }

        private static Image LoadLogoMarkImage(int size)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(LogoResourceName))
            {
                if (stream == null)
                    return Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap();
                using (Image image = Image.FromStream(stream))
                    return BuildLogoMark(image, size);
            }
        }

        private static Image LoadLogoGlowImage(int size)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(LogoResourceName))
            {
                if (stream == null)
                    return Icon.ExtractAssociatedIcon(Application.ExecutablePath).ToBitmap();
                using (Image image = Image.FromStream(stream))
                    return BuildLogoMark(image, size);
            }
        }

        private static Bitmap BuildLogoMark(Image source, int size)
        {
            Bitmap result = new Bitmap(size, size);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                float cropSize = Math.Min(source.Width, source.Height) * 0.64f;
                RectangleF sourceRect = new RectangleF(
                    source.Width * 0.18f,
                    source.Height * 0.16f,
                    cropSize,
                    cropSize
                );
                RectangleF targetRect = new RectangleF(0, 0, size, size);
                g.DrawImage(source, targetRect, sourceRect, GraphicsUnit.Pixel);
            }
            return result;
        }

        private void Install()
        {
            installButton.Enabled = false;
            try
            {
                SetProgress(10, "Preparando instalacion...", "Validando carpeta local de instalacion.");
                CleanupLegacyInstallations();
                string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
                if (Directory.Exists(installDir))
                    Directory.Delete(installDir, true);
                Directory.CreateDirectory(installDir);

                SetProgress(40, "Copiando aplicacion...", "Instalando Zaetta Capture en AppData.");
                string appPath = Path.Combine(installDir, AppName + ".exe");
                ExtractResource(ResourceName, appPath);
                string installerPath = Path.Combine(installDir, "Zaetta Capture Installer.exe");
                File.Copy(Application.ExecutablePath, installerPath, true);

                SetProgress(72, "Registrando aplicacion...", "Creando accesos directos y registro de Windows.");
                CreateShortcut(appPath, installDir);
                RegisterAppPath(appPath);
                RegisterStartup(appPath);
                RegisterUninstallEntry(installDir, appPath, installerPath);
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

                SetProgress(90, "Iniciando Zaetta Capture...", "Abriendo la aplicacion en la bandeja del sistema.");
                LaunchInstalledApp(appPath, installDir);

                SetProgress(100, "Instalacion completada", "Zaetta Capture quedo activo en la bandeja del sistema.");
                installButton.Text = "Finalizar";
                installButton.Enabled = true;
                completed = true;
            }
            catch (Exception ex)
            {
                status.Text = "La instalacion fallo";
                detail.Text = ex.Message;
                installButton.Text = "Reintentar";
                installButton.Enabled = true;
            }
        }

        private void SetProgress(int value, string text, string detailText)
        {
            progress.Value = value;
            status.Text = text;
            detail.Text = detailText;
            progress.Invalidate();
            Application.DoEvents();
            System.Threading.Thread.Sleep(180);
        }

        private static void ExtractResource(string resourceName, string targetPath)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream input = asm.GetManifestResourceStream(resourceName))
            {
                if (input == null)
                    throw new InvalidOperationException("No se encontro el recurso interno de la aplicacion.");
                using (FileStream output = File.Create(targetPath))
                    input.CopyTo(output);
            }
        }

        private static void CleanupLegacyInstallations()
        {
            StopRunningZaetta();
            RemoveLegacyUninstallEntries();
            RemoveLegacyAppPaths();
            RemoveStartupEntries();

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string[] names =
            {
                "Zaetta Capture",
                "Zaetta Capture Final",
                "Zaetta Capture Native"
            };

            foreach (string name in names)
            {
                string shortcut = Path.Combine(desktop, name + ".lnk");
                if (File.Exists(shortcut))
                    File.Delete(shortcut);

                string startShortcut = Path.Combine(programs, name + ".lnk");
                if (File.Exists(startShortcut))
                    File.Delete(startShortcut);

                string commandShortcut = Path.Combine(desktop, name + ".cmd");
                if (File.Exists(commandShortcut))
                    File.Delete(commandShortcut);

                string startCommandShortcut = Path.Combine(programs, name + ".cmd");
                if (File.Exists(startCommandShortcut))
                    File.Delete(startCommandShortcut);

                string dir = Path.Combine(localAppData, name);
                if (Directory.Exists(dir))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static void RunUninstallFromTemp()
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "ZaettaCaptureUninstall_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                Directory.CreateDirectory(tempDir);
                string tempInstaller = Path.Combine(tempDir, "Zaetta Capture Uninstaller.exe");
                File.Copy(Application.ExecutablePath, tempInstaller, true);
                ProcessStartInfo info = new ProcessStartInfo(tempInstaller, "/uninstall-temp");
                info.UseShellExecute = true;
                Process.Start(info);
            }
            catch
            {
                Uninstall(true);
            }
        }

        public static void Uninstall(bool showMessage)
        {
            try
            {
                StopRunningZaetta();
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string[] names =
                {
                    "Zaetta Capture",
                    "Zaetta Capture Final",
                    "Zaetta Capture Native"
                };

                foreach (string name in names)
                {
                    string shortcut = Path.Combine(desktop, name + ".lnk");
                    if (File.Exists(shortcut))
                        File.Delete(shortcut);

                    string startShortcut = Path.Combine(programs, name + ".lnk");
                    if (File.Exists(startShortcut))
                        File.Delete(startShortcut);

                    string commandShortcut = Path.Combine(desktop, name + ".cmd");
                    if (File.Exists(commandShortcut))
                        File.Delete(commandShortcut);

                    string startCommandShortcut = Path.Combine(programs, name + ".cmd");
                    if (File.Exists(startCommandShortcut))
                        File.Delete(startCommandShortcut);

                    string dir = Path.Combine(localAppData, name);
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                }

                RemoveLegacyUninstallEntries();
                RemoveLegacyAppPaths();
                RemoveStartupEntries();
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

                if (showMessage)
                    MessageBox.Show("Zaetta Capture fue desinstalado correctamente.", "Zaetta Capture", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (showMessage)
                    MessageBox.Show("No se pudo completar la desinstalacion: " + ex.Message, "Zaetta Capture", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static void StopRunningZaetta()
        {
            string[] processNames =
            {
                "Zaetta Capture",
                "Zaetta Capture Final",
                "Zaetta Capture Native"
            };

            foreach (string processName in processNames)
            {
                foreach (Process process in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        if (process.Id == Process.GetCurrentProcess().Id)
                            continue;
                        process.Kill();
                        process.WaitForExit(2500);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void CreateShortcut(string appPath, string workingDir)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            Directory.CreateDirectory(programs);
            string[] shortcutPaths =
            {
                Path.Combine(desktop, AppName + ".lnk"),
                Path.Combine(programs, AppName + ".lnk")
            };
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                object shell = Activator.CreateInstance(shellType);
                foreach (string shortcutPath in shortcutPaths)
                {
                    object shortcut = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                    Type shortcutType = shortcut.GetType();
                    shortcutType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, shortcut, new object[] { appPath });
                    shortcutType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, shortcut, new object[] { workingDir });
                    shortcutType.InvokeMember("Description", BindingFlags.SetProperty, null, shortcut, new object[] { AppName });
                    shortcutType.InvokeMember("IconLocation", BindingFlags.SetProperty, null, shortcut, new object[] { appPath });
                    shortcutType.InvokeMember("Save", BindingFlags.InvokeMethod, null, shortcut, null);
                    Marshal.ReleaseComObject(shortcut);
                }
                Marshal.ReleaseComObject(shell);
            }
            catch
            {
                throw new InvalidOperationException("No se pudo crear el acceso directo de Windows.");
            }
        }

        private static void RegisterAppPath(string appPath)
        {
            string appPathKey = @"Software\Microsoft\Windows\CurrentVersion\App Paths\" + AppName + ".exe";
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(appPathKey))
            {
                key.SetValue("", appPath);
                key.SetValue("Path", Path.GetDirectoryName(appPath));
            }
        }

        private static void RegisterStartup(string appPath)
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                key.SetValue(AppName, "\"" + appPath + "\"");
            }
        }

        private static void LaunchInstalledApp(string appPath, string workingDir)
        {
            ProcessStartInfo info = new ProcessStartInfo(appPath);
            info.WorkingDirectory = workingDir;
            info.UseShellExecute = true;
            Process.Start(info);
        }

        private static void RemoveStartupEntries()
        {
            string[] names =
            {
                "Zaetta Capture",
                "Zaetta Capture Final",
                "Zaetta Capture Native"
            };

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                if (key == null)
                    return;

                foreach (string name in names)
                    key.DeleteValue(name, false);
            }
        }

        private static void RegisterUninstallEntry(string installDir, string appPath, string installerPath)
        {
            string uninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + AppName;
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(uninstallKey))
            {
                key.SetValue("DisplayName", AppName);
                key.SetValue("DisplayVersion", Version);
                key.SetValue("Publisher", Publisher);
                key.SetValue("InstallLocation", installDir);
                key.SetValue("DisplayIcon", appPath);
                key.SetValue("UninstallString", "\"" + installerPath + "\" /uninstall");
                key.SetValue("QuietUninstallString", "\"" + installerPath + "\" /uninstall");
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                key.SetValue("EstimatedSize", 2 * 1024, RegistryValueKind.DWord);
            }
        }

        private static void RemoveLegacyUninstallEntries()
        {
            string baseKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
            string[] names =
            {
                "Zaetta Capture",
                "Zaetta Capture Final",
                "Zaetta Capture Native"
            };

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(baseKey, true))
            {
                if (key == null)
                    return;
                foreach (string name in names)
                {
                    try
                    {
                        key.DeleteSubKeyTree(name, false);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void RemoveLegacyAppPaths()
        {
            string baseKey = @"Software\Microsoft\Windows\CurrentVersion\App Paths";
            string[] names =
            {
                "Zaetta Capture.exe",
                "Zaetta Capture Final.exe",
                "Zaetta Capture Native.exe"
            };

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(baseKey, true))
            {
                if (key == null)
                    return;
                foreach (string name in names)
                {
                    try
                    {
                        key.DeleteSubKeyTree(name, false);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }

    internal sealed class SmoothProgress : Control
    {
        public int Value { get; set; }

        public SmoothProgress()
        {
            Value = 0;
            BackColor = Color.FromArgb(7, 14, 21);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath bgPath = Round(rect, Height / 2))
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(6, 13, 19)))
                e.Graphics.FillPath(bg, bgPath);

            int fillWidth = Math.Max(0, (Width * Math.Max(0, Math.Min(100, Value))) / 100);
            if (fillWidth > 0)
            {
                Rectangle fill = new Rectangle(0, 0, fillWidth, Height - 1);
                using (GraphicsPath fillPath = Round(fill, Height / 2))
                using (LinearGradientBrush brush = new LinearGradientBrush(fill, Color.FromArgb(255, 219, 91), Color.FromArgb(198, 137, 25), 0f))
                    e.Graphics.FillPath(brush, fillPath);
            }
        }

        private static GraphicsPath Round(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class FlatButton : Button
    {
        private readonly bool primary;
        private bool hover;

        public FlatButton(string text, bool primary)
        {
            this.primary = primary;
            Text = text;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(7, 14, 21);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10, FontStyle.Bold);
            Cursor = Cursors.Hand;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color fill = primary
                ? (hover ? Color.FromArgb(255, 219, 91) : Color.FromArgb(214, 151, 31))
                : Color.FromArgb(20, 36, 48);
            using (GraphicsPath path = Round(new Rectangle(0, 0, Width - 1, Height - 1), 6))
            using (SolidBrush brush = new SolidBrush(fill))
                e.Graphics.FillPath(brush, path);
            Color textColor = primary ? Color.FromArgb(12, 12, 10) : ForeColor;
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private static GraphicsPath Round(Rectangle rect, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
