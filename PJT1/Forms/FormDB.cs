// ================================================================
// ПРОСТРАНСТВА ИМЕН
// ================================================================
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PJT1.Models;
using PJT1.Repositories;
using PJT1.Services;

namespace PJT1.Forms
{
    /// <summary>
    /// КЛАСС FormDB - ГЛАВНАЯ ФОРМА ПРИЛОЖЕНИЯ
    /// 
    /// Это основное окно приложения, которое содержит:
    /// - Меню для управления данными
    /// - Таблицу для отображения данных
    /// - Панель кнопок быстрого доступа
    /// - Строку состояния
    /// 
    /// public partial class FormDB : Form
    /// FormDB наследует от Form (базовый класс Windows Forms)
    /// </summary>
    public partial class FormDB : Form
    {
        // ============================================================
        // ПОЛЯ КЛАССА
        // ============================================================

        /// <summary>
        /// РЕПОЗИТОРИЙ ДАННЫХ
        /// Хранит все данные и предоставляет методы для работы с ними
        /// </summary>
        private readonly DataBDRepository _repository;

        /// <summary>
        /// КОМПОНЕНТЫ ФОРМЫ
        /// Все элементы управления, которые будут на форме
        /// </summary>
        private MenuStrip mainMenu;
        private DataGridView dataGridView;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Panel buttonPanel;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnClear;
        private Button btnRefresh;
        private Button btnSave;

        // ============================================================
        // КОНСТРУКТОР ФОРМЫ
        // ============================================================

        /// <summary>
        /// КОНСТРУКТОР FormDB
        /// 
        /// Инициализирует форму, создает репозиторий
        /// и настраивает все элементы управления
        /// </summary>
        public FormDB()
        {
            // Создаем репозиторий для хранения данных
            _repository = new DataBDRepository();

            // Настраиваем внешний вид формы
            InitializeForm();

            // Создаем все элементы управления
            InitializeMenu();
            InitializeDataGridView();
            InitializeButtonPanel();
            InitializeStatusStrip();

            // Добавляем элементы на форму
            this.Controls.Add(mainMenu);
            this.Controls.Add(buttonPanel);
            this.Controls.Add(dataGridView);
            this.Controls.Add(statusStrip);

            // Загружаем тестовые данные для демонстрации
            LoadTestData();

            // Обновляем отображение данных
            RefreshDataGridView();
        }

        // ============================================================
        // НАСТРОЙКА ФОРМЫ
        // ============================================================

        private void InitializeForm()
        {
            this.Text = "PJT1 - Управление данными";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.MinimumSize = new Size(800, 500);
            this.FormClosing += FormDB_FormClosing;
        }

        // ============================================================
        // ГЛАВНОЕ МЕНЮ
        // ============================================================

