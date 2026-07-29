using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class ToolShortcuts
    {
        public static bool TryGet(Keys key, bool includeFullOverlayTools, out Tool tool)
        {
            tool = Tool.Arrow;
            switch (key)
            {
                case Keys.P:
                case Keys.M:
                    tool = Tool.Move;
                    return includeFullOverlayTools;
                case Keys.F:
                case Keys.A:
                    tool = Tool.Arrow;
                    return true;
                case Keys.R:
                    tool = Tool.Rect;
                    return true;
                case Keys.L:
                    tool = Tool.Line;
                    return includeFullOverlayTools;
                case Keys.D:
                    tool = Tool.Pencil;
                    return includeFullOverlayTools;
                case Keys.H:
                    tool = Tool.Highlight;
                    return includeFullOverlayTools;
                case Keys.T:
                    tool = Tool.Text;
                    return true;
                case Keys.X:
                    tool = Tool.Pixelate;
                    return true;
                case Keys.N:
                    tool = Tool.Number;
                    return includeFullOverlayTools;
                default:
                    return false;
            }
        }
    }
}
