using System.Windows.Forms;

namespace WinFormsKit
{
    /// <summary>
    /// Обертки над MessageBox с единым оформлением диалогов приложений
    /// </summary>
    public static class UserMessages
    {
        public static void Info(string text, string caption = "Информация")
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warning(string text, string caption = "Предупреждение")
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(string text, string caption = "Ошибка")
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Вопрос "Да/Нет". Возвращает true, если пользователь выбрал "Да"
        /// </summary>
        public static bool Confirm(
            string text,
            string caption,
            MessageBoxIcon icon = MessageBoxIcon.Question)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNo, icon) == DialogResult.Yes;
        }
    }
}
