using System;
using System.Windows.Forms;

namespace IO_PJT
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportFatal(e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void ReportFatal(Exception? ex)
        {
            MessageBox.Show(
                $"Непредвиденная ошибка:\n{ex}",
                "Критическая ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}