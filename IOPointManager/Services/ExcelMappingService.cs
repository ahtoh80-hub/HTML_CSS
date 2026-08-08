using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IOPointManager.Interfaces;
using IOPointManager.Models;
using OfficeOpenXml;

namespace IOPointManager.Services
{
    public class ExcelMappingService : IExcelMappingService
    {
        private readonly IEventLogger _logger;
        private readonly Dictionary<string, ExcelMappingTemplate> _templates = new();
        private readonly string _templatesDirectory;

        private readonly Dictionary<string, string> _fieldDescriptions = new()
        {
            { "Tag", "Уникальный тег устройства" },
            { "Area", "Номер технологической установки" },
            { "Title", "Номер установки/зоны" },
            { "Service", "Описание сигнала (на русском)" },
            { "ServiceEnglish", "Описание сигнала (на английском)" },
            { "InstrumentType", "Тип прибора" },
            { "System", "Тип системы управления" },
            { "IoType", "Тип сигнала ввода-вывода" },
            { "Location", "Место установки оборудования" },
            { "Controller", "Имя контроллера" },
            { "Pid", "Номер P&ID" },
            { "RangeMin", "Минимальное значение диапазона" },
            { "RangeMax", "Максимальное значение диапазона" },
            { "RangeUnit", "Единица измерения" },
            { "AlarmLL2", "Уставка LL2" },
            { "AlarmLL", "Уставка LL" },
            { "AlarmL", "Уставка L" },
            { "AlarmH", "Уставка H" },
            { "AlarmHH", "Уставка HH" },
            { "AlarmHH2", "Уставка HH2" },
            { "AlarmUnit", "Единица измерения уставок" },
            { "Column1", "Дополнительное поле 1" },
            { "Column2", "Дополнительное поле 2" },
            { "Column3", "Дополнительное поле 3" },
            { "Column4", "Дополнительное поле 4" },
            { "Column5", "Дополнительное поле 5" },
            { "Column6", "Дополнительное поле 6" },
            { "Column7", "Дополнительное поле 7" },
            { "Column8", "Дополнительное поле 8" },
            { "Column9", "Дополнительное поле 9" },
            { "Column10", "Дополнительное поле 10" }
        };

        public event EventHandler<LogEventArgs>? ProgressReport;

        public ExcelMappingService(IEventLogger logger)
        {
            _logger = logger;
            _templatesDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "IOPointManager",
                "MappingTemplates");
            Directory.CreateDirectory(_templatesDirectory);
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            try
            {
                foreach (var file in Directory.GetFiles(_templatesDirectory, "*.json"))
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var template = System.Text.Json.JsonSerializer.Deserialize<ExcelMappingTemplate>(json);
                        if (template != null && !string.IsNullOrEmpty(template.Name))
                        {
                            _templates[template.Name] = template;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Не удалось загрузить шаблон: {ex.Message}", "ExcelMapping");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка загрузки шаблонов: {ex.Message}", "ExcelMapping");
            }
        }

