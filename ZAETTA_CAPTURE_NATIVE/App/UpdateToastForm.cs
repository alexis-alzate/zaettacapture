using System;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class UpdateToastForm : Form
    {
        private readonly Timer closeTimer;
        private readonly Timer fadeTimer;

        public UpdateToastForm(UpdateInfo info)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            BackColor = Color.White;
            ForeColor = Color.FromArgb(28, 38, 48);
            Font = new Font("Segoe UI", 9f);
            ClientSize = new Size(470, 122);
            Opacity = 0.98;

            Panel iconBand = new Panel();
            iconBand.BackColor = Color.FromArgb(245, 245, 245);
            iconBand.SetBounds(0, 0, 96, ClientSize.Height);

            PictureBox logo = new PictureBox();
            logo.Image = LoadAppIconBitmap();
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.SetBounds(30, 34, 42, 42);
            iconBand.Controls.Add(logo);

            Label title = new Label();
            title.Text = "Actualizacion disponible";
            title.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(20, 30, 40);
            title.SetBounds(112, 18, 310, 22);

            Label message = new Label();
            message.Text = "Zaetta Capture " + info.Version + " esta listo para instalarse.";
            message.ForeColor = Color.FromArgb(36, 49, 62);
            message.SetBounds(112, 42, 310, 42);

            Button close = new Button();
            close.Text = "x";
            close.FlatStyle = FlatStyle.Flat;
            close.FlatAppearance.BorderSize = 0;
            close.BackColor = Color.White;
            close.ForeColor = Color.FromArgb(70, 70, 70);
            close.Font = new Font("Segoe UI", 9f);
            close.SetBounds(436, 8, 24, 24);
            close.Click += delegate { Close(); };

            Label hint = new Label();
            hint.Text = "Abriendo asistente...";
            hint.ForeColor = Ui.Accent;
            hint.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            hint.SetBounds(112, 86, 220, 22);

            Controls.Add(iconBand);
            Controls.Add(title);
            Controls.Add(message);
            Controls.Add(close);
            Controls.Add(hint);

            closeTimer = new Timer();
            closeTimer.Interval = 5000;
            closeTimer.Tick += delegate
            {
                closeTimer.Stop();
                fadeTimer.Start();
            };

            fadeTimer = new Timer();
            fadeTimer.Interval = 25;
            fadeTimer.Tick += delegate
            {
                Opacity -= 0.08;
                if (Opacity <= 0.05)
                {
                    fadeTimer.Stop();
                    Close();
                }
            };

            Shown += delegate
            {
                PositionNearTray();
                BringToFront();
                Activate();
                closeTimer.Start();
            };
            FormClosed += delegate
            {
                closeTimer.Dispose();
                fadeTimer.Dispose();
            };
        }

        public static void ShowFor(UpdateInfo info)
        {
            using (UpdateToastForm toast = new UpdateToastForm(info))
            {
                toast.Show();
                while (!toast.IsDisposed && toast.Visible)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(15);
                }
            }
        }

        private void PositionNearTray()
        {
            Rectangle workArea = Screen.FromPoint(Cursor.Position).WorkingArea;
            Left = workArea.Right - Width - 18;
            Top = workArea.Bottom - Height - 18;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen border = new Pen(Color.FromArgb(218, 218, 218), 1))
                e.Graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
        }

        private static Bitmap LoadAppIconBitmap()
        {
            try
            {
                Icon icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (icon != null)
                    return icon.ToBitmap();
            }
            catch
            {
            }

            return SystemIcons.Application.ToBitmap();
        }
    }
}
