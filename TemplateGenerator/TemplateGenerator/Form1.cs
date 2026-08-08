using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OfficeOpenXml;

namespace TemplateGenerator
{
    public partial class Form1 : Form
    {
        private DataTable replacementTable;
        private string templateContent = string.Empty;
        private string templateFilePath = string.Empty;
        private List<InstanceData> instances = new List<InstanceData>();
        private string excelFilePath = string.Empty;
        private Dictionary<int, string> activeReplacements = new Dictionary<int, string>();
        private bool convertTagsToUnderscore = false;
        private const int MAX_LOG_MESSAGES = 1000;

        public Form1()
        {
            InitializeComponent();
            InitializeReplacementTable();
            SetupDataGridViewStyle();
            ApplyModernTheme();
            AddLogMessage("Программа запущена", LogType.Info);
        }

        private void InitializeReplacementTable()
        {
            replacementTable = new DataTable();
            replacementTable.Columns.Add("№", typeof(int));
            replacementTable.Columns.Add("Имя тега для поиска", typeof(string));
            replacementTable.Columns.Add("Использовать", typeof(bool));

            for (int i = 1; i <= 10; i++)
            {
                replacementTable.Rows.Add(i, string.Empty, false);
            }

            dataGridViewReplacements.DataSource = replacementTable;
        }

        private void SetupDataGridViewStyle()
        {
            if (dataGridViewReplacements.Columns.Contains("№"))
            {
                dataGridViewReplacements.Columns["№"].ReadOnly = true;
                dataGridViewReplacements.Columns["№"].Width = 40;
                dataGridViewReplacements.Columns["№"].DefaultCellStyle.ForeColor = Color.Black;
            }
            if (dataGridViewReplacements.Columns.Contains("Имя тега для поиска"))
            {
                dataGridViewReplacements.Columns["Имя тега для поиска"].Width = 130;
                dataGridViewReplacements.Columns["Имя тега для поиска"].DefaultCellStyle.ForeColor = Color.Black;
            }
            if (dataGridViewReplacements.Columns.Contains("Использовать"))
            {
                dataGridViewReplacements.Columns["Использовать"].Width = 55;
                dataGridViewReplacements.Columns["Использовать"].DefaultCellStyle.ForeColor = Color.Black;
            }

            dataGridViewReplacements.ScrollBars = ScrollBars.Vertical;
            dataGridViewReplacements.RowHeadersVisible = false;
            dataGridViewReplacements.AllowUserToAddRows = false;
            dataGridViewReplacements.AllowUserToDeleteRows = false;
        }

        private void ApplyModernTheme()
        {
            this.BackColor = Color.FromArgb(13, 37, 63);
            this.ForeColor = Color.White;
            ApplyThemeToControl(this);
        }

