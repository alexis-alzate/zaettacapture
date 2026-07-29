using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color bg = Color.FromArgb(7, 10, 13);
        private readonly Color accent = Color.FromArgb(32, 29, 22);
        private readonly Color border = Ui.Accent2;

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
            using (Pen pen = new Pen(Color.FromArgb(76, 61, 34)))
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }
    }
}
