// ================================================================
// ПРОСТРАНСТВА ИМЕН
// ================================================================
using System;
using System.Drawing;
using System.Windows.Forms;
using PJT1.Services;
using PJT1.Models; 

namespace PJT1.Forms
{
    /// <summary>
    /// КЛАСС FormatSelectionDialog - ДИАЛОГ ВЫБОРА ФОРМАТА
    /// 
    /// Позволяет пользователю выбрать формат сохранения данных
    /// </summary>
    public class FormatSelectionDialog : Form
    {
        // ============================================================
        // СВОЙСТВА ДЛЯ ПОЛУЧЕНИЯ РЕЗУЛЬТАТА
        // ============================================================

        /// <summary>
        /// Выбранный формат экспорта
        /// </summary>
        public TxtExportService.ExportFormat SelectedFormat { get; private set; }

        /// <summary>
        /// Включать ли заголовок в файл
        /// </summary>
        public bool IncludeHeader { get; private set; } = true;

        // ============================================================
        // ЭЛЕМЕНТЫ УПРАВЛЕНИЯ
        // ============================================================

        private GroupBox groupFormat;
        private RadioButton rbTable;
        private RadioButton rbColumn;
        private RadioButton rbList;
        private RadioButton rbJsonLike;
        private RadioButton rbDetailed;
        private CheckBox chkIncludeHeader;
        private Button btnOK;
        private Button btnCancel;
        private Panel panelPreview;
        private Label lblPreview;

        // ============================================================
        // КОНСТРУКТОР
        // ============================================================

