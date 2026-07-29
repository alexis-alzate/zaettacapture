using System;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                NativeDpi.Enable();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new TrayContext());
            }
            catch (Exception ex)
            {
                StartupDiagnostics.Log(ex);
                MessageBox.Show(
                    "Zaetta Capture no pudo iniciar.\n\n" + ex.Message,
                    AppInfo.Name,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
