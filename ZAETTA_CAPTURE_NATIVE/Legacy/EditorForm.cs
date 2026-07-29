using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed class EditorForm : Form
    {
        private const int WM_CONTEXTMENU = 0x007B;
        private readonly Bitmap original;
        private readonly List<DrawOp> ops = new List<DrawOp>();
        private Tool tool = Tool.Arrow;
        private Color color = Color.FromArgb(255, 59, 48);
        private int pixelIntensity = Pixelation.DefaultIntensity;
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
            ops.Add(new DrawOp { Tool = tool, A = start, B = current, Color = color, Width = tool == Tool.Pixelate ? pixelIntensity : 4 });
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
                DrawOperation(e.Graphics, new DrawOp { Tool = tool, A = start, B = current, Color = color, Width = tool == Tool.Pixelate ? pixelIntensity : 4 }, imageRect);
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

            Pixelation.Draw(g, original, source, rect, op.Width);
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
                        Pixelation.Draw(g, original, rect, rect, op.Width);
                    }
                }
            }
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
