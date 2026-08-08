using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IOPointManager.Interfaces;
using IOPointManager.Models;
using Microsoft.Win32;

namespace IOPointManager.ViewModels
{
    public class MappingViewModel : INotifyPropertyChanged
    {
        private readonly IExcelMappingService _excelService;
        private readonly IEventLogger _logger;

        private string _fileName = string.Empty;
        private bool _isFileSelected;
        private bool _isAnalyzed;
        private string _startRow = "1";
        private string _endRow = string.Empty;
        private string _templateName = string.Empty;
        private IOPointField? _selectedIOPointField;
        private ExcelColumnInfo? _selectedExcelColumn;

        public ObservableCollection<IOPointField> IOPointFields { get; set; } = new();
        public ObservableCollection<ExcelColumnInfo> ExcelColumns { get; set; } = new();
        public ObservableCollection<MappingBinding> Bindings { get; set; } = new();

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        public bool IsFileSelected
        {
            get => _isFileSelected;
            set { _isFileSelected = value; OnPropertyChanged(); }
        }

        public bool IsAnalyzed
        {
            get => _isAnalyzed;
            set { _isAnalyzed = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanApply)); }
        }

        public string StartRow
        {
            get => _startRow;
            set { _startRow = value; OnPropertyChanged(); }
        }

        public string EndRow
        {
            get => _endRow;
            set { _endRow = value; OnPropertyChanged(); }
        }

        public string TemplateName
        {
            get => _templateName;
            set { _templateName = value; OnPropertyChanged(); }
        }

        public IOPointField? SelectedIOPointField
        {
            get => _selectedIOPointField;
            set { _selectedIOPointField = value; OnPropertyChanged(); }
        }

        public ExcelColumnInfo? SelectedExcelColumn
        {
            get => _selectedExcelColumn;
            set { _selectedExcelColumn = value; OnPropertyChanged(); }
        }

        public bool CanApply => IsAnalyzed && Bindings.Any();

        public ICommand SelectFileCommand { get; }
        public ICommand AnalyzeCommand { get; }
        public ICommand BindCommand { get; }
        public ICommand UnbindCommand { get; }
        public ICommand AutoDetectCommand { get; }
        public ICommand AutoBindCommand { get; }
        public ICommand ApplyMappingCommand { get; }
        public ICommand SaveTemplateCommand { get; }
        public ICommand LoadTemplateCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<List<IOPoint>>? MappingApplied;
        public event EventHandler? Cancelled;
        public event PropertyChangedEventHandler? PropertyChanged;

        public MappingViewModel(IExcelMappingService excelService, IEventLogger logger)
        {
            _excelService = excelService;
            _logger = logger;

            var fields = excelService.GetFieldDescriptions();
            foreach (var field in fields)
                IOPointFields.Add(new IOPointField { Name = field.Key, Description = field.Value });

            SelectFileCommand = new RelayCommand(_ => SelectFile());
            AnalyzeCommand = new RelayCommand(async _ => await AnalyzeFileAsync(), _ => IsFileSelected);
            BindCommand = new RelayCommand(_ => BindMapping(), _ => SelectedIOPointField != null && SelectedExcelColumn != null);
            UnbindCommand = new RelayCommand(_ => UnbindMapping(), _ => SelectedIOPointField != null);
            AutoDetectCommand = new RelayCommand(async _ => await AutoDetectAsync(), _ => IsFileSelected);
            AutoBindCommand = new RelayCommand(async _ => await AutoBindAsync(), _ => IsFileSelected);
            ApplyMappingCommand = new RelayCommand(async _ => await ApplyMappingAsync(), _ => CanApply);
            SaveTemplateCommand = new RelayCommand(async _ => await SaveTemplateAsync(), _ => Bindings.Any());
            LoadTemplateCommand = new RelayCommand(async _ => await LoadTemplateAsync());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void SelectFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*",
                Title = "Выберите Excel файл"
            };

            if (dialog.ShowDialog() == true)
            {
                FileName = dialog.FileName;
                IsFileSelected = true;
                IsAnalyzed = false;
                ExcelColumns.Clear();
                Bindings.Clear();
                _logger.LogInfo($"Выбран файл: {FileName}", "MappingViewModel");
            }
        }

        private async Task AnalyzeFileAsync()
        {
            try
            {
                var structure = await _excelService.AnalyzeExcelFileAsync(FileName);
                ExcelColumns.Clear();
                foreach (var column in structure.Columns)
                    ExcelColumns.Add(column);

                IsAnalyzed = true;
                _logger.LogInfo($"Проанализировано {structure.TotalColumns} колонок", "MappingViewModel");
                
                MessageBox.Show($"Анализ завершен!\nНайдено {structure.TotalColumns} колонок и {structure.TotalRows} строк данных.",
                    "Анализ Excel", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка анализа: {ex.Message}", "MappingViewModel");
                MessageBox.Show($"Ошибка анализа: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BindMapping()
        {
            if (SelectedIOPointField == null || SelectedExcelColumn == null)
                return;

            if (Bindings.Any(b => b.IOPointField.Name == SelectedIOPointField.Name))
            {
                MessageBox.Show($"Поле '{SelectedIOPointField.Name}' уже привязано.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Bindings.Add(new MappingBinding
            {
                IOPointField = SelectedIOPointField,
                ExcelColumn = SelectedExcelColumn,
                Confidence = 100
            });

            _logger.LogInfo($"Привязано: {SelectedIOPointField.Name} -> {SelectedExcelColumn.Name}", "MappingViewModel");
            OnPropertyChanged(nameof(CanApply));
        }

        private void UnbindMapping()
        {
            if (SelectedIOPointField == null)
                return;

            var binding = Bindings.FirstOrDefault(b => b.IOPointField.Name == SelectedIOPointField.Name);
            if (binding != null)
            {
                Bindings.Remove(binding);
                _logger.LogInfo($"Отвязано: {SelectedIOPointField.Name}", "MappingViewModel");
                OnPropertyChanged(nameof(CanApply));
            }
        }

        private async Task AutoDetectAsync()
        {
            try
            {
                if (!IsAnalyzed)
                    await AnalyzeFileAsync();

                var suggestions = await _excelService.AutoDetectMappingAsync(FileName);
                Bindings.Clear();

                foreach (var suggestion in suggestions)
                {
                    var field = IOPointFields.FirstOrDefault(f => f.Name == suggestion.IOPointField);
                    var column = ExcelColumns.FirstOrDefault(c => c.Name == suggestion.ExcelColumn);

                    if (field != null && column != null && suggestion.Confidence > 50)
                    {
                        Bindings.Add(new MappingBinding
                        {
                            IOPointField = field,
                            ExcelColumn = column,
                            Confidence = suggestion.Confidence
                        });
                    }
                }

                _logger.LogInfo($"Автодетект: найдено {Bindings.Count} привязок", "MappingViewModel");
                OnPropertyChanged(nameof(CanApply));

                MessageBox.Show($"Автодетект завершен!\nНайдено {Bindings.Count} сопоставлений.",
                    "Автодетект", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка автодетекта: {ex.Message}", "MappingViewModel");
            }
        }

        private async Task AutoBindAsync()
        {
            try
            {
                if (!IsAnalyzed)
                    await AnalyzeFileAsync();

                Bindings.Clear();

                foreach (var field in IOPointFields)
                {
                    var column = ExcelColumns.FirstOrDefault(c =>
                        string.Equals(c.Name, field.Name, StringComparison.OrdinalIgnoreCase));

                    if (column != null)
                    {
                        Bindings.Add(new MappingBinding
                        {
                            IOPointField = field,
                            ExcelColumn = column,
                            Confidence = 100
                        });
                    }
                }

                _logger.LogInfo($"Авто-привязка: найдено {Bindings.Count} совпадений", "MappingViewModel");
                OnPropertyChanged(nameof(CanApply));

                if (Bindings.Count == 0)
                {
                    MessageBox.Show("Не найдено совпадений по имени колонок.", 
                        "Авто-привязка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка авто-привязки: {ex.Message}", "MappingViewModel");
            }
        }

        private async Task ApplyMappingAsync()
        {
            try
            {
                var template = new ExcelMappingTemplate
                {
                    Name = string.IsNullOrEmpty(TemplateName) ? $"Template_{DateTime.Now:yyyyMMdd_HHmmss}" : TemplateName,
                    Description = $"Маппинг для файла {FileName}",
                    Mappings = Bindings.ToDictionary(b => b.IOPointField.Name, b => b.ExcelColumn.Name)
                };

                int startRow = int.TryParse(StartRow, out var s) ? Math.Max(1, s) : 1;
                int? endRow = int.TryParse(EndRow, out var e) ? e : (int?)null;

                var (points, result) = await _excelService.ApplyMappingAsync(FileName, template, startRow, endRow);

                MappingApplied?.Invoke(this, points);

                _logger.LogInfo($"Маппинг применен. Загружено {points.Count} точек, ошибок: {result.ErrorRows}", "MappingViewModel");

                if (result.ErrorRows > 0)
                {
                    MessageBox.Show($"Импорт завершен с ошибками.\n" +
                        $"Загружено: {result.MappedRows} точек\n" +
                        $"Ошибок: {result.ErrorRows}\n\n" +
                        $"Данные с ошибками записаны в дополнительные поля (Column1-Column10).",
                        "Результат импорта", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Импорт завершен успешно!\nЗагружено {result.MappedRows} точек.",
                        "Результат импорта", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка применения маппинга: {ex.Message}", "MappingViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveTemplateAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(TemplateName))
                    TemplateName = $"Template_{DateTime.Now:yyyyMMdd_HHmmss}";

                var template = new ExcelMappingTemplate
                {
                    Name = TemplateName,
                    Description = $"Шаблон маппинга от {DateTime.Now:dd.MM.yyyy HH:mm}",
                    Mappings = Bindings.ToDictionary(b => b.IOPointField.Name, b => b.ExcelColumn.Name)
                };

                await _excelService.SaveMappingTemplateAsync(template);
                _logger.LogInfo($"Шаблон '{TemplateName}' сохранен", "MappingViewModel");
                MessageBox.Show($"Шаблон '{TemplateName}' успешно сохранен!",
                    "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка сохранения шаблона: {ex.Message}", "MappingViewModel");
            }
        }

        private async Task LoadTemplateAsync()
        {
            try
            {
                var templates = await _excelService.GetMappingTemplatesAsync();
                var templateList = templates.ToList();

                if (!templateList.Any())
                {
                    MessageBox.Show("Нет сохраненных шаблонов.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var names = templateList.Select(t => t.Name).ToList();
                var selectedName = names.FirstOrDefault();
                
                if (string.IsNullOrEmpty(selectedName))
                    return;

                var template = await _excelService.LoadMappingTemplateAsync(selectedName);
                if (template != null)
                {
                    Bindings.Clear();
                    foreach (var mapping in template.Mappings)
                    {
                        var field = IOPointFields.FirstOrDefault(f => f.Name == mapping.Key);
                        var column = ExcelColumns.FirstOrDefault(c => c.Name == mapping.Value);
                        if (field != null && column != null)
                        {
                            Bindings.Add(new MappingBinding
                            {
                                IOPointField = field,
                                ExcelColumn = column,
                                Confidence = 100
                            });
                        }
                    }
                    TemplateName = template.Name;
                    _logger.LogInfo($"Шаблон '{template.Name}' загружен", "MappingViewModel");
                    OnPropertyChanged(nameof(CanApply));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка загрузки шаблона: {ex.Message}", "MappingViewModel");
            }
        }

        private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class IOPointField
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DisplayName => $"{Name} - {Description}";
    }

    public class MappingBinding
    {
        public IOPointField IOPointField { get; set; } = new();
        public ExcelColumnInfo ExcelColumn { get; set; } = new();
        public int Confidence { get; set; } = 100;
        public string DisplayName => $"{IOPointField.Name} ↔ {ExcelColumn.Name} ({Confidence}%)";
    }
}