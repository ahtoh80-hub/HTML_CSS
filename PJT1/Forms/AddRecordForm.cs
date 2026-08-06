using System;
using System.Drawing;
using System.Windows.Forms;
using WinFormsKit;

namespace PJT1.Forms
{
    /// <summary>
    /// ФОРМА ДЛЯ ДОБАВЛЕНИЯ ЗАПИСИ
    /// </summary>
    public class AddRecordForm : Form
    {
        public string Tagname { get; private set; }
        public string Loop { get; private set; }
        public string Comment { get; private set; }

        private Label lblTagname;
        private Label lblLoop;
        private Label lblComment;
        private TextBox txtTagname;
        private TextBox txtLoop;
        private TextBox txtComment;
        private Button btnOK;
        private Button btnCancel;

        public AddRecordForm()
        {
            this.Text = "Добавление записи";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            InitializeControls();

            this.Controls.Add(lblTagname);
            this.Controls.Add(lblLoop);
            this.Controls.Add(lblComment);
            this.Controls.Add(txtTagname);
            this.Controls.Add(txtLoop);
            this.Controls.Add(txtComment);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private void InitializeControls()
        {
            // Обязательные поля подсвечиваются основным цветом
            lblTagname = ControlFactory.CreateFieldLabel(
                "Имя тега:*", new Point(20, 20), ControlFactory.Primary);
            txtTagname = ControlFactory.CreateFieldTextBox(new Point(130, 20), new Size(320, 25));

            lblLoop = ControlFactory.CreateFieldLabel(
                "Цикл:*", new Point(20, 60), ControlFactory.Primary);
            txtLoop = ControlFactory.CreateFieldTextBox(new Point(130, 60), new Size(320, 25));

            lblComment = ControlFactory.CreateFieldLabel("Комментарий:", new Point(20, 100));
            txtComment = ControlFactory.CreateFieldTextBox(
                new Point(130, 100), new Size(320, 60), multiline: true);

            btnOK = ControlFactory.CreateDialogButton(
                "Добавить", new Point(270, 190), new Size(100, 35),
                ControlFactory.Primary, DialogResult.OK, BtnOK_Click);

            btnCancel = ControlFactory.CreateDialogButton(
                "Отмена", new Point(380, 190), new Size(80, 35),
                ControlFactory.Secondary, DialogResult.Cancel);

            // Валидация
            txtTagname.TextChanged += (s, e) => ValidateFields();
            txtLoop.TextChanged += (s, e) => ValidateFields();
        }

        private void ValidateFields()
        {
            btnOK.Enabled = !string.IsNullOrWhiteSpace(txtTagname.Text) &&
                           !string.IsNullOrWhiteSpace(txtLoop.Text);
        }

        /// <summary>
        /// Проверяет заполнение обязательного поля и сообщает об ошибке
        /// </summary>
        private bool IsRequiredFieldFilled(TextBox field, string fieldTitle)
        {
            if (!string.IsNullOrWhiteSpace(field.Text))
                return true;

            UserMessages.Warning($"Поле '{fieldTitle}' обязательно для заполнения.", "Ошибка");
            this.DialogResult = DialogResult.None;
            return false;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (!IsRequiredFieldFilled(txtTagname, "Имя тега") ||
                !IsRequiredFieldFilled(txtLoop, "Цикл"))
            {
                return;
            }

            Tagname = txtTagname.Text.Trim();
            Loop = txtLoop.Text.Trim();
            Comment = txtComment.Text.Trim();
        }
    }
}