        private void InitializeMenu()
        {
            mainMenu = new MenuStrip();

            // ============================================================
            // ПУНКТ МЕНЮ "ФАЙЛ"
            // ============================================================

            var fileMenu = new ToolStripMenuItem("&Файл");

            // Подпункт "Открыть Excel файл"
            var openExcelItem = new ToolStripMenuItem(
                "&Открыть Excel файл", 
                null, 
                OpenExcelMenuItem_Click
            );
            openExcelItem.ShortcutKeys = Keys.Control | Keys.O;
            openExcelItem.ShortcutKeyDisplayString = "Ctrl+O";

            // Подпункт "Сохранить в TXT"
            var saveTxtItem = new ToolStripMenuItem(
                "&Сохранить в TXT", 
                null, 
                SaveTxtMenuItem_Click
            );
            saveTxtItem.ShortcutKeys = Keys.Control | Keys.S;
            saveTxtItem.ShortcutKeyDisplayString = "Ctrl+S";

            // Подпункт "Выход"
            var exitItem = new ToolStripMenuItem("&Выход", null, ExitMenuItem_Click);
            exitItem.ShortcutKeys = Keys.Alt | Keys.F4;

            // Добавляем подпункты
            fileMenu.DropDownItems.Add(openExcelItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(saveTxtItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitItem);

            // ============================================================
            // ПУНКТ МЕНЮ "ДАННЫЕ"
            // ============================================================

            var dataMenu = new ToolStripMenuItem("&Данные");

            var addItem = new ToolStripMenuItem("&Добавить запись", null, AddMenuItem_Click);
            addItem.ShortcutKeys = Keys.Control | Keys.N;

            var deleteItem = new ToolStripMenuItem("&Удалить запись", null, DeleteMenuItem_Click);
            deleteItem.ShortcutKeys = Keys.Delete;

            var clearItem = new ToolStripMenuItem("&Очистить все", null, ClearMenuItem_Click);

            dataMenu.DropDownItems.Add(addItem);
            dataMenu.DropDownItems.Add(deleteItem);
            dataMenu.DropDownItems.Add(new ToolStripSeparator());
            dataMenu.DropDownItems.Add(clearItem);

            // ============================================================
            // ПУНКТ МЕНЮ "СПРАВКА"
            // ============================================================

            var helpMenu = new ToolStripMenuItem("&Справка");
            var aboutItem = new ToolStripMenuItem("&О программе", null, AboutMenuItem_Click);
            helpMenu.DropDownItems.Add(aboutItem);

            // ============================================================
            // ДОБАВЛЯЕМ ВСЕ МЕНЮ
            // ============================================================

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(dataMenu);
            mainMenu.Items.Add(helpMenu);

            mainMenu.Dock = DockStyle.Top;
            mainMenu.BackColor = Color.FromArgb(240, 240, 240);
        }

        // ============================================================
        // ТАБЛИЦА ДЛЯ ОТОБРАЖЕНИЯ ДАННЫХ
        // ============================================================

        private void InitializeDataGridView()
        {
            dataGridView = new DataGridView();

            // Настраиваем внешний вид
            dataGridView.BackgroundColor = Color.White;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.RowHeadersVisible = true;
            dataGridView.RowHeadersWidth = 40;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ReadOnly = true;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView.MultiSelect = false;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Расположение
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.Top = 60;

            // Привязываем изменение размера
            this.Resize += (s, e) =>
            {
                dataGridView.Width = this.ClientSize.Width;
                dataGridView.Height = this.ClientSize.Height - 120;
            };

            // Создаем столбцы
            CreateColumns();
        }

        private void CreateColumns()
        {
            dataGridView.Columns.Clear();

            // Столбец "ID"
            var idColumn = new DataGridViewTextBoxColumn();
            idColumn.HeaderText = "ID";
            idColumn.Name = "Id";
            idColumn.Width = 50;
            idColumn.ReadOnly = true;

            // Столбец "Tagname"
            var tagnameColumn = new DataGridViewTextBoxColumn();
            tagnameColumn.HeaderText = "Имя тега";
            tagnameColumn.Name = "Tagname";
            tagnameColumn.Width = 200;
            tagnameColumn.ReadOnly = true;

            // Столбец "Loop"
            var loopColumn = new DataGridViewTextBoxColumn();
            loopColumn.HeaderText = "Цикл";
            loopColumn.Name = "Loop";
            loopColumn.Width = 150;
            loopColumn.ReadOnly = true;

            // Столбец "Comment"
            var commentColumn = new DataGridViewTextBoxColumn();
            commentColumn.HeaderText = "Комментарий";
            commentColumn.Name = "Comment";
            commentColumn.Width = 250;
            commentColumn.ReadOnly = true;

            // Столбец "Дата создания"
            var dateColumn = new DataGridViewTextBoxColumn();
            dateColumn.HeaderText = "Дата создания";
            dateColumn.Name = "CreatedDate";
            dateColumn.Width = 150;
            dateColumn.ReadOnly = true;

            dataGridView.Columns.Add(idColumn);
            dataGridView.Columns.Add(tagnameColumn);
            dataGridView.Columns.Add(loopColumn);
            dataGridView.Columns.Add(commentColumn);
            dataGridView.Columns.Add(dateColumn);
        }

        // ============================================================
        // ПАНЕЛЬ КНОПОК
        // ============================================================

        private void InitializeButtonPanel()
        {
            buttonPanel = new Panel();
            buttonPanel.Height = 50;
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.BackColor = Color.FromArgb(240, 240, 240);
            buttonPanel.Padding = new Padding(10);

            // Кнопка "Добавить"
            btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Size = new Size(120, 35);
            btnAdd.Location = new Point(10, 8);
            btnAdd.BackColor = Color.FromArgb(0, 123, 255);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += AddMenuItem_Click;

            // Кнопка "Удалить"
            btnDelete = new Button();
            btnDelete.Text = "🗑️ Удалить";
            btnDelete.Size = new Size(120, 35);
            btnDelete.Location = new Point(140, 8);
            btnDelete.BackColor = Color.FromArgb(220, 53, 69);
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Click += DeleteMenuItem_Click;

            // Кнопка "Очистить все"
            btnClear = new Button();
            btnClear.Text = "🧹 Очистить все";
            btnClear.Size = new Size(120, 35);
            btnClear.Location = new Point(270, 8);
            btnClear.BackColor = Color.FromArgb(108, 117, 125);
            btnClear.ForeColor = Color.White;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Cursor = Cursors.Hand;
            btnClear.Click += ClearMenuItem_Click;

            // Кнопка "Обновить"
            btnRefresh = new Button();
            btnRefresh.Text = "🔄 Обновить";
            btnRefresh.Size = new Size(120, 35);
            btnRefresh.Location = new Point(400, 8);
            btnRefresh.BackColor = Color.FromArgb(40, 167, 69);
            btnRefresh.ForeColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Cursor = Cursors.Hand;
            btnRefresh.Click += RefreshMenuItem_Click;

            // Кнопка "Сохранить"
            btnSave = new Button();
            btnSave.Text = "💾 Сохранить";
            btnSave.Size = new Size(120, 35);
            btnSave.Location = new Point(530, 8);
            btnSave.BackColor = Color.FromArgb(23, 162, 184);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += SaveTxtMenuItem_Click;

            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnClear);
            buttonPanel.Controls.Add(btnRefresh);
            buttonPanel.Controls.Add(btnSave);

            // Информационная метка
            var infoLabel = new Label();
            infoLabel.Text = "Выберите запись для удаления";
            infoLabel.AutoSize = true;
            infoLabel.Location = new Point(670, 15);
            infoLabel.ForeColor = Color.Gray;
            buttonPanel.Controls.Add(infoLabel);
        }

