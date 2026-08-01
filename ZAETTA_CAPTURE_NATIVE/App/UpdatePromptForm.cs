using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class UpdatePromptForm : Form
    {
        public UpdatePromptForm(UpdateInfo info)
        {
            Text = "Actualizacion disponible - " + AppInfo.Name;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = true;
            TopMost = true;
            ClientSize = new Size(440, 310);
            BackColor = Ui.Bg;
            ForeColor = Ui.Text;
            Font = new Font("Segoe UI", 9f);

            Label title = new Label();
            title.Text = "Nueva version disponible";
            title.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            title.ForeColor = Ui.Text;
            title.Location = new Point(28, 24);
            title.Size = new Size(380, 34);

            Label version = new Label();
            version.Text = AppInfo.Version + " -> " + info.Version + BuildReleaseDate(info);
            version.ForeColor = Ui.Accent2;
            version.Location = new Point(30, 64);
            version.Size = new Size(380, 24);

            Label body = new Label();
            body.Text = "Presiona Actualizar una vez. Zaetta descargara, validara, cerrara la version anterior y abrira la nueva automaticamente.";
            body.ForeColor = Color.FromArgb(210, 226, 232);
            body.Location = new Point(30, 96);
            body.Size = new Size(380, 44);

            Label notes = new Label();
            notes.Text = BuildNotes(info);
            notes.ForeColor = Color.FromArgb(226, 234, 238);
            notes.BackColor = Ui.Panel;
            notes.Padding = new Padding(14, 12, 14, 12);
            notes.Location = new Point(30, 152);
            notes.Size = new Size(380, 82);

            ZaettaButton update = new ZaettaButton("Actualizar", true);
            update.TextFill = Color.FromArgb(12, 12, 10);
            update.Location = new Point(268, 254);
            update.Size = new Size(142, 36);
            update.Click += delegate
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            ZaettaButton later = new ZaettaButton("Mas tarde", false);
            later.Location = new Point(112, 254);
            later.Size = new Size(142, 36);
            later.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(title);
            Controls.Add(version);
            Controls.Add(body);
            Controls.Add(notes);
            Controls.Add(later);
            Controls.Add(update);
            AcceptButton = update;
            CancelButton = later;
            Shown += delegate
            {
                WindowState = FormWindowState.Normal;
                BringToFront();
                Activate();
                Focus();
            };
        }

        private static string BuildReleaseDate(UpdateInfo info)
        {
            return string.IsNullOrWhiteSpace(info.ReleasedAt) ? string.Empty : " - " + info.ReleasedAt;
        }

        private static string BuildNotes(UpdateInfo info)
        {
            if (info.Notes == null || info.Notes.Length == 0)
                return "Mejoras y correcciones incluidas en esta version.";

            int count = Math.Min(3, info.Notes.Length);
            string text = string.Empty;
            for (int i = 0; i < count; i++)
                text += "- " + info.Notes[i] + (i + 1 < count ? Environment.NewLine : string.Empty);

            return text;
        }
    }
}
