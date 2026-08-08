using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private void SetTool(Tool selected)
        {
            tool = selected;
            Cursor = tool == Tool.Text ? Cursors.IBeam : (tool == Tool.Move ? Cursors.SizeAll : Cursors.Cross);
            ShowToolbars();
            Invalidate();
        }

        private void ToggleSelectionLock()
        {
            selectionLocked = !selectionLocked;
            ShowToolbars();
            Invalidate();
        }

        private void ShowCaptureContextMenu(Control owner, Point location)
        {
            if (owner == null)
                owner = this;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Ui.Bg;
            menu.ForeColor = Color.FromArgb(238, 246, 250);
            menu.ShowImageMargin = false;
            menu.ShowItemToolTips = true;
            menu.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            menu.Padding = new Padding(2, 3, 2, 3);
            menu.Renderer = new DarkMenuRenderer();

            ToolStripMenuItem copy = new ToolStripMenuItem("Copiar");
            copy.ForeColor = menu.ForeColor;
            copy.Click += delegate { CopyAndClose(); };
            menu.Items.Add(copy);

            ToolStripMenuItem save = new ToolStripMenuItem("Guardar");
            save.ForeColor = menu.ForeColor;
            save.Click += delegate { SaveImage(); };
            menu.Items.Add(save);

            ToolStripMenuItem unlock = new ToolStripMenuItem("Desbloquear candado");
            unlock.ForeColor = menu.ForeColor;
            unlock.Click += delegate { ToggleSelectionLock(); };
            menu.Items.Add(unlock);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem cancel = new ToolStripMenuItem("Cancelar captura");
            cancel.ForeColor = menu.ForeColor;
            cancel.Click += delegate { Close(); };
            menu.Items.Add(cancel);

            menu.Show(owner, location);
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
            menu.BackColor = Ui.Bg;
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
            menu.BackColor = Ui.Bg;
            menu.ForeColor = Color.FromArgb(238, 246, 250);
            menu.ShowImageMargin = true;
            menu.ImageScalingSize = new Size(14, 14);
            menu.ShowItemToolTips = true;
            menu.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            menu.Padding = new Padding(2, 3, 2, 3);
            menu.Renderer = new DarkMenuRenderer();
            AddColorMenuItem(menu, "Zaetta", Ui.Accent2);
            AddColorMenuItem(menu, "Rojo", Color.FromArgb(255, 59, 48));
            AddColorMenuItem(menu, "Amarillo", Color.FromArgb(255, 204, 0));
            AddColorMenuItem(menu, "Verde", Color.FromArgb(52, 199, 89));
            AddColorMenuItem(menu, "Azul", Color.FromArgb(32, 196, 244));
            AddColorMenuItem(menu, "Blanco", Color.White);
            menu.Show(anchor, new Point(0, anchor.Height + 4));
        }

        private bool CanChangeSelectedOpColor()
        {
            return selectedOp != null && selectedOp.Tool != Tool.Pixelate;
        }

        private Color ActiveColor()
        {
            return CanChangeSelectedOpColor() ? selectedOp.Color : color;
        }

        private void ApplyColor(Color selected)
        {
            color = selected;
            if (CanChangeSelectedOpColor())
                selectedOp.Color = selected;

            ShowToolbars();
            Invalidate();
        }

        private void AddColorMenuItem(ContextMenuStrip menu, string name, Color selected)
        {
            string label = (ActiveColor().ToArgb() == selected.ToArgb() ? "> " : "  ") + name;
            ToolStripMenuItem item = new ToolStripMenuItem(label);
            item.Image = BuildColorIcon(selected);
            item.Padding = new Padding(4, 2, 10, 2);
            item.ToolTipText = CanChangeSelectedOpColor()
                ? "Cambiar la anotacion seleccionada a color " + name + "."
                : "Usar color " + name + " en las anotaciones.";
            item.Click += delegate { ApplyColor(selected); };
            menu.Items.Add(item);
        }

        private void AddToolMenuItem(ContextMenuStrip menu, Tool selected)
        {
            string label = (tool == selected ? "> " : "  ") + ToolName(selected);
            ToolStripMenuItem item = new ToolStripMenuItem(label);
            item.Image = BuildToolIcon(selected, tool == selected ? color : Color.FromArgb(232, 221, 196));
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
                Ui.Accent2,
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
            ApplyColor(colors[(index + 1) % colors.Length]);
        }

        private void Thinner()
        {
            if (tool == Tool.Pixelate)
            {
                if (selectedOp != null && selectedOp.Tool == Tool.Pixelate)
                    AdjustSelectedPixelIntensity(-2);
                else
                    AdjustPixelIntensity(-2);
            }
            else
                AdjustDrawWidth(-1);
            Invalidate();
        }

        private void Thicker()
        {
            if (tool == Tool.Pixelate)
            {
                if (selectedOp != null && selectedOp.Tool == Tool.Pixelate)
                    AdjustSelectedPixelIntensity(2);
                else
                    AdjustPixelIntensity(2);
            }
            else
                AdjustDrawWidth(1);
            Invalidate();
        }

        private void AdjustDrawWidth(int delta)
        {
            drawWidth = Math.Max(2, Math.Min(12, drawWidth + delta));
        }

        private void AdjustPixelIntensity(int delta)
        {
            pixelIntensity = Pixelation.ClampIntensity(pixelIntensity + delta);
        }

        private void AdjustSelectedPixelIntensity(int delta)
        {
            selectedOp.Width = Pixelation.ClampIntensity(selectedOp.Width + delta);
            pixelIntensity = selectedOp.Width;
        }
    }
}
