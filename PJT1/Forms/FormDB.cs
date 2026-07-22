// ================================================================
// ПОДКЛЮЧЕНИЕ ПРОСТРАНСТВ ИМЕН
// ================================================================
using System;
using System.Drawing;           // Для работы с графикой (размеры, цвета)
using System.Windows.Forms;    // Windows Forms элементы
using System.Linq;             // LINQ запросы
using PJT1.Models;             // Класс DataBD
using PJT1.Repositories;       // Репозиторий
using PJT1.Services;           // Excel сервис

namespace PJT1.Forms
{
    /// <summary>
    /// КЛАСС FormDB - ГЛАВНАЯ ФОРМА ПРИЛОЖЕНИЯ
    /// 
    /// Это основное окно приложения, которое содержит:
    /// - Меню для управления данными
    /// - Таблицу для отображения данных
    /// - Информационную панель
    /// 
    /// Наследование: public partial class FormDB : Form
    /// FormDB наследует от Form (базовый класс Windows Forms)
    /// </summary>
    public partial class FormDB : Form
    {
        // ============================================================
        // ПОЛЯ КЛАССА
        // ============================================================

        /// <summary>
        /// РЕПОЗИТОРИЙ ДАННЫХ
        /// 
        /// Хранит все данные и предоставляет методы для работы с ними
        /// </summary>
        private readonly DataBDRepository _repository;

        /// <summary>
        /// КОМПОНЕНТЫ ФОРМЫ
        /// 
        /// Здесь мы объявляем все элементы управления,
        /// которые будут на форме
        /// </summary>
        private MenuStrip mainMenu;          // Главное меню
        private DataGridView dataGridView;   // Таблица для данных
        private StatusStrip statusStrip;     // Строка состояния
        private ToolStripStatusLabel statusLabel; // Метка в строке состояния
        private Panel buttonPanel;           // Панель для кнопок
        private Button btnAdd;               // Кнопка "Добавить"
        private Button btnDelete;            // Кнопка "Удалить"
        private Button btnClear;             // Кнопка "Очистить"
        private Button btnRefresh;           // Кнопка "Обновить"

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

        /// <summary>
        /// НАСТРОЙКА ВНЕШНЕГО ВИДА ФОРМЫ
        /// 
        /// Устанавливает размер, заголовок, иконку и другие параметры
        /// </summary>
        private void InitializeForm()
        {
            // Заголовок окна
            this.Text = "PJT1 - Управление данными";

            // Размер окна (ширина, высота)
            this.Size = new Size(900, 600);

            // Начальное положение - центр экрана
            this.StartPosition = FormStartPosition.CenterScreen;

            // Цвет фона формы
            this.BackColor = Color.White;

            // Минимальный размер окна
            this.MinimumSize = new Size(800, 500);

            // Форма не должна быть максимизирована при запуске
            this.WindowState = FormWindowState.Normal;

            // Форма может изменять размер
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // Обработчик закрытия формы
            this.FormClosing += FormDB_FormClosing;
        }

        // ============================================================
        // ГЛАВНОЕ МЕНЮ
        // ============================================================

