using System;
using System.Threading;
using System.Windows.Forms;

namespace ZaettaCaptureNative
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew = false;
            Mutex singleInstance = null;

            try
            {
                singleInstance = new Mutex(true, "Local\\ZaettaCaptureNative", out createdNew);
                if (!createdNew)
                    return;

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
            finally
            {
                if (singleInstance != null)
                {
                    if (createdNew)
                        singleInstance.ReleaseMutex();
                    singleInstance.Dispose();
                }
            }
        }
    }
}
