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
using WinFormsKit;

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

        /// <summary>
        /// Размер кнопок панели быстрого доступа
        /// </summary>
        private static readonly Size ActionButtonSize = new Size(120, 35);

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
            var openExcelItem = ControlFactory.CreateMenuItem(
                "&Открыть Excel файл",
                OpenExcelMenuItem_Click,
                Keys.Control | Keys.O,
                "Ctrl+O"
            );

            // Подпункт "Сохранить в TXT"
            var saveTxtItem = ControlFactory.CreateMenuItem(
                "&Сохранить в TXT",
                SaveTxtMenuItem_Click,
                Keys.Control | Keys.S,
                "Ctrl+S"
            );

            // Подпункт "Выход"
            var exitItem = ControlFactory.CreateMenuItem(
                "&Выход",
                ExitMenuItem_Click,
                Keys.Alt | Keys.F4
            );

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

            var addItem = ControlFactory.CreateMenuItem(
                "&Добавить запись",
                AddMenuItem_Click,
                Keys.Control | Keys.N
            );

            var deleteItem = ControlFactory.CreateMenuItem(
                "&Удалить запись",
                DeleteMenuItem_Click,
                Keys.Delete
            );

            var clearItem = ControlFactory.CreateMenuItem("&Очистить все", ClearMenuItem_Click);

            dataMenu.DropDownItems.Add(addItem);
            dataMenu.DropDownItems.Add(deleteItem);
            dataMenu.DropDownItems.Add(new ToolStripSeparator());
            dataMenu.DropDownItems.Add(clearItem);

            // ============================================================
            // ПУНКТ МЕНЮ "СПРАВКА"
            // ============================================================

            var helpMenu = new ToolStripMenuItem("&Справка");
            var aboutItem = ControlFactory.CreateMenuItem("&О программе", AboutMenuItem_Click);
            helpMenu.DropDownItems.Add(aboutItem);

            // ============================================================
            // ДОБАВЛЯЕМ ВСЕ МЕНЮ
            // ============================================================

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(dataMenu);
            mainMenu.Items.Add(helpMenu);

            mainMenu.Dock = DockStyle.Top;
            mainMenu.BackColor = ControlFactory.Surface;
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

            dataGridView.Columns.Add(ControlFactory.CreateTextColumn("Id", "ID", 50));
            dataGridView.Columns.Add(ControlFactory.CreateTextColumn("Tagname", "Имя тега", 200));
            dataGridView.Columns.Add(ControlFactory.CreateTextColumn("Loop", "Цикл", 150));
            dataGridView.Columns.Add(ControlFactory.CreateTextColumn("Comment", "Комментарий", 250));
            dataGridView.Columns.Add(ControlFactory.CreateTextColumn("CreatedDate", "Дата создания", 150));
        }

        // ============================================================
        // ПАНЕЛЬ КНОПОК
        // ============================================================

        private void InitializeButtonPanel()
        {
            buttonPanel = new Panel();
            buttonPanel.Height = 50;
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.BackColor = ControlFactory.Surface;
            buttonPanel.Padding = new Padding(10);

            btnAdd = ControlFactory.CreateFlatButton(
                "➕ Добавить", new Point(10, 8), ActionButtonSize,
                ControlFactory.Primary, AddMenuItem_Click);

            btnDelete = ControlFactory.CreateFlatButton(
                "🗑️ Удалить", new Point(140, 8), ActionButtonSize,
                ControlFactory.Danger, DeleteMenuItem_Click);

            btnClear = ControlFactory.CreateFlatButton(
                "🧹 Очистить все", new Point(270, 8), ActionButtonSize,
                ControlFactory.Secondary, ClearMenuItem_Click);

            btnRefresh = ControlFactory.CreateFlatButton(
                "🔄 Обновить", new Point(400, 8), ActionButtonSize,
                ControlFactory.Success, RefreshMenuItem_Click);

            btnSave = ControlFactory.CreateFlatButton(
                "💾 Сохранить", new Point(530, 8), ActionButtonSize,
                ControlFactory.Accent, SaveTxtMenuItem_Click);

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
            statusStrip.BackColor = ControlFactory.Surface;

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

                        UserMessages.Info(
                            $"Данные успешно импортированы!\nВсего записей: {_repository.Count()}",
                            "Успех"
                        );

                        statusLabel.Text = $"Импорт завершен. Всего записей: {_repository.Count()}";
                    }
                    catch (Exception ex)
                    {
                        UserMessages.Error($"Ошибка при импорте данных:\n{ex.Message}");
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

                    _repository.Add(newData);
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
                UserMessages.Warning("Пожалуйста, выберите запись для удаления.");
                return;
            }

            int id = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["Id"].Value);
            var data = _repository.GetById(id);

            bool confirmed = UserMessages.Confirm(
                $"Вы уверены, что хотите удалить запись:\n{data?.Tagname} - {data?.Loop}?",
                "Подтверждение удаления"
            );

            if (confirmed)
            {
                _repository.Delete(id);
                RefreshDataGridView();
                statusLabel.Text = $"Удалена запись: {data?.Tagname} - {data?.Loop}";
            }
        }

        /// <summary>
        /// ОЧИСТКА ВСЕХ ДАННЫХ
        /// </summary>
        private void ClearMenuItem_Click(object sender, EventArgs e)
        {
            if (_repository.Count() == 0)
            {
                UserMessages.Info("В репозитории нет данных для очистки.");
                return;
            }

            bool confirmed = UserMessages.Confirm(
                $"Вы уверены, что хотите удалить все {_repository.Count()} записей?",
                "Подтверждение очистки",
                MessageBoxIcon.Warning
            );

            if (confirmed)
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
                UserMessages.Info(
                    "В репозитории нет данных для экспорта.\n" +
                    "Сначала добавьте данные или импортируйте из Excel.",
                    "Нет данных"
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
                UserMessages.Error($"Ошибка при экспорте данных:\n{ex.Message}");
                statusLabel.Text = "Ошибка экспорта данных";
            }
        }

        /// <summary>
        /// О ПРОГРАММЕ
        /// </summary>
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            UserMessages.Info(
                "PJT1 - Приложение для управления данными\n" +
                "Версия: 2.0\n" +
                "Функции:\n" +
                "- Импорт из Excel\n" +
                "- Добавление записей\n" +
                "- Удаление записей\n" +
                "- Очистка данных\n" +
                "- Экспорт в TXT (5 форматов)\n" +
                "© 2024 Все права защищены",
                "О программе"
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
            if (!UserMessages.Confirm("Вы уверены, что хотите выйти?", "Выход из программы"))
            {
                e.Cancel = true;
            }
        }
    }
}