        // ============================================================
        // СТРОКА СОСТОЯНИЯ
        // ============================================================

        private void InitializeStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(240, 240, 240);

            statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "Готов к работе";
            statusLabel.Spring = true;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(new ToolStripSeparator());

            var timeLabel = new ToolStripStatusLabel();
            timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            timeLabel.TextAlign = ContentAlignment.MiddleRight;

            var timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();

            statusStrip.Items.Add(timeLabel);
        }

        // ============================================================
        // ЗАГРУЗКА ТЕСТОВЫХ ДАННЫХ
        // ============================================================

        private void LoadTestData()
        {
            _repository.Add(new DataBD("Motor1", "LoopA", "Двигатель 1"));
            _repository.Add(new DataBD("Pump2", "LoopB", "Насос 2"));
            _repository.Add(new DataBD("Valve3", "LoopC", "Клапан 3"));
            _repository.Add(new DataBD("Sensor4", "LoopD", "Датчик 4"));
            _repository.Add(new DataBD("Actuator5", "LoopE", "Исполнительное устройство 5"));
        }

        // ============================================================
        // ОБНОВЛЕНИЕ ТАБЛИЦЫ
        // ============================================================

        private void RefreshDataGridView()
        {
            dataGridView.Rows.Clear();

            var dataList = _repository.GetAll();

            if (!dataList.Any())
            {
                statusLabel.Text = "Нет данных. Добавьте записи или импортируйте из Excel.";
                return;
            }

            foreach (var data in dataList)
            {
                dataGridView.Rows.Add(
                    data.Id,
                    data.Tagname,
                    data.Loop,
                    data.Comment,
                    data.CreatedDate.ToString("dd.MM.yyyy HH:mm")
                );
            }

            statusLabel.Text = $"Всего записей: {_repository.Count()}";
        }

        // ============================================================
        // ОБРАБОТЧИКИ СОБЫТИЙ
        // ============================================================