        private void ApplyThemeToControl(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button btn)
                {
                    if (btn.Name != "btnConvertTags" && btn.Name != "btnClearAll")
                    {
                        btn.BackColor = Color.FromArgb(0, 120, 215);
                        btn.ForeColor = Color.White;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    }
                }
                else if (ctrl is Label lbl)
                {
                    lbl.ForeColor = Color.FromArgb(200, 220, 240);
                    lbl.Font = new Font("Segoe UI", 9);
                }
                else if (ctrl is RichTextBox rtb)
                {
                    if (rtb.Name == "richTextBoxLog")
                    {
                        // Лог имеет свой собственный стиль
                        continue;
                    }
                    rtb.BackColor = Color.FromArgb(30, 60, 90);
                    rtb.ForeColor = Color.White;
                    rtb.Font = new Font("Consolas", 9);
                }
                else if (ctrl is DataGridView dgv)
                {
                    dgv.BackgroundColor = Color.FromArgb(30, 60, 90);
                    dgv.ForeColor = Color.White;
                    dgv.GridColor = Color.FromArgb(60, 100, 140);
                    dgv.DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 255);
                    dgv.DefaultCellStyle.ForeColor = Color.Black;
                    dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 80, 150);
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dgv.RowHeadersVisible = false;
                    dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dgv.BorderStyle = BorderStyle.None;
                }
                
                if (ctrl.HasChildren)
                {
                    ApplyThemeToControl(ctrl);
                }
            }
        }

        #region Логирование

        public enum LogType
        {
            Info,
            Success,
            Warning,
            Error,
            Process,
            File
        }

        private void AddLogMessage(string message, LogType type = LogType.Info)
        {
            if (richTextBoxLog == null) return;

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string prefix = type switch
            {
                LogType.Info => "ℹ️",
                LogType.Success => "✅",
                LogType.Warning => "⚠️",
                LogType.Error => "❌",
                LogType.Process => "🚀",
                LogType.File => "📄",
                _ => "ℹ️"
            };

            Color color = type switch
            {
                LogType.Info => Color.White,
                LogType.Success => Color.LightGreen,
                LogType.Warning => Color.Gold,
                LogType.Error => Color.LightCoral,
                LogType.Process => Color.LightBlue,
                LogType.File => Color.LightGray,
                _ => Color.White
            };

            string logMessage = $"[{timestamp}] {prefix} {message}";

            // Вставляем в начало
            richTextBoxLog.SelectionStart = 0;
            richTextBoxLog.SelectionColor = color;
            richTextBoxLog.SelectedText = logMessage + "\n";

            // Ограничиваем количество строк
            if (richTextBoxLog.Lines.Length > MAX_LOG_MESSAGES)
            {
                int linesToRemove = richTextBoxLog.Lines.Length - MAX_LOG_MESSAGES;
                int charsToRemove = 0;
                for (int i = 0; i < linesToRemove; i++)
                {
                    charsToRemove += richTextBoxLog.Lines[i].Length + 1;
                }
                richTextBoxLog.Text = richTextBoxLog.Text.Substring(charsToRemove);
            }

            // Прокручиваем к началу
            richTextBoxLog.SelectionStart = 0;
            richTextBoxLog.ScrollToCaret();
        }

        #endregion

        private void btnLoadTemplate_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.Title = "Выберите файл шаблона";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        templateFilePath = openFileDialog.FileName;
                        templateContent = File.ReadAllText(templateFilePath, Encoding.UTF8);
                        richTextBoxPreview.Text = templateContent;
                        lblTemplatePath.Text = $"Шаблон: {Path.GetFileName(templateFilePath)}";
                        lblTemplatePath.ForeColor = Color.LightGreen;
                        
                        long fileSize = new FileInfo(templateFilePath).Length;
                        AddLogMessage($"Шаблон загружен: {Path.GetFileName(templateFilePath)} ({fileSize} байт)", LogType.Success);
                    }
                    catch (Exception ex)
                    {
                        AddLogMessage($"Ошибка загрузки шаблона: {ex.Message}", LogType.Error);
                        MessageBox.Show($"Ошибка загрузки шаблона: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnLoadExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                openFileDialog.Title = "Выберите файл с параметрами замены";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        excelFilePath = openFileDialog.FileName;
                        LoadExcelData();
                        lblExcelPath.Text = $"Excel: {Path.GetFileName(excelFilePath)}";
                        lblExcelPath.ForeColor = Color.LightGreen;
                        UpdateInstancesCount();
                        UpdateMappingInfo();
                        AddLogMessage($"Excel файл загружен: {Path.GetFileName(excelFilePath)} ({instances.Count} экземпляров)", LogType.Success);
                    }
                    catch (Exception ex)
                    {
                        AddLogMessage($"Ошибка загрузки Excel: {ex.Message}", LogType.Error);
                        MessageBox.Show($"Ошибка загрузки Excel: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadExcelData()
        {
            instances.Clear();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null)
                {
                    AddLogMessage("Excel файл не содержит листов", LogType.Error);
                    MessageBox.Show("Excel файл не содержит листов", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int rowCount = worksheet.Dimension.Rows;

                var instanceData = new Dictionary<string, Dictionary<int, string>>();

                for (int row = 2; row <= rowCount; row++)
                {
                    string instanceName = worksheet.Cells[row, 1]?.Text?.Trim();
                    if (string.IsNullOrEmpty(instanceName))
                        continue;

                    string tagValue = worksheet.Cells[row, 2]?.Text?.Trim();
                    if (string.IsNullOrEmpty(tagValue))
                        continue;

                    string positionStr = worksheet.Cells[row, 3]?.Text?.Trim();
                    if (string.IsNullOrEmpty(positionStr))
                        continue;

                    if (!int.TryParse(positionStr, out int position))
                        continue;

                    if (!instanceData.ContainsKey(instanceName))
                    {
                        instanceData[instanceName] = new Dictionary<int, string>();
                    }

                    instanceData[instanceName][position] = tagValue;
                }

                foreach (var kvp in instanceData)
                {
                    var instance = new InstanceData
                    {
                        InstanceName = kvp.Key,
                        Replacements = kvp.Value
                    };
                    instances.Add(instance);
                }
            }

            UpdateInstancesCount();
        }

        private void UpdateInstancesCount()
        {
            if (lblInstancesCount != null)
            {
                lblInstancesCount.Text = $"Найдено экземпляров: {instances.Count}";
                lblInstancesCount.ForeColor = Color.LightGreen;
            }
        }

        private string ConvertDashToUnderscore(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return tag;
            return tag.Replace('-', '_');
        }

        private void UpdateMappingInfo()
        {
            if (richTextBoxMapping == null) return;

            if (!instances.Any())
            {
                richTextBoxMapping.Text = "Нет загруженных экземпляров.\n\n" +
                    "Формат Excel файла:\n" +
                    "Столбец A: Имя экземпляра (LOOP)\n" +
                    "Столбец B: Tag No (новый тег для замены)\n" +
                    "Столбец C: № позиции для замены (1-10)";
                return;
            }

            activeReplacements.Clear();
            foreach (DataRow row in replacementTable.Rows)
            {
                if (Convert.ToBoolean(row["Использовать"]))
                {
                    int position = Convert.ToInt32(row["№"]);
                    string searchTag = row["Имя тега для поиска"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(searchTag))
                    {
                        activeReplacements[position] = searchTag;
                    }
                }
            }

            if (!activeReplacements.Any())
            {
                richTextBoxMapping.Text = "Нет активных позиций для замены.\n" +
                    "Заполните таблицу замен слева (отметьте галочкой Использовать).";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║              ИНФОРМАЦИЯ О ЗАМЕНАХ                             ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            if (convertTagsToUnderscore)
            {
                sb.AppendLine("🔧 РЕЖИМ ПРЕОБРАЗОВАНИЯ ТЕГОВ: '-' → '_' АКТИВЕН");
                sb.AppendLine();
            }

            sb.AppendLine("┌─────────────────────────────────────────────────────────────────┐");
            sb.Append("│ Экземпляр".PadRight(25));
            foreach (var pos in activeReplacements.Keys.OrderBy(k => k))
            {
                sb.Append($"│ Поз.{pos,-5}");
            }
            sb.AppendLine("│");
            sb.AppendLine("├─────────────────────────────────────────────────────────────────┤");

            foreach (var instance in instances)
            {
                sb.Append($"│ {instance.InstanceName.PadRight(23)}");
                foreach (var pos in activeReplacements.Keys.OrderBy(k => k))
                {
                    string value = instance.Replacements.TryGetValue(pos, out string val) ? val : "---";
                    if (convertTagsToUnderscore && value != "---")
                    {
                        value = ConvertDashToUnderscore(value);
                    }
                    sb.Append($"│ {value,-8}");
                }
                sb.AppendLine("│");
            }
            sb.AppendLine("└─────────────────────────────────────────────────────────────────┘");
            sb.AppendLine();

            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║              ДЕТАЛИ ЗАМЕН ПО ЭКЗЕМПЛЯРАМ                     ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            foreach (var instance in instances)
            {
                sb.AppendLine($"📁 {instance.InstanceName}");
                sb.AppendLine("   ┌──────────────────────────────────────────────────────────────┐");
                
                bool hasReplacements = false;
                foreach (var pos in activeReplacements.Keys.OrderBy(k => k))
                {
                    if (instance.Replacements.TryGetValue(pos, out string newTag))
                    {
                        string searchTag = activeReplacements[pos];
                        string displayNewTag = convertTagsToUnderscore ? ConvertDashToUnderscore(newTag) : newTag;
                        sb.AppendLine($"   │ Позиция {pos,2}:  [{searchTag}]  →  [{displayNewTag}]");
                        hasReplacements = true;
                    }
                }
                
                if (!hasReplacements)
                {
                    sb.AppendLine("   │ (нет активных замен для этого экземпляра)");
                }
                sb.AppendLine("   └──────────────────────────────────────────────────────────────┘");
                sb.AppendLine();
            }

            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║              СТАТИСТИКА                                       ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"   Всего экземпляров: {instances.Count}");
            sb.AppendLine($"   Активных позиций замен: {activeReplacements.Count}");
            sb.AppendLine($"   Преобразование '-' → '_': {(convertTagsToUnderscore ? "ВКЛ" : "ВЫКЛ")}");
            
            int totalReplacements = 0;
            foreach (var instance in instances)
            {
                foreach (var pos in activeReplacements.Keys)
                {
                    if (instance.Replacements.ContainsKey(pos))
                        totalReplacements++;
                }
            }
            sb.AppendLine($"   Всего замен: {totalReplacements}");

            richTextBoxMapping.Text = sb.ToString();
        }

        private void btnConvertTags_Click(object sender, EventArgs e)
        {
            convertTagsToUnderscore = !convertTagsToUnderscore;
            
            if (convertTagsToUnderscore)
            {
                btnConvertTags.BackColor = Color.FromArgb(76, 175, 80);
                btnConvertTags.ForeColor = Color.White;
                btnConvertTags.Text = "🔄 Преобразование\nВКЛ (→ _)";
                AddLogMessage("Режим преобразования включен", LogType.Info);
            }
            else
            {
                btnConvertTags.BackColor = Color.FromArgb(255, 193, 7);
                btnConvertTags.ForeColor = Color.Black;
                btnConvertTags.Text = "🔄 Преобразовать тэг";
                AddLogMessage("Режим преобразования выключен", LogType.Info);
            }
            
            UpdateMappingInfo();
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите очистить все данные?\n" +
                "Будут удалены: шаблон, Excel данные, таблица замен и лог.", 
                "Подтверждение очистки", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Очищаем шаблон
                templateContent = string.Empty;
                templateFilePath = string.Empty;
                richTextBoxPreview.Text = string.Empty;
                lblTemplatePath.Text = "Шаблон: -";
                lblTemplatePath.ForeColor = Color.FromArgb(200, 220, 240);

                // Очищаем Excel данные
                instances.Clear();
                excelFilePath = string.Empty;
                lblExcelPath.Text = "Excel: -";
                lblExcelPath.ForeColor = Color.FromArgb(200, 220, 240);
                lblInstancesCount.Text = "Найдено экземпляров: 0";

                // Очищаем таблицу замен
                foreach (DataRow row in replacementTable.Rows)
                {
                    row["Имя тега для поиска"] = string.Empty;
                    row["Использовать"] = false;
                }
                dataGridViewReplacements.Refresh();

                // Сбрасываем состояние преобразования
                convertTagsToUnderscore = false;
                btnConvertTags.BackColor = Color.FromArgb(255, 193, 7);
                btnConvertTags.ForeColor = Color.Black;
                btnConvertTags.Text = "🔄 Преобразовать тэг";

                // Очищаем информацию справа
                richTextBoxMapping.Text = "Все данные очищены.\n\n" +
                    "Загрузите шаблон и Excel файл для начала работы.";

                // Очищаем лог
                richTextBoxLog.Clear();
                AddLogMessage("Все данные очищены", LogType.Info);

                MessageBox.Show("Все данные успешно очищены!", "Готово", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(templateContent))
            {
                AddLogMessage("Ошибка: не загружен шаблон", LogType.Warning);
                MessageBox.Show("Сначала загрузите файл шаблона!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!instances.Any())
            {
                AddLogMessage("Ошибка: не загружен Excel файл", LogType.Warning);
                MessageBox.Show("Загрузите Excel файл с данными для замены!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!HasActiveReplacements())
            {
                AddLogMessage("Ошибка: нет активных позиций для замены", LogType.Warning);
                MessageBox.Show("Заполните хотя бы одну позицию для поиска и замены!", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "Выберите папку для сохранения сгенерированных файлов";
                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string outputFolder = folderDialog.SelectedPath;
                        AddLogMessage($"Начата генерация файлов в папку: {outputFolder}", LogType.Process);
                        GenerateInstances(outputFolder);
                        AddLogMessage($"Успешно сгенерировано {instances.Count} файлов!", LogType.Success);
                        MessageBox.Show($"Успешно сгенерировано {instances.Count} файлов!", "Готово",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Ошибка генерации: {ex.Message}", LogType.Error);
                MessageBox.Show($"Ошибка генерации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool HasActiveReplacements()
        {
            if (replacementTable == null) return false;
            
            foreach (DataRow row in replacementTable.Rows)
            {
                if (Convert.ToBoolean(row["Использовать"]) &&
                    !string.IsNullOrEmpty(row["Имя тега для поиска"]?.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        private void GenerateInstances(string outputFolder)
        {
            var activeReplacementsDict = new Dictionary<int, string>();
            foreach (DataRow row in replacementTable.Rows)
            {
                if (Convert.ToBoolean(row["Использовать"]))
                {
                    int position = Convert.ToInt32(row["№"]);
                    string searchTag = row["Имя тега для поиска"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(searchTag))
                    {
                        activeReplacementsDict[position] = searchTag;
                    }
                }
            }

            if (!activeReplacementsDict.Any())
            {
                throw new Exception("Нет активных позиций для замены");
            }

            int generatedCount = 0;
            foreach (var instance in instances)
            {
                string resultContent = templateContent;

                foreach (var replacement in activeReplacementsDict)
                {
                    int position = replacement.Key;
                    string searchTag = replacement.Value;

                    if (instance.Replacements.TryGetValue(position, out string newTag))
                    {
                        string finalTag = convertTagsToUnderscore ? ConvertDashToUnderscore(newTag) : newTag;
                        resultContent = ReplaceTagInContent(resultContent, searchTag, finalTag);
                    }
                }

                string fileName = $"{instance.InstanceName}.txt";
                string fullPath = Path.Combine(outputFolder, fileName);

                int counter = 1;
                while (File.Exists(fullPath))
                {
                    fileName = $"{instance.InstanceName}_{counter}.txt";
                    fullPath = Path.Combine(outputFolder, fileName);
                    counter++;
                }

                File.WriteAllText(fullPath, resultContent, Encoding.UTF8);
                generatedCount++;
                AddLogMessage($"Создан файл: {fileName}", LogType.File);
            }
        }

        private string ReplaceTagInContent(string content, string searchTag, string newTag)
        {
            if (string.IsNullOrEmpty(searchTag) || string.IsNullOrEmpty(newTag))
                return content;

            string escapedSearch = Regex.Escape(searchTag);
            return Regex.Replace(content, escapedSearch, newTag, RegexOptions.None);
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            if (!HasActiveReplacements())
            {
                AddLogMessage("Ошибка: нет активных позиций для замены", LogType.Warning);
                MessageBox.Show("Заполните хотя бы одну позицию для поиска и замены!", "Проверка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var activePositions = new List<int>();
            foreach (DataRow row in replacementTable.Rows)
            {
                if (Convert.ToBoolean(row["Использовать"]))
                {
                    int position = Convert.ToInt32(row["№"]);
                    string searchTag = row["Имя тега для поиска"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(searchTag))
                    {
                        activePositions.Add(position);
                    }
                }
            }

            var missingReplacements = new List<string>();
            foreach (var instance in instances)
            {
                foreach (var pos in activePositions)
                {
                    if (!instance.Replacements.ContainsKey(pos))
                    {
                        missingReplacements.Add($"{instance.InstanceName} - позиция {pos}");
                    }
                }
            }

            if (missingReplacements.Any())
            {
                foreach (var missing in missingReplacements.Take(10))
                {
                    AddLogMessage($"Для {missing} отсутствует замена", LogType.Warning);
                }
            }

            string message = $"Проверка пройдена!\n" +
                $"Активных позиций: {activePositions.Count}\n" +
                $"Загружено экземпляров: {instances.Count}\n" +
                $"Шаблон загружен: {!string.IsNullOrEmpty(templateContent)}\n" +
                $"Преобразование '-' → '_': {(convertTagsToUnderscore ? "ВКЛ" : "ВЫКЛ")}\n";

            if (missingReplacements.Any())
            {
                message += $"\n⚠️ ВНИМАНИЕ: Для следующих экземпляров отсутствуют замены:\n" +
                    string.Join("\n", missingReplacements.Take(10));
                if (missingReplacements.Count > 10)
                    message += $"\n... и еще {missingReplacements.Count - 10}";
                AddLogMessage($"Обнаружено {missingReplacements.Count} отсутствующих замен", LogType.Warning);
            }
            else
            {
                AddLogMessage("Проверка успешно пройдена", LogType.Success);
            }

            MessageBox.Show(message, "Проверка", MessageBoxButtons.OK, 
                missingReplacements.Any() ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void dataGridViewReplacements_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                dataGridViewReplacements.RefreshEdit();
                UpdateMappingInfo();
            }
        }

        private void dataGridViewReplacements_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewReplacements.IsCurrentCellDirty)
            {
                dataGridViewReplacements.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
    }

    public class InstanceData
    {
        public string InstanceName { get; set; }
        public Dictionary<int, string> Replacements { get; set; } = new Dictionary<int, string>();
    }
}