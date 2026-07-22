using System;
using System.Drawing;
using System.Windows.Forms;

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
            // Метка "Имя тега"
            lblTagname = new Label();
            lblTagname.Text = "Имя тега:*";
            lblTagname.Location = new Point(20, 20);
            lblTagname.Size = new Size(100, 25);
            lblTagname.Font = new Font("Arial", 10, FontStyle.Bold);
            lblTagname.ForeColor = Color.FromArgb(0, 123, 255);

            // Поле "Имя тега"
            txtTagname = new TextBox();
            txtTagname.Location = new Point(130, 20);
            txtTagname.Size = new Size(320, 25);
            txtTagname.Font = new Font("Arial", 10);
            txtTagname.BackColor = Color.FromArgb(248, 248, 248);

            // Метка "Цикл"
            lblLoop = new Label();
            lblLoop.Text = "Цикл:*";
            lblLoop.Location = new Point(20, 60);
            lblLoop.Size = new Size(100, 25);
            lblLoop.Font = new Font("Arial", 10, FontStyle.Bold);
            lblLoop.ForeColor = Color.FromArgb(0, 123, 255);

            // Поле "Цикл"
            txtLoop = new TextBox();
            txtLoop.Location = new Point(130, 60);
            txtLoop.Size = new Size(320, 25);
            txtLoop.Font = new Font("Arial", 10);
            txtLoop.BackColor = Color.FromArgb(248, 248, 248);

            // Метка "Комментарий"
            lblComment = new Label();
            lblComment.Text = "Комментарий:";
            lblComment.Location = new Point(20, 100);
            lblComment.Size = new Size(100, 25);
            lblComment.Font = new Font("Arial", 10, FontStyle.Bold);

            // Поле "Комментарий"
            txtComment = new TextBox();
            txtComment.Location = new Point(130, 100);
            txtComment.Size = new Size(320, 60);
            txtComment.Font = new Font("Arial", 10);
            txtComment.BackColor = Color.FromArgb(248, 248, 248);
            txtComment.Multiline = true;
            txtComment.ScrollBars = ScrollBars.Vertical;

            // Кнопка "OK"
            btnOK = new Button();
            btnOK.Text = "Добавить";
            btnOK.Location = new Point(270, 190);
            btnOK.Size = new Size(100, 35);
            btnOK.BackColor = Color.FromArgb(0, 123, 255);
            btnOK.ForeColor = Color.White;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOK.Cursor = Cursors.Hand;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Click += BtnOK_Click;

            // Кнопка "Отмена"
            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(380, 190);
            btnCancel.Size = new Size(80, 35);
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;

            // Валидация
            txtTagname.TextChanged += (s, e) => ValidateFields();
            txtLoop.TextChanged += (s, e) => ValidateFields();
        }

        private void ValidateFields()
        {
            btnOK.Enabled = !string.IsNullOrWhiteSpace(txtTagname.Text) &&
                           !string.IsNullOrWhiteSpace(txtLoop.Text);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTagname.Text))
            {
                MessageBox.Show(
                    "Поле 'Имя тега' обязательно для заполнения.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                this.DialogResult = DialogResult.None;
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLoop.Text))
            {
                MessageBox.Show(
                    "Поле 'Цикл' обязательно для заполнения.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                this.DialogResult = DialogResult.None;
                return;
            }

            Tagname = txtTagname.Text.Trim();
            Loop = txtLoop.Text.Trim();
            Comment = txtComment.Text.Trim();
        }
    }
}