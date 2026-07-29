using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay
    {
        private void ShowToolbars()
        {
            HideToolbars();
            bottomToolbar = new FloatingToolbarPanel { Width = 506, Height = 34 };
            bottomToolbar.Left = Math.Max(8, Math.Min(selection.Left, ClientSize.Width - bottomToolbar.Width - 8));
            bottomToolbar.Top = selection.Bottom + 8 < ClientSize.Height - bottomToolbar.Height
                ? selection.Bottom + 8
                : Math.Max(8, selection.Top - bottomToolbar.Height - 8);
            bottomToolbar.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            bottomToolbar.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            Controls.Add(bottomToolbar);
            bottomToolbar.BringToFront();

            ZaettaButton brand = AddToolButton(bottomToolbar, "Z", 5, 4, 28, false, delegate { ShowAbout(); }, AppInfo.AboutTitle + ".");
            brand.OutlineOnly = false;
            brand.Fill = Color.FromArgb(3, 8, 13);
            brand.HoverFill = Color.FromArgb(30, 24, 16);
            brand.TextFill = Ui.Accent2;
            brand.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            brand.Invalidate();
            ZaettaButton toolButton = AddToolButton(bottomToolbar, ToolName(tool), 38, 4, 72, false, delegate { }, "Herramienta activa.");
            toolButton.TextFill = Color.FromArgb(238, 246, 250);
            toolButton.Invalidate();
            ColorSwatchButton colorButton = AddColorButton(bottomToolbar, 116, 4, 28);
            tips.SetToolTip(colorButton, "Cambiar color de flechas, marcos, texto y resaltador.");
            colorButton.Click += delegate { ShowColorMenu(colorButton); };
            AddToolButton(bottomToolbar, "-", 150, 4, 28, false, delegate { Thinner(); }, tool == Tool.Pixelate ? "Disminuir intensidad del pixelado." : "Disminuir grosor del trazo.");
            AddToolButton(bottomToolbar, "+", 184, 4, 28, false, delegate { Thicker(); }, tool == Tool.Pixelate ? "Aumentar intensidad del pixelado." : "Aumentar grosor del trazo.");
            AddToolButton(bottomToolbar, "Undo", 218, 4, 42, false, delegate { Undo(); }, "Deshacer el ultimo cambio.");
            ZaettaButton moreBottom = AddToolButton(bottomToolbar, "...", 266, 4, 34, false, delegate { }, "Ver mas herramientas.");
            moreBottom.Click += delegate { ShowToolsMenu(moreBottom); };
            AddToolButton(bottomToolbar, "Copiar", 306, 4, 68, false, delegate { CopyAndClose(); }, "Copiar al portapapeles y cerrar la captura.");
            AddToolButton(bottomToolbar, "Guardar", 380, 4, 70, true, delegate { SaveImage(); }, "Guardar la captura como archivo PNG.");
            ZaettaButton close = AddToolButton(bottomToolbar, "X", 456, 4, 44, false, delegate { Close(); }, "Cancelar y cerrar sin copiar.");
            close.Fill = Color.FromArgb(120, 32, 42);
            close.HoverFill = Color.FromArgb(170, 48, 58);
            close.Invalidate();

            sideToolbar = new FloatingToolbarPanel { Width = 36, Height = 188 };
            sideToolbar.Left = selection.Right + 6 < ClientSize.Width - sideToolbar.Width
                ? selection.Right + 6
                : Math.Max(8, selection.Left - sideToolbar.Width - 6);
            sideToolbar.Top = Math.Max(8, Math.Min(selection.Top, ClientSize.Height - sideToolbar.Height - 8));
            sideToolbar.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            sideToolbar.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            Controls.Add(sideToolbar);
            sideToolbar.BringToFront();
            AddToolButton(sideToolbar, "P", 4, 4, 28, tool == Tool.Move, delegate { SetTool(Tool.Move); }, "Mover cualquier anotacion agregada.");
            AddToolButton(sideToolbar, "->", 4, 34, 28, tool == Tool.Arrow, delegate { SetTool(Tool.Arrow); }, "Dibujar flechas para senalar puntos importantes.");
            AddToolButton(sideToolbar, "[]", 4, 64, 28, tool == Tool.Rect, delegate { SetTool(Tool.Rect); }, "Dibujar marcos para resaltar areas.");
            AddToolButton(sideToolbar, "T", 4, 94, 28, tool == Tool.Text, delegate { SetTool(Tool.Text); }, "Agregar texto editable dentro de la captura.");
            AddToolButton(sideToolbar, "Px", 4, 124, 28, tool == Tool.Pixelate, delegate { SetTool(Tool.Pixelate); }, "Pixelar informacion sensible.");
            ZaettaButton moreSide = AddToolButton(sideToolbar, "...", 4, 154, 28, false, delegate { }, "Ver herramientas adicionales.");
            moreSide.Click += delegate { ShowToolsMenu(moreSide); };
        }

        private ZaettaButton AddToolButton(Panel parent, string text, int x, int y, int width, bool primary, EventHandler click, string tooltip)
        {
            ZaettaButton button = new ZaettaButton(text, primary);
            button.Left = x;
            button.Top = y;
            button.Width = width;
            button.Height = 26;
            button.Radius = width <= 34 ? 5 : 6;

            string iconKey = IconKeyForButton(text);
            if (!string.IsNullOrEmpty(iconKey))
            {
                button.IconKey = iconKey;
                button.Text = string.Empty;
                button.TextFill = primary ? Color.FromArgb(4, 11, 16) : Color.FromArgb(220, 240, 246);
            }

            if (primary && parent == sideToolbar)
            {
                button.Fill = Ui.Accent;
                button.HoverFill = Ui.Accent2;
                button.TextFill = Color.FromArgb(12, 12, 10);
            }
            else if (parent == sideToolbar)
            {
                button.Fill = Color.FromArgb(12, 16, 19);
                button.HoverFill = Color.FromArgb(34, 28, 19);
                button.TextFill = Color.FromArgb(238, 232, 212);
            }

            button.Click += click;
            tips.SetToolTip(button, tooltip);
            button.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            button.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            parent.Controls.Add(button);
            return button;
        }

        private static string IconKeyForButton(string text)
        {
            if (text == "P")
                return "move";
            if (text == "->")
                return "arrow";
            if (text == "[]")
                return "rect";
            if (text == "T")
                return "text";
            if (text == "Px")
                return "pixel";
            if (text == "...")
                return "more";
            return string.Empty;
        }

        private static void ConfigureTips(ToolTip tooltip)
        {
            tooltip.InitialDelay = 350;
            tooltip.ReshowDelay = 100;
            tooltip.AutoPopDelay = 6500;
            tooltip.ShowAlways = true;
        }

        private ColorSwatchButton AddColorButton(Panel parent, int x, int y, int size)
        {
            ColorSwatchButton button = new ColorSwatchButton();
            button.Left = x;
            button.Top = y;
            button.Width = size;
            button.Height = 26;
            button.Swatch = color;
            button.MouseDown += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    BeginRightCopy();
            };
            button.MouseUp += delegate(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right)
                    FinishRightCopy();
            };
            parent.Controls.Add(button);
            return button;
        }

        private void HideToolbars()
        {
            if (bottomToolbar != null)
            {
                Controls.Remove(bottomToolbar);
                bottomToolbar.Dispose();
                bottomToolbar = null;
            }
            if (sideToolbar != null)
            {
                Controls.Remove(sideToolbar);
                sideToolbar.Dispose();
                sideToolbar = null;
            }
        }
    }
}
