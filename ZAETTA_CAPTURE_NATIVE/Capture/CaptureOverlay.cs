using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal sealed partial class CaptureOverlay : Form
    {
        private const int WM_CONTEXTMENU = 0x007B;
        private readonly Bitmap screenshot;
        private readonly Bitmap dimmedScreenshot;
        private readonly List<DrawOp> ops = new List<DrawOp>();
        private Rectangle selection;
        private Point start;
        private Point current;
        private Point drawStart;
        private bool selecting;
        private bool reselecting;
        private Rectangle previousSelectionBeforeReselect;
        private bool drawing;
        private Tool tool = Tool.Arrow;
        private Color color = Color.FromArgb(255, 59, 48);
        private int drawWidth = 4;
        private int pixelIntensity = Pixelation.DefaultIntensity;
        private int counterValue = 1;
        private Panel bottomToolbar;
        private Panel sideToolbar;
        private TextBox activeTextBox;
        private bool textEditing;
        private Point activeTextPoint;
        private Rectangle activeTextBounds;
        private bool movingActiveText;
        private Point activeTextMoveOffset;
        private int activeTextSize = 18;
        private bool activeTextCaretVisible = true;
        private Timer activeTextCaretTimer;
        private string activeTextValue = "";
        private DrawOp selectedOp;
        private DrawOp movingOp;
        private bool movingSelection;
        private DrawOp resizingOp;
        private int resizeHandleIndex = -1;
        private bool resizingSelection;
        private int selectionResizeHandleIndex = -1;
        private Point moveOffset;
        private Point selectionMoveOffset;
        private bool pendingRightCopy;
        private bool leftButtonDown;
        private bool selectionLocked;
        private Control pendingRightOwner;
        private Point pendingRightLocation;
        private readonly ToolTip tips = new ToolTip();
        private static readonly Color SelectionStroke = Color.FromArgb(245, 245, 245);
        private static readonly Color SelectionShadow = Color.FromArgb(72, 24, 24, 24);
        private static readonly Color SelectionDash = Color.FromArgb(42, 42, 42);
        private static readonly Color SelectionHandle = Color.FromArgb(235, 245, 245, 245);
        private static readonly Color SelectionHandleBorder = Color.FromArgb(95, 95, 95);
        private static readonly Color SelectionLabelBg = Color.FromArgb(232, 20, 20, 20);

        public CaptureOverlay(Rectangle screenBounds, Bitmap screenshot)
            : this(screenBounds, screenshot, false)
        {
        }

        public CaptureOverlay(Rectangle screenBounds, Bitmap screenshot, bool startLocked)
        {
            this.screenshot = screenshot;
            this.dimmedScreenshot = BuildDimmedScreenshot(screenshot);
            selectionLocked = startLocked;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Location = screenBounds.Location;
            Size = screenBounds.Size;
            TopMost = true;
            ShowInTaskbar = false;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Cursor = Cursors.Cross;
            KeyPreview = true;
            ContextMenuStrip = ContextMenus.Suppressed();
            ConfigureTips(tips);
            BackColor = Color.Black;
            Opacity = 1;
        }

        public CaptureOverlay(Rectangle screenBounds, Bitmap screenshot, Rectangle initialSelection)
            : this(screenBounds, screenshot, initialSelection, false)
        {
        }

        public CaptureOverlay(Rectangle screenBounds, Bitmap screenshot, Rectangle initialSelection, bool startLocked)
            : this(screenBounds, screenshot, startLocked)
        {
            selection = ClampInitialSelection(initialSelection);
            start = selection.Location;
            current = new Point(selection.Right, selection.Bottom);
        }

        public bool HasCompletedSelection
        {
            get { return HasSelection(); }
        }

        public Rectangle CurrentSelection
        {
            get { return selection; }
        }

        private void ShowAbout()
        {
            MessageBox.Show(AppInfo.AboutText, AppInfo.AboutTitle);
        }

        private bool HasSelection()
        {
            return selection.Width > 0 && selection.Height > 0;
        }

        private Point ClampToSelection(Point p)
        {
            return new Point(
                Math.Max(selection.Left, Math.Min(selection.Right, p.X)),
                Math.Max(selection.Top, Math.Min(selection.Bottom, p.Y))
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dimmedScreenshot.Dispose();
                screenshot.Dispose();
                if (activeTextCaretTimer != null)
                {
                    activeTextCaretTimer.Stop();
                    activeTextCaretTimer.Dispose();
                }
                HideToolbars();
            }
            base.Dispose(disposing);
        }

        private static Bitmap BuildDimmedScreenshot(Bitmap source)
        {
            Bitmap dimmed = new Bitmap(source.Width, source.Height);
            using (Graphics g = Graphics.FromImage(dimmed))
            {
                g.DrawImageUnscaled(source, Point.Empty);
                using (SolidBrush shade = new SolidBrush(Color.FromArgb(112, 0, 0, 0)))
                    g.FillRectangle(shade, new Rectangle(Point.Empty, dimmed.Size));
            }
            return dimmed;
        }

        private static Rectangle Normalize(Point a, Point b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        private Rectangle ClampInitialSelection(Rectangle requested)
        {
            int width = Math.Max(10, Math.Min(requested.Width, ClientSize.Width));
            int height = Math.Max(10, Math.Min(requested.Height, ClientSize.Height));
            int left = Math.Max(0, Math.Min(ClientSize.Width - width, requested.Left));
            int top = Math.Max(0, Math.Min(ClientSize.Height - height, requested.Top));
            return new Rectangle(left, top, width, height);
        }
    }
}