        /// <summary>
        /// СОЗДАНИЕ ГЛАВНОГО МЕНЮ
        /// 
        /// Меню содержит пункты:
        /// - Файл: Открыть Excel, Выход
        /// - Данные: Добавить, Удалить, Очистить
        /// - Справка: О программе
        /// </summary>
        private void InitializeMenu()
        {
            // Создаем главное меню
            mainMenu = new MenuStrip();

            // ============================================================
            // ПУНКТ МЕНЮ "ФАЙЛ"
            // ============================================================

            // Создаем пункт меню "Файл"
            var fileMenu = new ToolStripMenuItem("&Файл");

            // Подпункт "Открыть Excel файл"
            var openExcelItem = new ToolStripMenuItem("&Открыть Excel файл", null, OpenExcelMenuItem_Click);
            openExcelItem.ShortcutKeys = Keys.Control | Keys.O; // Ctrl+O
            openExcelItem.ShortcutKeyDisplayString = "Ctrl+O";

            // Подпункт "Выход"
            var exitItem = new ToolStripMenuItem("&Выход", null, ExitMenuItem_Click);
            exitItem.ShortcutKeys = Keys.Alt | Keys.F4; // Alt+F4

            // Добавляем подпункты в меню "Файл"
            fileMenu.DropDownItems.Add(openExcelItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator()); // Разделитель
            fileMenu.DropDownItems.Add(exitItem);

            // ============================================================
            // ПУНКТ МЕНЮ "ДАННЫЕ"
            // ============================================================

            // Создаем пункт меню "Данные"
            var dataMenu = new ToolStripMenuItem("&Данные");

            // Подпункт "Добавить запись"
            var addItem = new ToolStripMenuItem("&Добавить запись", null, AddMenuItem_Click);
            addItem.ShortcutKeys = Keys.Control | Keys.N; // Ctrl+N

            // Подпункт "Удалить запись"
            var deleteItem = new ToolStripMenuItem("&Удалить запись", null, DeleteMenuItem_Click);
            deleteItem.ShortcutKeys = Keys.Delete; // Delete

            // Подпункт "Очистить все"
            var clearItem = new ToolStripMenuItem("&Очистить все", null, ClearMenuItem_Click);

            // Добавляем подпункты в меню "Данные"
            dataMenu.DropDownItems.Add(addItem);
            dataMenu.DropDownItems.Add(deleteItem);
            dataMenu.DropDownItems.Add(new ToolStripSeparator());
            dataMenu.DropDownItems.Add(clearItem);

            // ============================================================
            // ПУНКТ МЕНЮ "СПРАВКА"
            // ============================================================

            // Создаем пункт меню "Справка"
            var helpMenu = new ToolStripMenuItem("&Справка");

            // Подпункт "О программе"
            var aboutItem = new ToolStripMenuItem("&О программе", null, AboutMenuItem_Click);

            // Добавляем подпункты в меню "Справка"
            helpMenu.DropDownItems.Add(aboutItem);

            // ============================================================
            // ДОБАВЛЯЕМ МЕНЮ В ГЛАВНОЕ МЕНЮ
            // ============================================================

            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(dataMenu);
            mainMenu.Items.Add(helpMenu);

            // Расположение меню
            mainMenu.Dock = DockStyle.Top; // Прикрепляем к верхней части

            // Цвет фона меню
            mainMenu.BackColor = Color.FromArgb(240, 240, 240);
        }

        // ============================================================
        // ТАБЛИЦА ДЛЯ ОТОБРАЖЕНИЯ ДАННЫХ
        // ============================================================

        /// <summary>
        /// СОЗДАНИЕ И НАСТРОЙКА ТАБЛИЦЫ DataGridView
        /// 
        /// DataGridView - мощный элемент для отображения табличных данных
        /// </summary>
        private void InitializeDataGridView()
        {
            // Создаем таблицу
            dataGridView = new DataGridView();

            // Настраиваем внешний вид
            dataGridView.BackgroundColor = Color.White;
            dataGridView.BorderStyle = BorderStyle.Fixed3D;
            dataGridView.RowHeadersVisible = true;
            dataGridView.RowHeadersWidth = 40;
            dataGridView.AllowUserToAddRows = false; // Запрещаем добавление строк
            dataGridView.AllowUserToDeleteRows = false; // Запрещаем удаление
            dataGridView.ReadOnly = true; // Только для чтения
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выбор всей строки
            dataGridView.MultiSelect = false; // Запрещаем выбор нескольких строк
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоматический размер

            // Настраиваем расположение (будет между меню и строкой состояния)
            dataGridView.Top = 60; // Отступ сверху (после меню)
            dataGridView.Left = 0;
            dataGridView.Width = this.ClientSize.Width;
            dataGridView.Height = this.ClientSize.Height - 120; // Оставляем место для панели кнопок и статуса

            // Привязываем изменение размера формы
            this.Resize += (s, e) =>
            {
                dataGridView.Width = this.ClientSize.Width;
                dataGridView.Height = this.ClientSize.Height - 120;
            };

            // Создаем столбцы
            CreateColumns();
        }

        /// <summary>
        /// СОЗДАНИЕ СТОЛБЦОВ ТАБЛИЦЫ
        /// 
        /// Определяем структуру таблицы
        /// </summary>
        private void CreateColumns()
        {
            // Очищаем существующие столбцы
            dataGridView.Columns.Clear();

            // Создаем столбец "ID"
            var idColumn = new DataGridViewTextBoxColumn();
            idColumn.HeaderText = "ID";
            idColumn.Name = "Id";
            idColumn.Width = 50;
            idColumn.ReadOnly = true;

            // Создаем столбец "Tagname"
            var tagnameColumn = new DataGridViewTextBoxColumn();
            tagnameColumn.HeaderText = "Имя тега";
            tagnameColumn.Name = "Tagname";
            tagnameColumn.Width = 200;
            tagnameColumn.ReadOnly = true;

            // Создаем столбец "Loop"
            var loopColumn = new DataGridViewTextBoxColumn();
            loopColumn.HeaderText = "Цикл";
            loopColumn.Name = "Loop";
            loopColumn.Width = 150;
            loopColumn.ReadOnly = true;

            // Создаем столбец "Comment"
            var commentColumn = new DataGridViewTextBoxColumn();
            commentColumn.HeaderText = "Комментарий";
            commentColumn.Name = "Comment";
            commentColumn.Width = 250;
            commentColumn.ReadOnly = true;

            // Создаем столбец "Дата создания"
            var dateColumn = new DataGridViewTextBoxColumn();
            dateColumn.HeaderText = "Дата создания";
            dateColumn.Name = "CreatedDate";
            dateColumn.Width = 150;
            dateColumn.ReadOnly = true;

            // Добавляем столбцы в таблицу
            dataGridView.Columns.Add(idColumn);
            dataGridView.Columns.Add(tagnameColumn);
            dataGridView.Columns.Add(loopColumn);
            dataGridView.Columns.Add(commentColumn);
            dataGridView.Columns.Add(dateColumn);
        }

