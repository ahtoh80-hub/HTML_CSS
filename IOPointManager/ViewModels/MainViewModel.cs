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
using IOPointManager.Services;
using IOPointManager.Views;
using Microsoft.Win32;

namespace IOPointManager.ViewModels
{
    /// <summary>
    /// Вспомогательный класс для команд RelayCommand
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
    }

    /// <summary>
    /// Главная модель представления приложения
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IEventLogger _logger;
        private readonly IIOPointRepository _repository;
        private readonly IExcelMappingService _excelService;
        private readonly IAccessService _accessService;

        private ObservableCollection<IOPoint> _points = new();
        private ObservableCollection<IOPoint> _filteredPoints = new();
        private string _searchText = string.Empty;
        private IOPoint? _selectedPoint;
        private bool _isLoading;
        private int _totalCount;
        private int _filteredCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        // ==================== СВОЙСТВА ====================

        public ObservableCollection<IOPoint> Points
        {
            get => _points;
            set
            {
                _points = value;
                OnPropertyChanged();
                UpdateCounts();
            }
        }

        public ObservableCollection<IOPoint> FilteredPoints
        {
            get => _filteredPoints;
            set
            {
                _filteredPoints = value;
                OnPropertyChanged();
                UpdateCounts();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public IOPoint? SelectedPoint
        {
            get => _selectedPoint;
            set
            {
                _selectedPoint = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                OnPropertyChanged();
            }
        }

        public int FilteredCount
        {
            get => _filteredCount;
            set
            {
                _filteredCount = value;
                OnPropertyChanged();
            }
        }

        // Коллекции для ComboBox
        public List<SystemType> SystemTypes => Enum.GetValues(typeof(SystemType)).Cast<SystemType>().ToList();
        public List<IOPointStatus> StatusTypes => Enum.GetValues(typeof(IOPointStatus)).Cast<IOPointStatus>().ToList();

        // Журнал событий
        public ObservableCollection<LogEventArgs> Logs { get; } = new();

        // ==================== КОМАНДЫ ====================

        public ICommand LoadDataCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddPointCommand { get; }
        public ICommand EditPointCommand { get; }
        public ICommand DeletePointCommand { get; }
        public ICommand OpenExcelCommand { get; }
        public ICommand AccessCommand { get; }
        public ICommand ShowMappingDialogCommand { get; }
        public ICommand ValidateAllCommand { get; }
        public ICommand ValidateAlarmsCommand { get; }
        public ICommand ClearLogCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }

        // ==================== КОНСТРУКТОРЫ ====================

        /// <summary>
        /// Конструктор по умолчанию для тестирования
        /// </summary>
        public MainViewModel()
        {
            _logger = new EventLogger();
            _repository = new InMemoryRepository();
            _excelService = new ExcelMappingService(_logger);
            _accessService = new AccessService(_logger);

            InitializeCommands();
            InitializeLogging();
            LoadData();
            
            _logger.LogInfo("Приложение запущено (конструктор по умолчанию)", "MainViewModel");
        }

        /// <summary>
        /// Основной конструктор для DI
        /// </summary>
        public MainViewModel(
            IEventLogger logger,
            IIOPointRepository repository,
            IExcelMappingService excelService,
            IAccessService accessService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
            _accessService = accessService ?? throw new ArgumentNullException(nameof(accessService));

            InitializeCommands();
            InitializeLogging();
            LoadData();
            
            _logger.LogInfo("Приложение запущено (DI конструктор)", "MainViewModel");
        }

        // ==================== ИНИЦИАЛИЗАЦИЯ ====================

        private void InitializeCommands()
        {
            LoadDataCommand = new RelayCommand(_ => LoadData());
            RefreshCommand = new RelayCommand(_ => LoadData());
            AddPointCommand = new RelayCommand(_ => AddNewPoint());
            EditPointCommand = new RelayCommand(_ => EditSelectedPoint(), _ => SelectedPoint != null);
            DeletePointCommand = new RelayCommand(_ => DeleteSelectedPoint(), _ => SelectedPoint != null);
            OpenExcelCommand = new RelayCommand(_ => OpenExcel());
            AccessCommand = new RelayCommand(_ => OpenAccessDialog());
            ShowMappingDialogCommand = new RelayCommand(_ => ShowMappingDialog());
            ValidateAllCommand = new RelayCommand(_ => ValidateAll());
            ValidateAlarmsCommand = new RelayCommand(_ => ValidateAlarms());
            ClearLogCommand = new RelayCommand(_ => ClearLog());
            ExitCommand = new RelayCommand(_ => Application.Current?.Shutdown());
            AboutCommand = new RelayCommand(_ => ShowAboutDialog());
        }

        private void InitializeLogging()
        {
            // Подписываемся на события репозитория
            if (_repository is InMemoryRepository memoryRepo)
            {
                memoryRepo.LogEvent += (s, e) => _logger.Log(e);
            }

            // Подписываемся на события логгера
            if (_logger is EventLogger eventLogger)
            {
                eventLogger.LogAdded += (s, e) =>
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, e);
                        if (Logs.Count > 1000)
                            Logs.RemoveAt(Logs.Count - 1);
                    });
                };
            }
        }

        // ==================== МЕТОДЫ ЗАГРУЗКИ ДАННЫХ ====================

        private void LoadData()
        {
            try
            {
                IsLoading = true;
                var points = _repository.GetAllAsync().Result;
                
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Points.Clear();
                    foreach (var point in points)
                    {
                        Points.Add(point);
                    }
                    UpdateCounts();
                    ApplyFilter();
                });

                _logger.LogInfo($"Загружено {Points.Count} точек", "MainViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка загрузки данных: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateCounts()
        {
            TotalCount = Points?.Count ?? 0;
            FilteredCount = FilteredPoints?.Count ?? 0;
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredPoints = new ObservableCollection<IOPoint>(Points);
            }
            else
            {
                var term = SearchText.ToLowerInvariant();
                var filtered = Points.Where(p =>
                    (p.Tag?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Service?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.InstrumentType?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Title?.ToLowerInvariant().Contains(term) ?? false) ||
                    (p.Area?.ToString().Contains(term) ?? false)
                ).ToList();

                FilteredPoints = new ObservableCollection<IOPoint>(filtered);
            }

            FilteredCount = FilteredPoints.Count;
        }

        // ==================== КОМАНДЫ РАБОТЫ С ТОЧКАМИ ====================

        private void AddNewPoint()
        {
            try
            {
                var point = new IOPoint
                {
                    Tag = $"NEW-{DateTime.Now:yyyyMMdd-HHmmss}",
                    Status = IOPointStatus.Active,
                    CreatedAt = DateTime.Now,
                    Service = "Новая точка",
                    DataQualityScore = 100
                };

                // Добавляем в репозиторий
                _repository.AddAsync(point).Wait();

                // Добавляем в коллекцию
                Points.Add(point);
                FilteredPoints = new ObservableCollection<IOPoint>(Points);
                SelectedPoint = point;
                UpdateCounts();

                _logger.LogInfo($"Создана новая точка: {point.Tag}", "MainViewModel");
                MessageBox.Show($"Создана новая точка: {point.Tag}", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка создания точки: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка создания точки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSelectedPoint()
        {
            if (SelectedPoint == null)
            {
                MessageBox.Show("Выберите точку для редактирования.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _logger.LogInfo($"Редактирование точки: {SelectedPoint.Tag}", "MainViewModel");

                // Здесь будет открываться диалог редактирования
                MessageBox.Show(
                    $"Редактирование точки: {SelectedPoint.Tag}\n\n" +
                    $"Service: {SelectedPoint.Service}\n" +
                    $"InstrumentType: {SelectedPoint.InstrumentType}\n" +
                    $"System: {SelectedPoint.System}\n" +
                    $"Status: {SelectedPoint.Status}\n\n" +
                    "В реальном приложении здесь будет диалог редактирования всех полей.",
                    "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка редактирования: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSelectedPoint()
        {
            if (SelectedPoint == null)
            {
                MessageBox.Show("Выберите точку для удаления.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить точку '{SelectedPoint.Tag}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _repository.DeleteAsync(SelectedPoint.Id).Wait();
                    Points.Remove(SelectedPoint);
                    FilteredPoints = new ObservableCollection<IOPoint>(Points);
                    SelectedPoint = null;
                    UpdateCounts();

                    _logger.LogWarning($"Точка удалена", "MainViewModel");
                    MessageBox.Show("Точка удалена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка удаления: {ex.Message}", "MainViewModel");
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ==================== КОМАНДЫ ИМПОРТА/ЭКСПОРТА ====================

        private void OpenExcel()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Excel Files|*.xlsx;*.xls|All Files|*.*",
                    Title = "Выберите Excel файл для импорта"
                };

                if (dialog.ShowDialog() == true)
                {
                    _logger.LogInfo($"Выбран файл: {dialog.FileName}", "MainViewModel");

                    // Открываем диалог маппинга
                    var mappingDialog = new MappingDialog(_excelService, _logger);
                    mappingDialog.Owner = Application.Current.MainWindow;

                    if (mappingDialog.ShowDialog() == true)
                    {
                        var points = mappingDialog.GetImportedPoints();
                        if (points.Any())
                        {
                            if (_repository is InMemoryRepository repo)
                            {
                                repo.AddRange(points);
                                LoadData();
                                _logger.LogInfo($"Импортировано {points.Count} точек из Excel", "MainViewModel");
                                MessageBox.Show($"Импортировано {points.Count} точек из файла '{dialog.SafeFileName}'.",
                                    "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка импорта Excel: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAccessDialog()
        {
            try
            {
                var dialog = new AccessDialog(_accessService, _logger);
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    if (dialog.ImportedPoints != null && dialog.ImportedPoints.Any())
                    {
                        // Импорт из Access
                        if (_repository is InMemoryRepository repo)
                        {
                            repo.Clear();
                            repo.AddRange(dialog.ImportedPoints);
                            LoadData();
                            _logger.LogInfo($"Импортировано {dialog.ImportedPoints.Count} точек из Access", "MainViewModel");
                            MessageBox.Show($"Импортировано {dialog.ImportedPoints.Count} точек.",
                                "Импорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else if (!string.IsNullOrEmpty(dialog.ExportTableName))
                    {
                        // Экспорт в Access
                        var points = Points.ToList();
                        if (!points.Any())
                        {
                            MessageBox.Show("Нет данных для экспорта.",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        var count = _accessService.WritePointsAsync(dialog.ExportTableName, points).Result;
                        _logger.LogInfo($"Экспортировано {count} точек в Access", "MainViewModel");
                        MessageBox.Show($"Экспортировано {count} точек в таблицу '{dialog.ExportTableName}'.",
                            "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка работы с Access: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowMappingDialog()
        {
            _logger.LogInfo("Открытие диалога маппинга", "MainViewModel");
            MessageBox.Show(
                "Диалог управления шаблонами маппинга.\n\n" +
                "Здесь можно:\n" +
                "• Создавать новые шаблоны маппинга\n" +
                "• Редактировать существующие шаблоны\n" +
                "• Удалять шаблоны\n" +
                "• Применять шаблоны к Excel файлам",
                "Управление маппингом", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================== КОМАНДЫ ВАЛИДАЦИИ ====================

        private void ValidateAll()
        {
            try
            {
                IsLoading = true;
                int errorCount = 0;
                int warningCount = 0;
                int invalidPoints = 0;

                foreach (var point in Points)
                {
                    var result = point.Validate();
                    if (!result.IsValid)
                    {
                        invalidPoints++;
                        errorCount += result.Errors.Count;
                        warningCount += result.Warnings.Count;
                    }
                }

                _logger.LogInfo(
                    $"Валидация завершена. Ошибок: {errorCount}, Предупреждений: {warningCount}, Невалидных точек: {invalidPoints}",
                    "MainViewModel");

                var message = $"Валидация завершена.\n" +
                    $"Всего точек: {Points.Count}\n" +
                    $"Невалидных точек: {invalidPoints}\n" +
                    $"Ошибок: {errorCount}\n" +
                    $"Предупреждений: {warningCount}";

                if (invalidPoints > 0)
                {
                    // Показываем список невалидных точек
                    var invalidList = Points.Where(p => !p.IsValid).Take(10);
                    message += "\n\nНевалидные точки:\n";
                    foreach (var p in invalidList)
                    {
                        message += $"• {p.Tag}: {p.ValidationErrorCount} ошибок\n";
                    }
                    if (Points.Count(p => !p.IsValid) > 10)
                        message += $"\n... и еще {Points.Count(p => !p.IsValid) - 10} точек";
                }

                MessageBox.Show(message, "Результат валидации", MessageBoxButton.OK,
                    invalidPoints > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка валидации: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка валидации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ValidateAlarms()
        {
            try
            {
                IsLoading = true;
                var invalidPoints = _repository.GetInvalidAlarmsAsync().Result;
                var list = invalidPoints.ToList();

                if (!list.Any())
                {
                    _logger.LogInfo("Проверка уставок: все точки валидны", "MainViewModel");
                    MessageBox.Show("Все точки имеют корректную иерархию уставок LL2 < LL < L < H < HH < HH2.",
                        "Проверка уставок", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var message = $"Найдено {list.Count} точек с некорректными уставками:\n\n";
                foreach (var point in list.Take(10))
                {
                    var result = point.ValidateAlarmHierarchy();
                    var errors = string.Join("; ", result.Errors);
                    message += $"• {point.Tag}: {errors}\n";
                }
                if (list.Count > 10)
                    message += $"\n... и еще {list.Count - 10} точек";

                _logger.LogWarning($"Найдено {list.Count} точек с некорректными уставками", "MainViewModel");
                MessageBox.Show(message, "Некорректные уставки",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка проверки уставок: {ex.Message}", "MainViewModel");
                MessageBox.Show($"Ошибка проверки уставок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ==================== ВСПОМОГАТЕЛЬНЫЕ КОМАНДЫ ====================

        private void ClearLog()
        {
            _logger.Clear();
            Logs.Clear();
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show(
                "═══════════════════════════════════════\n" +
                "        IOPoint Manager v1.0\n" +
                "═══════════════════════════════════════\n\n" +
                "Управление точками ввода-вывода\n" +
                "в системах промышленной автоматизации\n\n" +
                "Разработано с использованием:\n" +
                "• .NET 8 WPF\n" +
                "• EPPlus (Excel)\n" +
                "• System.Data.OleDb (Access)\n\n" +
                "═══════════════════════════════════════\n" +
                "© 2024 Все права защищены",
                "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==================== INotifyPropertyChanged ====================

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}