// ================================================================
// ПРОСТРАНСТВА ИМЕН
// ================================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PJT1.Models;

namespace PJT1.Services
{
    /// <summary>
    /// КЛАСС TxtExportService - СЕРВИС ДЛЯ ЭКСПОРТА В TXT
    /// 
    /// Статический класс для сохранения данных в текстовый файл
    /// Поддерживает 5 различных форматов экспорта
    /// </summary>
    public static class TxtExportService
    {
        // ============================================================
        // ПЕРЕЧИСЛЕНИЕ ФОРМАТОВ ЭКСПОРТА
        // ============================================================

        /// <summary>
        /// Форматы экспорта данных в TXT файл
        /// </summary>
        public enum ExportFormat
        {
            Table,      // Табличный формат с разделителями
            Column,     // Столбчатый формат с выравниванием
            List,       // Список с маркерами
            JsonLike,   // JSON-подобный формат
            Detailed    // Подробный формат с полным описанием
        }

        // ============================================================
        // ОСНОВНОЙ МЕТОД ЭКСПОРТА
        // ============================================================

        /// <summary>
        /// ЭКСПОРТ ДАННЫХ В TXT ФАЙЛ
        /// 
        /// Сохраняет все данные из репозитория в текстовый файл
        /// с выбором формата
        /// </summary>
        /// <param name="dataList">Список данных для экспорта</param>
        /// <param name="filePath">Путь к файлу для сохранения</param>
        /// <param name="format">Формат экспорта</param>
        /// <param name="includeHeader">Включать ли заголовок</param>
        public static void ExportToTxt(
            IEnumerable<DataBD> dataList,
            string filePath,
            ExportFormat format = ExportFormat.Table,
            bool includeHeader = true)
        {
            // Проверяем, что список не null
            if (dataList == null)
                throw new ArgumentNullException(nameof(dataList), "Список данных не может быть null");

            // Проверяем путь
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

            // Добавляем расширение .txt, если его нет
            if (!filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                filePath += ".txt";

            try
            {
                // Генерируем содержимое в зависимости от формата
                string content = format switch
                {
                    ExportFormat.Table => ExportAsTable(dataList, includeHeader),
                    ExportFormat.Column => ExportAsColumns(dataList, includeHeader),
                    ExportFormat.List => ExportAsList(dataList, includeHeader),
                    ExportFormat.JsonLike => ExportAsJsonLike(dataList, includeHeader),
                    ExportFormat.Detailed => ExportAsDetailed(dataList, includeHeader),
                    _ => ExportAsTable(dataList, includeHeader)
                };

                // Записываем в файл (UTF-8 для поддержки кириллицы)
                File.WriteAllText(filePath, content, Encoding.UTF8);

                // Показываем сообщение об успехе
                MessageBox.Show(
                    $"✅ Данные успешно сохранены в файл:\n{filePath}\n" +
                    $"📊 Количество записей: {GetCount(dataList)}\n" +
                    $"📦 Размер файла: {new FileInfo(filePath).Length / 1024} КБ",
                    "Экспорт завершен",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении файла: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ЭКСПОРТ С ВЫБОРОМ ФОРМАТА ЧЕРЕЗ ДИАЛОГ
        /// 
        /// Показывает диалог выбора формата и сохраняет файл
        /// </summary>
        public static bool ExportWithDialog(
            IEnumerable<DataBD> dataList, 
            string defaultFileName = "export")
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    // Настраиваем диалог сохранения
                    saveDialog.Title = "Сохранить данные в TXT файл";
                    saveDialog.Filter = "Текстовый файл (*.txt)|*.txt";
                    saveDialog.DefaultExt = "txt";
                    saveDialog.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}";
                    saveDialog.InitialDirectory = Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments
                    );

                    // Если пользователь выбрал файл
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Показываем диалог выбора формата
                        using (var formatDialog = new Forms.FormatSelectionDialog())
                        {
                            if (formatDialog.ShowDialog() == DialogResult.OK)
                            {
                                ExportToTxt(
                                    dataList,
                                    saveDialog.FileName,
                                    formatDialog.SelectedFormat,
                                    formatDialog.IncludeHeader
                                );
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при экспорте данных:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            return false;
        }

        // ============================================================
        // ПРИВАТНЫЕ МЕТОДЫ ФОРМАТИРОВАНИЯ
        // ============================================================

        /// <summary>
        /// ТАБЛИЧНЫЙ ФОРМАТ (С РАЗДЕЛИТЕЛЯМИ)
        /// </summary>
        private static string ExportAsTable(IEnumerable<DataBD> dataList, bool includeHeader)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                sb.AppendLine($"Экспорт данных: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine(new string('=', 80));
                sb.AppendLine();
                sb.AppendLine("ID\t| Tagname\t\t| Loop\t\t| Comment\t\t| Дата создания");
                sb.AppendLine(new string('-', 80));
            }

            foreach (var data in dataList)
            {
                sb.AppendLine(
                    $"{data.Id}\t| {data.Tagname?.PadRight(15) ?? "null"}\t| " +
                    $"{data.Loop?.PadRight(10) ?? "null"}\t| " +
                    $"{data.Comment?.PadRight(20) ?? "null"}\t| " +
                    $"{data.CreatedDate:dd.MM.yyyy HH:mm}"
                );
            }

            if (includeHeader)
            {
                sb.AppendLine(new string('-', 80));
                sb.AppendLine($"Всего записей: {GetCount(dataList)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// СТОЛБЧАТЫЙ ФОРМАТ (С ВЫРАВНИВАНИЕМ)
        /// </summary>
        private static string ExportAsColumns(IEnumerable<DataBD> dataList, bool includeHeader)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                sb.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                sb.AppendLine("║                       ЭКСПОРТ ДАННЫХ ИЗ РЕПОЗИТОРИЯ                         ║");
                sb.AppendLine($"║                      Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}                     ║");
                sb.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                sb.AppendLine();
            }

            // Определяем максимальную длину каждого поля
            int maxTagnameLen = Math.Min(30, Math.Max(15, GetMaxLength(dataList, d => d.Tagname?.Length ?? 0) + 2));
            int maxLoopLen = Math.Min(20, Math.Max(10, GetMaxLength(dataList, d => d.Loop?.Length ?? 0) + 2));
            int maxCommentLen = Math.Min(40, Math.Max(20, GetMaxLength(dataList, d => d.Comment?.Length ?? 0) + 2));

            // Формируем шапку
            string header = $"{"ID".PadRight(4)} " +
                           $"{"Tagname".PadRight(maxTagnameLen)} " +
                           $"{"Loop".PadRight(maxLoopLen)} " +
                           $"{"Comment".PadRight(maxCommentLen)} " +
                           $"{"Дата".PadRight(20)}";

            if (includeHeader)
            {
                sb.AppendLine(header);
                sb.AppendLine(new string('-', header.Length));
            }

            // Данные
            foreach (var data in dataList)
            {
                sb.AppendLine(
                    $"{data.Id.ToString().PadRight(4)} " +
                    $"{data.Tagname?.PadRight(maxTagnameLen) ?? "null"} " +
                    $"{data.Loop?.PadRight(maxLoopLen) ?? "null"} " +
                    $"{data.Comment?.PadRight(maxCommentLen) ?? "null"} " +
                    $"{data.CreatedDate:dd.MM.yyyy HH:mm}"
                );
            }

            if (includeHeader)
            {
                sb.AppendLine(new string('-', header.Length));
                sb.AppendLine($"Всего записей: {GetCount(dataList)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// СПИСОК С МАРКЕРАМИ
        /// </summary>
        private static string ExportAsList(IEnumerable<DataBD> dataList, bool includeHeader)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"       ЭКСПОРТ ДАННЫХ В ФОРМАТЕ СПИСКА");
                sb.AppendLine($"       Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();
            }

            int counter = 1;
            foreach (var data in dataList)
            {
                sb.AppendLine($"{counter}. {data.Tagname} - {data.Loop}");
                if (!string.IsNullOrEmpty(data.Comment))
                {
                    sb.AppendLine($"   Комментарий: {data.Comment}");
                }
                sb.AppendLine($"   ID: {data.Id} | Дата: {data.CreatedDate:dd.MM.yyyy HH:mm}");
                sb.AppendLine();
                counter++;
            }

            if (includeHeader)
            {
                sb.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine($"Всего записей: {GetCount(dataList)}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// JSON-ПОДОБНЫЙ ФОРМАТ
        /// </summary>
        private static string ExportAsJsonLike(IEnumerable<DataBD> dataList, bool includeHeader)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                sb.AppendLine("{");
                sb.AppendLine($"  \"exportDate\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
                sb.AppendLine($"  \"recordCount\": {GetCount(dataList)},");
                sb.AppendLine("  \"data\": [");
            }

            int index = 0;
            int count = GetCount(dataList);
            foreach (var data in dataList)
            {
                sb.AppendLine("    {");
                sb.AppendLine($"      \"id\": {data.Id},");
                sb.AppendLine($"      \"tagname\": \"{EscapeJson(data.Tagname)}\",");
                sb.AppendLine($"      \"loop\": \"{EscapeJson(data.Loop)}\",");
                sb.AppendLine($"      \"comment\": \"{EscapeJson(data.Comment)}\",");
                sb.AppendLine($"      \"createdDate\": \"{data.CreatedDate:yyyy-MM-dd HH:mm:ss}\"");
                sb.Append(index < count - 1 ? "    }," : "    }");
                sb.AppendLine();
                index++;
            }

            if (includeHeader)
            {
                sb.AppendLine("  ]");
                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// ПОДРОБНЫЙ ФОРМАТ (ПОЛНОЕ ОПИСАНИЕ)
        /// </summary>
        private static string ExportAsDetailed(IEnumerable<DataBD> dataList, bool includeHeader)
        {
            var sb = new StringBuilder();

            if (includeHeader)
            {
                sb.AppendLine(new string('═', 60));
                sb.AppendLine($"         ПОДРОБНЫЙ ОТЧЕТ ПО ДАННЫМ");
                sb.AppendLine($"         Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine(new string('═', 60));
                sb.AppendLine();
            }

            foreach (var data in dataList)
            {
                sb.AppendLine($"┌─ ЗАПИСЬ #{data.Id} ───────────────────────────────────────────────┐");
                sb.AppendLine($"│");
                sb.AppendLine($"│  Имя тега:    {data.Tagname}");
                sb.AppendLine($"│  Цикл:        {data.Loop}");
                sb.AppendLine($"│  Комментарий: {data.Comment}");
                sb.AppendLine($"│  Дата:        {data.CreatedDate:dd.MM.yyyy HH:mm:ss}");
                sb.AppendLine($"│");
                sb.AppendLine($"└──────────────────────────────────────────────────────────────────┘");
                sb.AppendLine();
            }

            if (includeHeader)
            {
                sb.AppendLine(new string('═', 60));
                sb.AppendLine($"ИТОГО ЗАПИСЕЙ: {GetCount(dataList)}");
                sb.AppendLine($"ПЕРИОД: {GetDateRange(dataList)}");
                sb.AppendLine(new string('═', 60));
            }

            return sb.ToString();
        }

        // ============================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ============================================================

        /// <summary>
        /// ПОЛУЧИТЬ КОЛИЧЕСТВО ЗАПИСЕЙ
        /// </summary>
        private static int GetCount(IEnumerable<DataBD> dataList)
        {
            if (dataList == null) return 0;
            using var enumerator = dataList.GetEnumerator();
            int count = 0;
            while (enumerator.MoveNext()) count++;
            return count;
        }

        /// <summary>
        /// ПОЛУЧИТЬ МАКСИМАЛЬНУЮ ДЛИНУ ПОЛЯ
        /// </summary>
        private static int GetMaxLength(IEnumerable<DataBD> dataList, Func<DataBD, int> selector)
        {
            int max = 0;
            foreach (var data in dataList)
            {
                int len = selector(data);
                if (len > max) max = len;
            }
            return max;
        }

        /// <summary>
        /// ПОЛУЧИТЬ ДИАПАЗОН ДАТ
        /// </summary>
        private static string GetDateRange(IEnumerable<DataBD> dataList)
        {
            if (dataList == null) return "Нет данных";
            using var enumerator = dataList.GetEnumerator();
            if (!enumerator.MoveNext()) return "Нет данных";

            DateTime minDate = enumerator.Current.CreatedDate;
            DateTime maxDate = minDate;

            while (enumerator.MoveNext())
            {
                if (enumerator.Current.CreatedDate < minDate)
                    minDate = enumerator.Current.CreatedDate;
                if (enumerator.Current.CreatedDate > maxDate)
                    maxDate = enumerator.Current.CreatedDate;
            }

            return minDate == maxDate 
                ? minDate.ToString("dd.MM.yyyy")
                : $"{minDate:dd.MM.yyyy} - {maxDate:dd.MM.yyyy}";
        }

        /// <summary>
        /// ЭКРАНИРОВАНИЕ СПЕЦСИМВОЛОВ ДЛЯ JSON
        /// </summary>
        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}