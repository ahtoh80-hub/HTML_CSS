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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDB());
        }
    }
}