        /// <summary>
        /// ОТКРЫТЬ EXCEL ФАЙЛ
        /// </summary>
        private async void OpenExcelMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Выберите Excel файл";
                openFileDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        this.Enabled = false;
                        statusLabel.Text = "Импорт данных из Excel...";

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            _repository.ImportFromExcel(openFileDialog.FileName);
                        });

                        RefreshDataGridView();

                        MessageBox.Show(
                            $"Данные успешно импортированы!\nВсего записей: {_repository.Count()}",
                            "Успех",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );

                        statusLabel.Text = $"Импорт завершен. Всего записей: {_repository.Count()}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Ошибка при импорте данных:\n{DescribeError(ex)}",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        statusLabel.Text = "Ошибка импорта данных";
                    }
                    finally
                    {
                        this.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// ДОБАВЛЕНИЕ ЗАПИСИ
        /// </summary>
        private void AddMenuItem_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddRecordForm())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    var newData = new DataBD(
                        addForm.Tagname,
                        addForm.Loop,
                        addForm.Comment
                    );

                    try
                    {
                        _repository.Add(newData);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Не удалось добавить запись:\n{DescribeError(ex)}",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        statusLabel.Text = "Ошибка добавления записи";
                        return;
                    }

                    RefreshDataGridView();
                    statusLabel.Text = $"Добавлена запись: {newData.Tagname} - {newData.Loop}";
                }
            }
        }

        /// <summary>
        /// УДАЛЕНИЕ ЗАПИСИ
        /// </summary>
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Пожалуйста, выберите запись для удаления.",
                    "Предупреждение",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var idCell = dataGridView.SelectedRows[0].Cells["Id"].Value;
            if (idCell == null || !int.TryParse(idCell.ToString(), out int id))
            {
                MessageBox.Show(
                    "Не удалось определить идентификатор выбранной записи.",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                statusLabel.Text = "Ошибка удаления: некорректный ID";
                return;
            }

            var data = _repository.GetById(id);
            if (data == null)
            {
                MessageBox.Show(
                    $"Запись с ID {id} не найдена. Возможно, таблица устарела.",
                    "Запись не найдена",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                RefreshDataGridView();
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись:\n{data.Tagname} - {data.Loop}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                _repository.Delete(id);
                RefreshDataGridView();
                statusLabel.Text = $"Удалена запись: {data.Tagname} - {data.Loop}";
            }
        }

        /// <summary>
        /// ОЧИСТКА ВСЕХ ДАННЫХ
        /// </summary>
        private void ClearMenuItem_Click(object sender, EventArgs e)
        {
            if (_repository.Count() == 0)
            {
                MessageBox.Show(
                    "В репозитории нет данных для очистки.",
                    "Информация",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить все {_repository.Count()} записей?",
                "Подтверждение очистки",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                _repository.Clear();
                RefreshDataGridView();
                statusLabel.Text = "Все данные очищены";
            }
        }

        /// <summary>
        /// ОБНОВЛЕНИЕ ДАННЫХ
        /// </summary>
        private void RefreshMenuItem_Click(object sender, EventArgs e)
        {
            RefreshDataGridView();
            statusLabel.Text = "Данные обновлены";
        }

        /// <summary>
        /// СОХРАНЕНИЕ В TXT
        /// </summary>
        private void SaveTxtMenuItem_Click(object sender, EventArgs e)
        {
            if (_repository.Count() == 0)
            {
                MessageBox.Show(
                    "В репозитории нет данных для экспорта.\n" +
                    "Сначала добавьте данные или импортируйте из Excel.",
                    "Нет данных",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                var dataList = _repository.GetAll();
                bool success = TxtExportService.ExportWithDialog(dataList, "PJT1_Data");

                if (success)
                {
                    statusLabel.Text = $"Данные экспортированы. Всего записей: {_repository.Count()}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при экспорте данных:\n{DescribeError(ex)}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                statusLabel.Text = "Ошибка экспорта данных";
            }
        }

        /// <summary>
        /// СООБЩЕНИЕ ОБ ОШИБКЕ
        /// Разворачивает цепочку InnerException, чтобы не терять первопричину
        /// </summary>
        private static string DescribeError(Exception ex)
        {
            var sb = new System.Text.StringBuilder(ex.Message);
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                sb.Append("\n→ ").Append(inner.Message);
            }
            return sb.ToString();
        }

        /// <summary>
        /// О ПРОГРАММЕ
        /// </summary>
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "PJT1 - Приложение для управления данными\n" +
                "Версия: 2.0\n" +
                "Функции:\n" +
                "- Импорт из Excel\n" +
                "- Добавление записей\n" +
                "- Удаление записей\n" +
                "- Очистка данных\n" +
                "- Экспорт в TXT (5 форматов)\n" +
                "© 2024 Все права защищены",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// ВЫХОД
        /// </summary>
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// ЗАКРЫТИЕ ФОРМЫ
        /// </summary>
        private void FormDB_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти?",
                "Выход из программы",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}