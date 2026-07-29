using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ZaettaCaptureNative
{
    internal sealed class HotKeyWindow : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        private readonly Action action;
        private readonly SynchronizationContext syncContext;
        private int currentId = 100;
        private bool registered;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotKeyWindow(Action action)
        {
            this.action = action;
            this.syncContext = SynchronizationContext.Current;
            CreateHandle(new CreateParams());
        }

        public bool Register(Keys key, uint modifiers)
        {
            if (registered)
            {
                UnregisterHotKey(Handle, currentId);
                registered = false;
            }

            registered = RegisterHotKey(Handle, currentId, modifiers, (uint)key);
            return registered;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                TriggerAction();
                return;
            }
            base.WndProc(ref m);
        }

        private void TriggerAction()
        {
            if (syncContext != null)
                syncContext.Post(delegate { action(); }, null);
            else
                action();
        }

        public void Dispose()
        {
            if (registered)
                UnregisterHotKey(Handle, currentId);
            DestroyHandle();
        }
    }
}
