using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
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
}
