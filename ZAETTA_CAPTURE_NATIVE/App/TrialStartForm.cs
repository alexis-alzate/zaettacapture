using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class TrialStartForm : Form
    {
        private readonly TextBox emailBox;
        private readonly Label statusLabel;
        private readonly Button startButton;

        public string Email { get; private set; }

        public TrialStartForm()
        {
            Text = "Activar prueba gratuita";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(420, 226);
            BackColor = Color.FromArgb(8, 9, 9);

            Label title = new Label
            {
                Text = "Bienvenido a " + AppInfo.Name,
                AutoSize = false,
                Left = 24,
                Top = 20,
                Width = 372,
                Height = 28,
                ForeColor = Color.FromArgb(244, 196, 48),
                Font = new Font("Segoe UI", 13, FontStyle.Bold)
            };

            Label hint = new Label
            {
                Text = "Ingresa tu correo para comenzar tu prueba gratuita de 30 dias.",
                AutoSize = false,
                Left = 24,
                Top = 54,
                Width = 372,
                Height = 40,
                ForeColor = Color.FromArgb(167, 170, 164),
                Font = new Font("Segoe UI", 9.5f)
            };

            emailBox = new TextBox
            {
                Left = 24,
                Top = 98,
                Width = 372,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.FromArgb(10, 11, 9),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            statusLabel = new Label
            {
                Left = 24,
                Top = 128,
                Width = 372,
                Height = 34,
                ForeColor = Color.FromArgb(226, 104, 92),
                Font = new Font("Segoe UI", 8.5f),
                Visible = false
            };

            startButton = new Button
            {
                Text = "Comenzar prueba gratuita",
                Left = 24,
                Top = 172,
                Width = 372,
                Height = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(244, 196, 48),
                ForeColor = Color.FromArgb(35, 28, 4),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += delegate { TryAccept(); };

            Controls.Add(title);
            Controls.Add(hint);
            Controls.Add(emailBox);
            Controls.Add(statusLabel);
            Controls.Add(startButton);

            AcceptButton = startButton;
        }

        private void TryAccept()
        {
            string value = emailBox.Text.Trim();
            if (!IsValidEmail(value))
            {
                statusLabel.Text = "Ingresa un correo valido.";
                statusLabel.Visible = true;
                return;
            }

            Email = value.ToLowerInvariant();
            DialogResult = DialogResult.OK;
            Close();
        }

        private static bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 5 || value.Length > 254)
                return false;

            return Regex.IsMatch(value, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
        }
    }
}
