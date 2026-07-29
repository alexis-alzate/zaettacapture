using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class ContextMenus
    {
        public static ContextMenuStrip Suppressed()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Opening += delegate(object sender, System.ComponentModel.CancelEventArgs e)
            {
                e.Cancel = true;
            };
            return menu;
        }
    }
}