        // ============================================================
        // ПАНЕЛЬ КНОПОК
        // ============================================================

        /// <summary>
        /// СОЗДАНИЕ ПАНЕЛИ С КНОПКАМИ
        /// 
        /// Добавляем кнопки для быстрого доступа к основным функциям
        /// </summary>
        private void InitializeButtonPanel()
        {
            // Создаем панель для кнопок
            buttonPanel = new Panel();
            buttonPanel.Height = 50;
            buttonPanel.Dock = DockStyle.Bottom; // Прикрепляем к нижней части
            buttonPanel.BackColor = Color.FromArgb(240, 240, 240);
            buttonPanel.Padding = new Padding(10);

            // ============================================================
            // КНОПКА "ДОБАВИТЬ"
            // ============================================================

            btnAdd = new Button();
            btnAdd.Text = "➕ Добавить";
            btnAdd.Size = new Size(120, 35);
            btnAdd.Location = new Point(10, 8);
            btnAdd.BackColor = Color.FromArgb(0, 123, 255);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += AddMenuItem_Click; // Обработчик клика

            // ============================================================
            // КНОПКА "УДАЛИТЬ"
            // ============================================================

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

            // ============================================================
            // КНОПКА "ОЧИСТИТЬ ВСЕ"
            // ============================================================

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

            // ============================================================
            // КНОПКА "ОБНОВИТЬ"
            // ============================================================

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

            // Добавляем кнопки на панель
            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnClear);
            buttonPanel.Controls.Add(btnRefresh);

            // Информационная метка на панели
            var infoLabel = new Label();
            infoLabel.Text = "Выберите запись для удаления";
            infoLabel.AutoSize = true;
            infoLabel.Location = new Point(540, 15);
            infoLabel.ForeColor = Color.Gray;
            buttonPanel.Controls.Add(infoLabel);
        }

        // ============================================================
        // СТРОКА СОСТОЯНИЯ
        // ============================================================

        /// <summary>
        /// СОЗДАНИЕ СТРОКИ СОСТОЯНИЯ
        /// 
        /// Отображает информацию о количестве записей
        /// </summary>
        private void InitializeStatusStrip()
        {
            // Создаем строку состояния
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(240, 240, 240);

            // Создаем метку для отображения информации
            statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "Готов к работе";
            statusLabel.Spring = true; // Занимает все свободное место
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Добавляем метку в строку состояния
            statusStrip.Items.Add(statusLabel);

            // Добавляем разделитель
            statusStrip.Items.Add(new ToolStripSeparator());

            // Добавляем информацию о времени
            var timeLabel = new ToolStripStatusLabel();
            timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            timeLabel.TextAlign = ContentAlignment.MiddleRight;

            // Обновляем время каждую секунду
            var timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();

            statusStrip.Items.Add(timeLabel);
        }

        // ============================================================
        // ЗАГРУЗКА ТЕСТОВЫХ ДАННЫХ
        // ============================================================

        /// <summary>
        /// ЗАГРУЗКА ТЕСТОВЫХ ДАННЫХ
        /// 
        /// Добавляет примеры данных для демонстрации работы приложения
        /// </summary>
        private void LoadTestData()
        {
            // Добавляем тестовые данные
            _repository.Add(new DataBD("Motor1", "LoopA", "Двигатель 1"));
            _repository.Add(new DataBD("Pump2", "LoopB", "Насос 2"));
            _repository.Add(new DataBD("Valve3", "LoopC", "Клапан 3"));
            _repository.Add(new DataBD("Sensor4", "LoopD", "Датчик 4"));
            _repository.Add(new DataBD("Actuator5", "LoopE", "Исполнительное устройство 5"));
        }

        // ============================================================
        // ОБНОВЛЕНИЕ ТАБЛИЦЫ
        // ============================================================

