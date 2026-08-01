using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class UpdateProgressForm : Form
    {
        private readonly UpdateInfo info;
        private readonly ProgressBar progress;
        private readonly Label status;
        private readonly ZaettaButton cancelButton;
        private WebClient client;
        private string installerPath;

        public UpdateProgressForm(UpdateInfo info)
        {
            this.info = info;
            Text = "Actualizando - " + AppInfo.Name;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            ClientSize = new Size(440, 230);
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Descargando actualizacion";
            title.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            title.ForeColor = Ui.Text;
            title.Location = new Point(28, 24);
            title.Size = new Size(380, 34);

            status = new Label();
            status.Text = "Preparando descarga...";
            status.ForeColor = Color.FromArgb(210, 226, 232);
            status.Location = new Point(30, 76);
            status.Size = new Size(380, 30);

            progress = new ProgressBar();
            progress.Location = new Point(32, 118);
            progress.Size = new Size(376, 18);
            progress.Style = ProgressBarStyle.Continuous;

            cancelButton = new ZaettaButton("Cancelar", false);
            cancelButton.Location = new Point(266, 164);
            cancelButton.Size = new Size(142, 36);
            cancelButton.Click += delegate { CancelDownload(); };

            Controls.Add(title);
            Controls.Add(status);
            Controls.Add(progress);
            Controls.Add(cancelButton);
            Shown += delegate
            {
                WindowState = FormWindowState.Normal;
                BringToFront();
                Activate();
                Focus();
                StartDownload();
            };
            FormClosing += OnFormClosing;
        }

        private void StartDownload()
        {
            try
            {
                Directory.CreateDirectory(Paths.UpdatesDir);
                installerPath = UpdateService.GetInstallerDownloadPath(info);

                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                client = new WebClient();
                client.Headers[HttpRequestHeader.UserAgent] = AppInfo.Name + "/" + AppInfo.Version;
                client.DownloadProgressChanged += OnDownloadProgressChanged;
                client.DownloadFileCompleted += OnDownloadFileCompleted;
                client.DownloadFileAsync(new Uri(info.DownloadUrl), installerPath);
            }
            catch (Exception ex)
            {
                ShowFailure("No se pudo iniciar la descarga.\n\n" + ex.Message);
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progress.Value = Math.Max(0, Math.Min(100, e.ProgressPercentage));
            status.Text = "Descargando " + progress.Value + "%";
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                status.Text = "Descarga cancelada.";
                return;
            }

            if (e.Error != null)
            {
                ShowFailure("No se pudo descargar la actualizacion.\n\n" + e.Error.Message);
                return;
            }

            try
            {
                status.Text = "Validando instalador...";
                if (!UpdateService.VerifySha256(installerPath, info.Sha256))
                {
                    TryDeleteInstaller();
                    ShowFailure("La validacion SHA256 fallo. El instalador descargado no coincide con el manifest oficial.");
                    return;
                }

                status.Text = "Abriendo instalador...";
                Process.Start(new ProcessStartInfo(installerPath, "/upgrade") { UseShellExecute = true });
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                ShowFailure("No se pudo abrir el instalador.\n\n" + ex.Message);
            }
        }

        private void CancelDownload()
        {
            if (client != null && client.IsBusy)
                client.CancelAsync();

            Close();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null && client.IsBusy)
                client.CancelAsync();
        }

        private void ShowFailure(string message)
        {
            status.Text = "Actualizacion detenida.";
            MessageBox.Show(message, AppInfo.Name, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cancelButton.Text = "Cerrar";
        }

        private void TryDeleteInstaller()
        {
            try
            {
                if (!string.IsNullOrEmpty(installerPath) && File.Exists(installerPath))
                    File.Delete(installerPath);
            }
            catch
            {
            }
        }
    }
}
