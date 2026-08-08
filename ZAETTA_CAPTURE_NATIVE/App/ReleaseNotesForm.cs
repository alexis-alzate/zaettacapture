using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class ReleaseNotesForm : Form
    {
        public ReleaseNotesForm(UpdateInfo info)
        {
            Text = "Novedades - " + AppInfo.Name;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            ClientSize = new Size(560, 450);
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Todo lo nuevo en Zaetta Capture";
            title.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            title.ForeColor = Ui.Text;
            title.Location = new Point(30, 24);
            title.Size = new Size(500, 34);

            Label version = new Label();
            version.Text = "Version " + info.Version + BuildReleaseDate(info);
            version.ForeColor = Ui.Accent2;
            version.Location = new Point(30, 64);
            version.Size = new Size(500, 24);

            Label intro = new Label();
            intro.Text = "Estas son las mejoras y correcciones incluidas en esta actualizacion:";
            intro.ForeColor = Color.FromArgb(210, 226, 232);
            intro.Location = new Point(30, 96);
            intro.Size = new Size(500, 24);

            RichTextBox notes = new RichTextBox();
            notes.Text = BuildAllNotes(info);
            notes.ReadOnly = true;
            notes.BorderStyle = BorderStyle.FixedSingle;
            notes.BackColor = Ui.Panel;
            notes.ForeColor = Color.FromArgb(226, 234, 238);
            notes.Font = new Font("Segoe UI", 10f);
            notes.Location = new Point(30, 128);
            notes.Size = new Size(500, 230);
            notes.ScrollBars = RichTextBoxScrollBars.Vertical;
            notes.DetectUrls = true;
            notes.TabStop = false;
            notes.LinkClicked += delegate(object sender, LinkClickedEventArgs e)
            {
                OpenUrl(e.LinkText);
            };

            Label count = new Label();
            count.Text = BuildCount(info);
            count.ForeColor = Ui.Muted;
            count.Location = new Point(30, 368);
            count.Size = new Size(220, 20);

            ZaettaButton web = new ZaettaButton("Ver en la web", false);
            web.Location = new Point(30, 397);
            web.Size = new Size(125, 36);
            web.Visible = IsHttpUrl(info.ReleaseNotesUrl);
            web.Click += delegate { OpenUrl(info.ReleaseNotesUrl); };

            ZaettaButton back = new ZaettaButton("Volver", false);
            back.Location = new Point(260, 397);
            back.Size = new Size(125, 36);
            back.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            ZaettaButton update = new ZaettaButton("Actualizar ahora", true);
            update.TextFill = Color.FromArgb(12, 12, 10);
            update.Location = new Point(405, 397);
            update.Size = new Size(125, 36);
            update.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            Controls.Add(title);
            Controls.Add(version);
            Controls.Add(intro);
            Controls.Add(notes);
            Controls.Add(count);
            Controls.Add(web);
            Controls.Add(back);
            Controls.Add(update);
            AcceptButton = update;
            CancelButton = back;
        }

        private static string BuildAllNotes(UpdateInfo info)
        {
            if (info.Notes == null || info.Notes.Length == 0)
                return "Mejoras y correcciones incluidas en esta version.";

            StringBuilder text = new StringBuilder();
            for (int i = 0; i < info.Notes.Length; i++)
            {
                if (i > 0)
                    text.AppendLine().AppendLine();

                text.Append(i + 1).Append(". ").Append(info.Notes[i]);
            }
            return text.ToString();
        }

        private static string BuildCount(UpdateInfo info)
        {
            int count = info.Notes == null ? 0 : info.Notes.Length;
            return count == 1 ? "1 cambio incluido" : count + " cambios incluidos";
        }

        private static string BuildReleaseDate(UpdateInfo info)
        {
            return string.IsNullOrWhiteSpace(info.ReleasedAt) ? string.Empty : " - " + info.ReleasedAt;
        }

        private static bool IsHttpUrl(string url)
        {
            Uri parsed;
            return Uri.TryCreate(url, UriKind.Absolute, out parsed)
                && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
        }

        private static void OpenUrl(string url)
        {
            if (!IsHttpUrl(url))
                return;

            try
            {
                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir el enlace.\n\n" + ex.Message,
                    AppInfo.Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
    }
}
