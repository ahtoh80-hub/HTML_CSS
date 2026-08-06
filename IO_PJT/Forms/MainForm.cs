using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using IO_PJT.Services;
using IO_PJT.Utils;
using WinFormsKit;

namespace IO_PJT
{
    public class MainForm : Form
    {
        private TextBox txtDbPath = null!;
        private Button btnBrowse = null!;
        private Button btnCreateDb = null!;
        private TextBox txtTableName = null!;
        private Button btnCreateTable = null!;
        private Button btnInsertSample = null!;
        private RichTextBox txtLog = null!;
        private Label lblStatus = null!;
        private OpenFileDialog openFileDialog = null!;
        private Logger? _logger;

        public MainForm()
        {
            InitializeComponent();
            SetupDefaultPath();
        }

        private void InitializeComponent()
        {
            this.Text = "IO_PJT - Создание таблицы IoPoint в Access";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10);
            this.BackColor = DarkTheme.Background;
            this.ForeColor = DarkTheme.Text;

            // ----- Path Section -----
            var lblDbPath = ControlFactory.CreateLabel(
                "📁 Путь к базе данных Access (.mdb или .accdb):",
                new Point(20, 20), new Size(400, 25), DarkTheme.Text);

            txtDbPath = CreateDarkTextBox(new Point(20, 50), new Size(560, 25));

            btnBrowse = CreateDarkButton(
                "📂 Обзор", new Point(590, 48), new Size(85, 30),
                DarkTheme.NeutralButton, BtnBrowse_Click);

            btnCreateDb = CreateDarkButton(
                "🆕 Создать БД", new Point(685, 48), new Size(85, 30),
                DarkTheme.CreateDbButton, BtnCreateDb_Click);

            // ----- Table Name Section -----
            var lblTableName = ControlFactory.CreateLabel(
                "📝 Имя создаваемой таблицы:",
                new Point(20, 95), new Size(250, 25), DarkTheme.Text);

            txtTableName = CreateDarkTextBox(new Point(20, 125), new Size(300, 25), "IoPoint");

            // ----- Buttons -----
            btnCreateTable = CreateDarkButton(
                "⚡ Создать таблицу", new Point(20, 165), new Size(180, 40),
                DarkTheme.PrimaryButton, BtnCreateTable_Click,
                new Font("Segoe UI", 11, FontStyle.Bold));

            btnInsertSample = CreateDarkButton(
                "📥 Добавить тестовые данные", new Point(210, 165), new Size(200, 40),
                DarkTheme.SecondaryButton, BtnInsertSample_Click,
                new Font("Segoe UI", 10));

            // ----- Status Label -----
            lblStatus = ControlFactory.CreateLabel(
                "Готов к работе", new Point(20, 215), new Size(600, 25), DarkTheme.MutedText);

            // ----- Log Section -----
            var lblLog = ControlFactory.CreateLabel(
                "📋 Лог операций:", new Point(20, 250), new Size(150, 25), DarkTheme.Text);

            txtLog = new RichTextBox
            {
                Location = new Point(20, 280),
                Size = new Size(750, 360),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.None
            };

            // ----- OpenFileDialog -----
            openFileDialog = new OpenFileDialog
            {
                Filter = "Access Database (*.mdb;*.accdb)|*.mdb;*.accdb|All files (*.*)|*.*",
                Title = "Выберите базу данных Access"
            };

            // Add controls
            this.Controls.Add(lblDbPath);
            this.Controls.Add(txtDbPath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(btnCreateDb);
            this.Controls.Add(lblTableName);
            this.Controls.Add(txtTableName);
            this.Controls.Add(btnCreateTable);
            this.Controls.Add(btnInsertSample);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblLog);
            this.Controls.Add(txtLog);
        }

        /// <summary>
        /// Поле ввода в темном оформлении формы
        /// </summary>
        private static TextBox CreateDarkTextBox(Point location, Size size, string? text = null)
        {
            var textBox = ControlFactory.CreateFieldTextBox(
                location, size,
                backColor: DarkTheme.Field,
                foreColor: DarkTheme.Text,
                borderStyle: BorderStyle.FixedSingle,
                text: text);
            textBox.Font = new Font("Segoe UI", 10);
            return textBox;
        }

        /// <summary>
        /// Кнопка в темном оформлении формы
        /// </summary>
        private static Button CreateDarkButton(
            string text,
            Point location,
            Size size,
            Color backColor,
            EventHandler onClick,
            Font? font = null)
        {
            var button = ControlFactory.CreateFlatButton(text, location, size, backColor, onClick, font);
            button.Cursor = Cursors.Default;
            return button;
        }