        public FormatSelectionDialog()
        {
            // Настройка формы
            this.Text = "Выбор формата экспорта";
            this.Size = new Size(550, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Создаем элементы
            InitializeControls();

            // Устанавливаем формат по умолчанию
            rbTable.Checked = true;
            SelectedFormat = TxtExportService.ExportFormat.Table;

            // Обновляем предпросмотр
            UpdatePreview();
        }

        // ============================================================
        // МЕТОДЫ СОЗДАНИЯ ЭЛЕМЕНТОВ
        // ============================================================

        private void InitializeControls()
        {
            // Группа выбора формата
            groupFormat = new GroupBox();
            groupFormat.Text = "Выберите формат экспорта:";
            groupFormat.Location = new Point(15, 15);
            groupFormat.Size = new Size(520, 200);
            groupFormat.Font = new Font("Arial", 10, FontStyle.Bold);

            // Создаем радиокнопки
            int yPos = 30;
            rbTable = CreateRadioButton("Табличный (с разделителями)", 20, ref yPos);
            rbColumn = CreateRadioButton("Столбчатый (с выравниванием)", 20, ref yPos);
            rbList = CreateRadioButton("Список с маркерами", 20, ref yPos);
            rbJsonLike = CreateRadioButton("JSON-подобный", 20, ref yPos);
            rbDetailed = CreateRadioButton("Подробный (полное описание)", 20, ref yPos);

            // Подписываемся на события
            rbTable.CheckedChanged += (s, e) => { if (rbTable.Checked) { SelectedFormat = TxtExportService.ExportFormat.Table; UpdatePreview(); } };
            rbColumn.CheckedChanged += (s, e) => { if (rbColumn.Checked) { SelectedFormat = TxtExportService.ExportFormat.Column; UpdatePreview(); } };
            rbList.CheckedChanged += (s, e) => { if (rbList.Checked) { SelectedFormat = TxtExportService.ExportFormat.List; UpdatePreview(); } };
            rbJsonLike.CheckedChanged += (s, e) => { if (rbJsonLike.Checked) { SelectedFormat = TxtExportService.ExportFormat.JsonLike; UpdatePreview(); } };
            rbDetailed.CheckedChanged += (s, e) => { if (rbDetailed.Checked) { SelectedFormat = TxtExportService.ExportFormat.Detailed; UpdatePreview(); } };

            // Панель предпросмотра
            panelPreview = new Panel();
            panelPreview.Location = new Point(15, 225);
            panelPreview.Size = new Size(520, 120);
            panelPreview.BorderStyle = BorderStyle.FixedSingle;
            panelPreview.BackColor = Color.FromArgb(248, 248, 248);

            lblPreview = new Label();
            lblPreview.Location = new Point(5, 5);
            lblPreview.Size = new Size(505, 105);
            lblPreview.Font = new Font("Consolas", 8);
            lblPreview.Text = "Предпросмотр формата...";
            lblPreview.BackColor = Color.Transparent;

            // Чекбокс "Включить заголовок"
            chkIncludeHeader = new CheckBox();
            chkIncludeHeader.Text = "Включить заголовок и метаданные";
            chkIncludeHeader.Location = new Point(20, 355);
            chkIncludeHeader.Size = new Size(250, 25);
            chkIncludeHeader.Font = new Font("Arial", 10);
            chkIncludeHeader.Checked = true;
            chkIncludeHeader.CheckedChanged += (s, e) =>
            {
                IncludeHeader = chkIncludeHeader.Checked;
                UpdatePreview();
            };

            // Кнопки
            btnOK = new Button();
            btnOK.Text = "Сохранить";
            btnOK.Location = new Point(360, 395);
            btnOK.Size = new Size(100, 35);
            btnOK.BackColor = Color.FromArgb(0, 123, 255);
            btnOK.ForeColor = Color.White;
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Arial", 10, FontStyle.Bold);
            btnOK.Cursor = Cursors.Hand;
            btnOK.DialogResult = DialogResult.OK;

            btnCancel = new Button();
            btnCancel.Text = "Отмена";
            btnCancel.Location = new Point(470, 395);
            btnCancel.Size = new Size(80, 35);
            btnCancel.BackColor = Color.FromArgb(108, 117, 125);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Arial", 10, FontStyle.Bold);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.DialogResult = DialogResult.Cancel;

            // Добавляем все элементы
            groupFormat.Controls.Add(rbTable);
            groupFormat.Controls.Add(rbColumn);
            groupFormat.Controls.Add(rbList);
            groupFormat.Controls.Add(rbJsonLike);
            groupFormat.Controls.Add(rbDetailed);
            
            panelPreview.Controls.Add(lblPreview);
            
            this.Controls.Add(groupFormat);
            this.Controls.Add(panelPreview);
            this.Controls.Add(chkIncludeHeader);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        /// <summary>
        /// СОЗДАНИЕ РАДИОКНОПКИ
        /// </summary>
        private RadioButton CreateRadioButton(string text, int x, ref int y)
        {
            var rb = new RadioButton();
            rb.Text = text;
            rb.Location = new Point(x, y);
            rb.Size = new Size(450, 25);
            rb.Font = new Font("Arial", 10);
            rb.ForeColor = Color.FromArgb(64, 64, 64);
            y += 30;
            return rb;
        }

        /// <summary>
        /// ОБНОВЛЕНИЕ ПРЕДПРОСМОТРА
        /// </summary>
        private void UpdatePreview()
        {
            // Создаем тестовые данные
            var testData = new[]
            {
                new DataBD("Motor1", "LoopA", "Двигатель 1"),
                new DataBD("Pump2", "LoopB", "Насос 2"),
                new DataBD("Valve3", "LoopC", "Клапан 3")
            };

            // Генерируем предпросмотр в зависимости от формата
            string preview = SelectedFormat switch
            {
                TxtExportService.ExportFormat.Table => 
                    "Формат: Табличный (с разделителями)\n" +
                    "1\t| Motor1\t| LoopA\t| Двигатель 1\t| 01.01.2024 12:00\n" +
                    "2\t| Pump2\t\t| LoopB\t| Насос 2\t\t| 01.01.2024 12:00\n" +
                    "3\t| Valve3\t| LoopC\t| Клапан 3\t\t| 01.01.2024 12:00",

                TxtExportService.ExportFormat.Column =>
                    "Формат: Столбчатый (с выравниванием)\n" +
                    "ID  Tagname        Loop        Comment             Дата\n" +
                    "1   Motor1         LoopA       Двигатель 1         01.01.2024 12:00\n" +
                    "2   Pump2          LoopB       Насос 2             01.01.2024 12:00\n" +
                    "3   Valve3         LoopC       Клапан 3            01.01.2024 12:00",

                TxtExportService.ExportFormat.List =>
                    "Формат: Список с маркерами\n" +
                    "1. Motor1 - LoopA\n" +
                    "   Комментарий: Двигатель 1\n" +
                    "2. Pump2 - LoopB\n" +
                    "   Комментарий: Насос 2\n" +
                    "3. Valve3 - LoopC\n" +
                    "   Комментарий: Клапан 3",

                TxtExportService.ExportFormat.JsonLike =>
                    "Формат: JSON-подобный\n" +
                    "{\n" +
                    "  \"data\": [\n" +
                    "    {\"id\": 1, \"tagname\": \"Motor1\", \"loop\": \"LoopA\"},\n" +
                    "    {\"id\": 2, \"tagname\": \"Pump2\", \"loop\": \"LoopB\"},\n" +
                    "    {\"id\": 3, \"tagname\": \"Valve3\", \"loop\": \"LoopC\"}\n" +
                    "  ]\n" +
                    "}",

                TxtExportService.ExportFormat.Detailed =>
                    "Формат: Подробный (полное описание)\n" +
                    "┌─ ЗАПИСЬ #1 ─────────────────────────────────────┐\n" +
                    "│  Имя тега:    Motor1                          │\n" +
                    "│  Цикл:        LoopA                           │\n" +
                    "│  Комментарий: Двигатель 1                     │\n" +
                    "└────────────────────────────────────────────────┘",

                _ => "Неизвестный формат"
            };

            lblPreview.Text = preview;
        }
    }
}