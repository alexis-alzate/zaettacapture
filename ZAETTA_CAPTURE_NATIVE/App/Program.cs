using System; // permite usar exeptión para manejo de errores, ayuda a capturar el error
using System.Threading; //control de hilos/sincronización Mutex. evita abrir 2 instanacias
using System.Windows.Forms; //crear y ejecutar la app visual de windows forms

namespace ZaettaCaptureNative// forma de organizar el código, como una carpteta logica donde viven las clases del proyecto
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew = false; //candado ya existe por lo tanto debe cerrar la instancia, o no se abre
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