        public async Task<ExcelStructureInfo> AnalyzeExcelFileAsync(string filePath, int headerRow = 1)
        {
            ProgressReport?.Invoke(this, new LogEventArgs($"Анализ файла: {Path.GetFileName(filePath)}", EventType.Info, "ExcelMapping"));

            var result = new ExcelStructureInfo { HeaderRow = headerRow };

            await Task.Run(() =>
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                        throw new InvalidOperationException("Excel файл не содержит листов");

                    result.SheetName = worksheet.Name;
                    result.TotalRows = worksheet.Dimension?.Rows ?? 0;
                    result.TotalColumns = worksheet.Dimension?.Columns ?? 0;

                    for (int col = 1; col <= result.TotalColumns; col++)
                    {
                        var header = worksheet.Cells[headerRow, col].Text?.Trim();
                        if (string.IsNullOrEmpty(header))
                            header = $"Column{col}";

                        var colInfo = new ExcelColumnInfo
                        {
                            Name = header,
                            Index = col,
                            SampleValues = new List<object>()
                        };

                        int nullCount = 0;
                        bool isNumeric = true;
                        bool isDateTime = true;
                        int sampleRows = Math.Min(10, result.TotalRows - headerRow);

                        for (int row = headerRow + 1; row <= headerRow + sampleRows && row <= result.TotalRows; row++)
                        {
                            var cell = worksheet.Cells[row, col];
                            var value = cell.Value;

                            if (value == null || string.IsNullOrWhiteSpace(cell.Text))
                            {
                                nullCount++;
                                continue;
                            }

                            colInfo.SampleValues.Add(value);

                            if (isNumeric && !decimal.TryParse(value.ToString(), out _))
                                isNumeric = false;

                            if (isDateTime && !DateTime.TryParse(value.ToString(), out _))
                                isDateTime = false;
                        }

                        colInfo.NullCount = nullCount;
                        colInfo.IsNumeric = isNumeric;
                        colInfo.IsDateTime = isDateTime;
                        colInfo.DataType = isNumeric ? typeof(decimal) :
                                          isDateTime ? typeof(DateTime) : typeof(string);

                        result.Columns.Add(colInfo);
                    }

                    ProgressReport?.Invoke(this, new LogEventArgs($"Анализ завершен. Найдено {result.TotalColumns} колонок, {result.TotalRows} строк",
                        EventType.Info, "ExcelMapping"));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка анализа Excel: {ex.Message}", "ExcelMapping");
                    throw;
                }
            });

            return result;
        }

        public async Task<List<ExcelMappingSuggestion>> AutoDetectMappingAsync(string filePath, int headerRow = 1)
        {
            var suggestions = new List<ExcelMappingSuggestion>();
            var structure = await AnalyzeExcelFileAsync(filePath, headerRow);

            var fieldNames = _fieldDescriptions.Keys.ToList();

            foreach (var column in structure.Columns)
            {
                var colName = column.Name.Trim();

                var exactMatch = fieldNames.FirstOrDefault(f =>
                    string.Equals(f, colName, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    suggestions.Add(new ExcelMappingSuggestion
                    {
                        IOPointField = exactMatch,
                        ExcelColumn = colName,
                        Confidence = 100,
                        Reason = "Точное совпадение имени"
                    });
                    continue;
                }

                foreach (var field in fieldNames)
                {
                    var similarity = GetSimilarity(field.ToLowerInvariant(), colName.ToLowerInvariant());
                    if (similarity > 0.6)
                    {
                        suggestions.Add(new ExcelMappingSuggestion
                        {
                            IOPointField = field,
                            ExcelColumn = colName,
                            Confidence = (int)(similarity * 100),
                            Reason = $"Частичное совпадение ({similarity:P0})"
                        });
                    }
                }
            }

            var grouped = suggestions
                .GroupBy(s => s.IOPointField)
                .Select(g => g.OrderByDescending(s => s.Confidence).First())
                .OrderByDescending(s => s.Confidence)
                .ToList();

            return grouped;
        }

        public async Task<List<ExcelMappingSuggestion>> SuggestMappingAsync(string filePath, int headerRow = 1)
        {
            return await AutoDetectMappingAsync(filePath, headerRow);
        }

        public async Task<(List<IOPoint> points, ExcelMappingResult result)> ApplyMappingAsync(
            string filePath, ExcelMappingTemplate template, int startRow = 1, int? endRow = null)
        {
            var points = new List<IOPoint>();
            var result = new ExcelMappingResult
            {
                AppliedMappings = template.Mappings,
                IsValid = true,
                Confidence = 0
            };

            await Task.Run(() =>
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(filePath));
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                        throw new InvalidOperationException("Excel файл не содержит листов");

                    int headerRow = 1;
                    var headerMap = new Dictionary<string, int>();
                    for (int col = 1; col <= (worksheet.Dimension?.Columns ?? 0); col++)
                    {
                        var header = worksheet.Cells[headerRow, col].Text?.Trim();
                        if (!string.IsNullOrEmpty(header))
                            headerMap[header] = col;
                    }

                    var reverseMapping = new Dictionary<string, string>();
                    foreach (var mapping in template.Mappings)
                    {
                        if (headerMap.ContainsKey(mapping.Value))
                            reverseMapping[mapping.Value] = mapping.Key;
                    }

                    int maxRow = endRow ?? (worksheet.Dimension?.Rows ?? 0);
                    maxRow = Math.Min(maxRow, worksheet.Dimension?.Rows ?? 0);

                    var pointType = typeof(IOPoint);
                    var properties = pointType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .ToDictionary(p => p.Name, p => p);

                    for (int row = startRow + 1; row <= maxRow; row++)
                    {
                        var point = new IOPoint();
                        var hasData = false;
                        var errors = new List<string>();

                        foreach (var (excelColumn, ioField) in reverseMapping)
                        {
                            if (!headerMap.TryGetValue(excelColumn, out int col))
                                continue;

                            var cell = worksheet.Cells[row, col];
                            var cellValue = cell.Value?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(cellValue))
                                continue;

                            hasData = true;

                            if (properties.TryGetValue(ioField, out var prop))
                            {
                                try
                                {
                                    var converted = ConvertValue(cellValue, prop.PropertyType);
                                    prop.SetValue(point, converted);
                                }
                                catch (Exception ex)
                                {
                                    errors.Add($"Поле '{ioField}': {ex.Message}");
                                    for (int i = 1; i <= 10; i++)
                                    {
                                        var colProp = properties.GetValueOrDefault($"Column{i}");
                                        if (colProp != null && string.IsNullOrEmpty(colProp.GetValue(point)?.ToString()))
                                        {
                                            colProp.SetValue(point, $"ERR: {cellValue}");
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (hasData)
                        {
                            point.ImportSource = Path.GetFileName(filePath);
                            point.ImportRowNumber = row;
                            point.IsImportValid = errors.Count == 0;
                            point.ImportError = errors.Count > 0 ? string.Join("; ", errors) : null;

                            var validation = point.Validate();

                            if (errors.Count > 0 || !validation.IsValid)
                            {
                                result.ErrorRows++;
                                result.Errors.AddRange(errors);
                                result.Errors.AddRange(validation.Errors);
                            }
                            else
                            {
                                result.MappedRows++;
                            }

                            points.Add(point);
                        }
                    }

                    result.Confidence = result.MappedRows > 0
                        ? (int)((double)result.MappedRows / (result.MappedRows + result.ErrorRows) * 100)
                        : 0;
                    result.IsValid = result.Errors.Count == 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка применения маппинга: {ex.Message}", "ExcelMapping");
                    throw;
                }
            });

            return (points, result);
        }

        private object? ConvertValue(string value, Type targetType)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            targetType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(int) || targetType == typeof(int?))
                return int.Parse(value);

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
                return decimal.Parse(value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);

            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return value.Equals("1") || value.Equals("true", StringComparison.OrdinalIgnoreCase);

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                return DateTime.Parse(value);

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);

            return Convert.ChangeType(value, targetType);
        }

        private double GetSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0;

            int maxLen = Math.Max(s1.Length, s2.Length);
            int distance = LevenshteinDistance(s1, s2);
            return 1.0 - (double)distance / maxLen;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        public async Task SaveMappingTemplateAsync(ExcelMappingTemplate template)
        {
            if (string.IsNullOrEmpty(template.Name))
                throw new ArgumentException("Имя шаблона не может быть пустым");

            template.ModifiedAt = DateTime.Now;
            _templates[template.Name] = template;

            var filePath = Path.Combine(_templatesDirectory, $"{template.Name}.json");
            var json = System.Text.Json.JsonSerializer.Serialize(template, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInfo($"Шаблон маппинга '{template.Name}' сохранен", "ExcelMapping");
        }

        public Task<ExcelMappingTemplate?> LoadMappingTemplateAsync(string name)
        {
            if (_templates.TryGetValue(name, out var template))
                return Task.FromResult<ExcelMappingTemplate?>(template);

            return Task.FromResult<ExcelMappingTemplate?>(null);
        }

        public Task<IEnumerable<ExcelMappingTemplate>> GetMappingTemplatesAsync()
        {
            return Task.FromResult(_templates.Values.AsEnumerable());
        }

        public Task DeleteMappingTemplateAsync(string name)
        {
            if (_templates.Remove(name))
            {
                var filePath = Path.Combine(_templatesDirectory, $"{name}.json");
                if (File.Exists(filePath))
                    File.Delete(filePath);
                _logger.LogInfo($"Шаблон маппинга '{name}' удален", "ExcelMapping");
            }

            return Task.CompletedTask;
        }

        public ExcelMappingResult ValidateMapping(ExcelMappingTemplate template)
        {
            var result = new ExcelMappingResult { IsValid = true };

            if (template == null)
            {
                result.Errors.Add("Шаблон не может быть null");
                result.IsValid = false;
                return result;
            }

            if (string.IsNullOrEmpty(template.Name))
            {
                result.Errors.Add("Имя шаблона не может быть пустым");
                result.IsValid = false;
            }

            if (template.Mappings == null || template.Mappings.Count == 0)
            {
                result.Warnings.Add("Шаблон не содержит маппингов");
            }

            var fieldNames = _fieldDescriptions.Keys.ToHashSet();
            foreach (var mapping in template.Mappings)
            {
                if (!fieldNames.Contains(mapping.Key))
                {
                    result.Errors.Add($"Поле '{mapping.Key}' не существует в IOPoint");
                    result.IsValid = false;
                }
            }

            result.Confidence = result.IsValid ? 100 : 0;
            return result;
        }

        public Dictionary<string, string> GetFieldDescriptions() => _fieldDescriptions;

        public List<string> GetFieldNames() => _fieldDescriptions.Keys.ToList();
    }
}