using System;
using System.Windows.Forms;
using PJT1.Forms;

namespace PJT1
{
    /// <summary>
    /// ГЛАВНЫЙ КЛАСС ПРОГРАММЫ
    /// Точка входа в приложение
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// ТОЧКА ВХОДА
        /// Запускает главную форму приложения
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportFatal(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportFatal(e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDB());
        }

        /// <summary>
        /// ОБРАБОТКА НЕПЕРЕХВАЧЕННЫХ ИСКЛЮЧЕНИЙ
        /// Показывает пользователю причину сбоя вместо молчаливого падения
        /// </summary>
        private static void ReportFatal(Exception? ex)
        {
            MessageBox.Show(
                $"Непредвиденная ошибка:\n{ex}",
                "Критическая ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}