        /// <summary>
        /// ОБНОВЛЕНИЕ ОТОБРАЖЕНИЯ ДАННЫХ
        /// 
        /// Очищает таблицу и заполняет её данными из репозитория
        /// </summary>
        private void RefreshDataGridView()
        {
            // Очищаем строки таблицы
            dataGridView.Rows.Clear();

            // Получаем все данные из репозитория
            var dataList = _repository.GetAll();

            // Если данных нет - выводим сообщение
            if (!dataList.Any())
            {
                statusLabel.Text = "Нет данных. Добавьте записи или импортируйте из Excel.";
                return;
            }

            // Добавляем каждую запись в таблицу
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

            // Обновляем строку состояния
            statusLabel.Text = $"Всего записей: {_repository.Count()}";
        }

        // ============================================================
        // ОБРАБОТЧИКИ СОБЫТИЙ МЕНЮ
        // ============================================================

        /// <summary>
        /// ОТКРЫТЬ EXCEL ФАЙЛ
        /// 
        /// Открывает диалог выбора файла и импортирует данные
        /// </summary>
        private async void OpenExcelMenuItem_Click(object sender, EventArgs e)
        {
            // Создаем диалог открытия файла
            using (var openFileDialog = new OpenFileDialog())
            {
                // Настраиваем диалог
                openFileDialog.Title = "Выберите Excel файл";
                openFileDialog.Filter = "Excel файлы (*.xlsx)|*.xlsx|Все файлы (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                // Показываем диалог и проверяем результат
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Блокируем интерфейс на время импорта
                        this.Enabled = false;
                        statusLabel.Text = "Импорт данных из Excel...";

                        // Импортируем данные (асинхронно)
                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            _repository.ImportFromExcel(openFileDialog.FileName);
                        });

                        // Обновляем отображение
                        RefreshDataGridView();

                        // Показываем сообщение об успехе
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
                        // Обрабатываем ошибку
                        MessageBox.Show(
                            $"Ошибка при импорте данных:\n{ex.Message}",
                            "Ошибка",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        statusLabel.Text = "Ошибка импорта данных";
                    }
                    finally
                    {
                        // Разблокируем интерфейс
                        this.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// ДОБАВЛЕНИЕ ЗАПИСИ
        /// 
        /// Открывает диалог для ввода новой записи
        /// </summary>
        private void AddMenuItem_Click(object sender, EventArgs e)
        {
            // Создаем форму для ввода данных
            using (var addForm = new AddRecordForm())
            {
                // Показываем форму и проверяем результат
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    // Получаем данные из формы
                    var newData = new DataBD(
                        addForm.Tagname,
                        addForm.Loop,
                        addForm.Comment
                    );

                    // Добавляем в репозиторий
                    _repository.Add(newData);

                    // Обновляем таблицу
                    RefreshDataGridView();

                    // Показываем сообщение
                    statusLabel.Text = $"Добавлена запись: {newData.Tagname} - {newData.Loop}";
                }
            }
        }

        /// <summary>
        /// УДАЛЕНИЕ ЗАПИСИ
        /// 
        /// Удаляет выбранную запись из таблицы
        /// </summary>
        private void DeleteMenuItem_Click(object sender, EventArgs e)
        {
            // Проверяем, выбрана ли строка
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

            // Получаем ID выбранной записи
            int id = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["Id"].Value);

            // Находим запись в репозитории
            var data = _repository.GetById(id);

            // Запрашиваем подтверждение
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись:\n{data?.Tagname} - {data?.Loop}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Удаляем запись
                _repository.Delete(id);

                // Обновляем таблицу
                RefreshDataGridView();

                // Показываем сообщение
                statusLabel.Text = $"Удалена запись: {data?.Tagname} - {data?.Loop}";
            }
        }

        /// <summary>
        /// ОЧИСТКА ВСЕХ ДАННЫХ
        /// 
        /// Удаляет все записи из репозитория
        /// </summary>
        private void ClearMenuItem_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли данные
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

            // Запрашиваем подтверждение
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить все {_repository.Count()} записей?",
                "Подтверждение очистки",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                // Очищаем репозиторий
                _repository.Clear();

                // Обновляем таблицу
                RefreshDataGridView();

                // Показываем сообщение
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
        /// О ПРОГРАММЕ
        /// 
        /// Показывает информацию о программе
        /// </summary>
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "PJT1 - Приложение для управления данными\n" +
                "Версия: 1.0\n" +
                "Разработано: C# Windows Forms\n" +
                "Функции:\n" +
                "- Импорт из Excel\n" +
                "- Добавление записей\n" +
                "- Удаление записей\n" +
                "- Очистка данных\n" +
                "© 2024 Все права защищены",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// ВЫХОД ИЗ ПРОГРАММЫ
        /// </summary>
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            // Закрываем приложение
            this.Close();
        }

        /// <summary>
        /// ОБРАБОТЧИК ЗАКРЫТИЯ ФОРМЫ
        /// </summary>
        private void FormDB_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Запрашиваем подтверждение при закрытии
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти?",
                "Выход из программы",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                e.Cancel = true; // Отменяем закрытие
            }
        }
    }
}