        private void SetupDefaultPath()
        {
            string defaultDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "IoPointDB.mdb");
            txtDbPath.Text = defaultDbPath;
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtDbPath.Text = openFileDialog.FileName;
                GetLogger().Success($"Выбрана БД: {openFileDialog.FileName}");
            }
        }

        private void BtnCreateDb_Click(object sender, EventArgs e)
        {
            try
            {
                string dbPath = txtDbPath.Text.Trim();
                if (string.IsNullOrEmpty(dbPath))
                {
                    UserMessages.Warning("Укажите путь для новой базы данных!", "Ошибка");
                    return;
                }

                var dbService = new DatabaseService(dbPath);
                if (dbService.DatabaseExists())
                {
                    if (!UserMessages.Confirm("База данных уже существует. Пересоздать?", "Подтверждение"))
                        return;

                    try
                    {
                        File.Delete(dbPath);
                        GetLogger().Info($"Удалена существующая БД: {dbPath}");
                    }
                    catch
                    {
                        UserMessages.Warning(
                            "Не удалось удалить существующую БД. Возможно, она открыта в другой программе.",
                            "Ошибка");
                        return;
                    }
                }

                dbService.CreateEmptyDatabase();
                GetLogger().Success($"Создана новая база данных: {dbPath}");
                UpdateStatus($"✅ БД создана: {Path.GetFileName(dbPath)}");

                UserMessages.Info($"База данных создана:\n{dbPath}", "Успех");
            }
            catch (Exception ex)
            {
                ReportFailure("Ошибка создания БД", ex);
            }
        }

        private void BtnCreateTable_Click(object sender, EventArgs e)
        {
            try
            {
                string dbPath = txtDbPath.Text.Trim();
                string tableName = txtTableName.Text.Trim();

                if (!ValidateDbPathAndTableName(dbPath, tableName))
                    return;

                var logger = GetLogger();
                var dbService = new DatabaseService(dbPath);

                // Проверяем существует ли БД
                if (!dbService.DatabaseExists())
                {
                    logger.Info("База данных не найдена. Создаем новую...");
                    dbService.CreateEmptyDatabase();
                    logger.Success("База данных создана");
                }

                logger.Info($"Создание таблицы '{tableName}'...");
                UpdateStatus($"⏳ Создание таблицы '{tableName}'...");

                // Создаем таблицу
                dbService.CreateTableIfNotExists(tableName);

                logger.Success($"Таблица '{tableName}' успешно создана!");
                UpdateStatus($"✅ Таблица '{tableName}' создана");

                // Показываем список таблиц
                var tables = dbService.GetTableNames();
                logger.Info($"Таблицы в БД: {string.Join(", ", tables)}");

                // Показываем структуру таблицы
                logger.Info($"Количество полей в таблице: {Models.TableStructure.GetFields().Count}");

                UserMessages.Info($"Таблица '{tableName}' успешно создана в БД:\n{dbPath}", "Успех");
            }
            catch (Exception ex)
            {
                ReportFailure("Ошибка создания таблицы", ex);
            }
        }

        private void BtnInsertSample_Click(object sender, EventArgs e)
        {
            try
            {
                string dbPath = txtDbPath.Text.Trim();
                string tableName = txtTableName.Text.Trim();

                if (!ValidateDbPathAndTableName(dbPath, tableName))
                    return;

                var logger = GetLogger();
                var dbService = new DatabaseService(dbPath);

                if (!dbService.DatabaseExists())
                {
                    logger.Error("База данных не найдена!");
                    UserMessages.Warning(
                        "База данных не найдена. Сначала создайте базу данных и таблицу.", "Ошибка");
                    return;
                }

                if (!dbService.TableExists(tableName))
                {
                    logger.Error($"Таблица '{tableName}' не существует!");
                    UserMessages.Warning(
                        $"Таблица '{tableName}' не существует. Сначала создайте таблицу.", "Ошибка");
                    return;
                }

                logger.Info("Добавление тестовых данных...");
                dbService.InsertSampleData(tableName);
                logger.Success("Тестовые данные добавлены!");

                UserMessages.Info("Тестовые данные успешно добавлены в таблицу!", "Успех");
            }
            catch (Exception ex)
            {
                ReportFailure("Ошибка добавления данных", ex, updateStatus: false);
            }
        }

        /// <summary>
        /// Проверка обязательных полей: путь к БД и имя таблицы
        /// </summary>
        private static bool ValidateDbPathAndTableName(string dbPath, string tableName)
        {
            if (string.IsNullOrEmpty(dbPath))
            {
                UserMessages.Warning("Укажите путь к базе данных!", "Ошибка");
                return false;
            }

            if (string.IsNullOrEmpty(tableName))
            {
                UserMessages.Warning("Введите имя таблицы!", "Ошибка");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Запись ошибки в лог, строку состояния и диалог пользователя
        /// </summary>
        private void ReportFailure(string title, Exception ex, bool updateStatus = true)
        {
            GetLogger().Error($"{title}: {ex.Message}");

            if (updateStatus)
                UpdateStatus($"❌ Ошибка: {ex.Message}");

            UserMessages.Error($"{title}:\n{ex.Message}");
        }

        private Logger GetLogger()
        {
            if (_logger == null)
                _logger = new Logger(txtLog);
            return _logger;
        }

        private void UpdateStatus(string message)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.Invoke(new Action(() => lblStatus.Text = message));
            }
            else
            {
                lblStatus.Text = message;
            }
        }
    }
}