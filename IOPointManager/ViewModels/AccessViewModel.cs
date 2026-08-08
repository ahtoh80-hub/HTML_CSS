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
using Microsoft.Win32;  // <-- ДОБАВЬТЕ ЭТУ СТРОКУ

namespace IOPointManager.ViewModels
{
    public class AccessViewModel : INotifyPropertyChanged
    {
        private readonly IAccessService _accessService;
        private readonly IEventLogger _logger;

        private string _connectionString = string.Empty;
        private bool _isConnected;
        private string _tableName = "IOPoints";
        private string _selectedTable = string.Empty;
        private ObservableCollection<string> _tables = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ConnectionString
        {
            get => _connectionString;
            set { _connectionString = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConnectionStatus)); }
        }

        public string ConnectionStatus => IsConnected ? "Подключено" : "Не подключено";

        public string TableName
        {
            get => _tableName;
            set { _tableName = value; OnPropertyChanged(); }
        }

        public string SelectedTable
        {
            get => _selectedTable;
            set { _selectedTable = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Tables
        {
            get => _tables;
            set { _tables = value; OnPropertyChanged(); }
        }

        public bool CanConnect => !string.IsNullOrEmpty(ConnectionString);

        public ICommand SelectDatabaseCommand { get; }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand CreateTableCommand { get; }
        public ICommand DropTableCommand { get; }
        public ICommand BackupTableCommand { get; }
        public ICommand ClearTableCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand RefreshTablesCommand { get; }
        public ICommand CloseCommand { get; }

        public event EventHandler<AccessImportEventArgs>? ImportRequested;
        public event EventHandler<AccessExportEventArgs>? ExportRequested;
        public event EventHandler? CloseRequested;

        public AccessViewModel(IAccessService accessService, IEventLogger logger)
        {
            _accessService = accessService;
            _logger = logger;

            _accessService.ProgressReport += (s, e) => _logger.Log(e);

            SelectDatabaseCommand = new RelayCommand(_ => SelectDatabase());
            ConnectCommand = new RelayCommand(_ => Connect(), _ => CanConnect);
            DisconnectCommand = new RelayCommand(_ => Disconnect(), _ => IsConnected);
            CreateTableCommand = new RelayCommand(async _ => await CreateTableAsync(), _ => IsConnected);
            DropTableCommand = new RelayCommand(async _ => await DropTableAsync(), _ => IsConnected && !string.IsNullOrEmpty(SelectedTable));
            BackupTableCommand = new RelayCommand(async _ => await BackupTableAsync(), _ => IsConnected && !string.IsNullOrEmpty(SelectedTable));
            ClearTableCommand = new RelayCommand(async _ => await ClearTableAsync(), _ => IsConnected && !string.IsNullOrEmpty(SelectedTable));
            ImportCommand = new RelayCommand(async _ => await ImportFromAccessAsync(), _ => IsConnected && !string.IsNullOrEmpty(SelectedTable));
            ExportCommand = new RelayCommand(async _ => await ExportToAccessAsync(), _ => IsConnected);
            RefreshTablesCommand = new RelayCommand(async _ => await RefreshTablesAsync(), _ => IsConnected);
            CloseCommand = new RelayCommand(_ => CloseRequested?.Invoke(this, EventArgs.Empty));
        }

private void SelectDatabase()
{
    var dialog = new Microsoft.Win32.OpenFileDialog
    {
        Filter = "Access Database|*.accdb;*.mdb|All Files|*.*",
        Title = "Выберите файл базы данных Access"
    };

    if (dialog.ShowDialog() == true)
    {
        ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dialog.FileName};";
        _logger.LogInfo($"Выбран файл базы данных: {dialog.FileName}", "AccessViewModel");
    }
}

        private void Connect()
        {
            try
            {
                if (_accessService.Connect(ConnectionString))
                {
                    IsConnected = true;
                    _logger.LogInfo("Подключение к Access установлено", "AccessViewModel");
                    Task.Run(async () => await RefreshTablesAsync());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка подключения: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Disconnect()
        {
            _accessService.Disconnect();
            IsConnected = false;
            Tables.Clear();
            _logger.LogInfo("Отключено от Access", "AccessViewModel");
        }

        private async Task RefreshTablesAsync()
        {
            try
            {
                var tables = await _accessService.GetTableNamesAsync();
                Tables.Clear();
                foreach (var table in tables)
                    Tables.Add(table);
                _logger.LogInfo($"Обновлен список таблиц: {Tables.Count} таблиц", "AccessViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения списка таблиц: {ex.Message}", "AccessViewModel");
            }
        }

        private async Task CreateTableAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TableName))
                {
                    MessageBox.Show("Введите имя таблицы", "Предупреждение", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = await _accessService.CreateTableAsync(TableName);
                if (result)
                {
                    await RefreshTablesAsync();
                    _logger.LogInfo($"Таблица '{TableName}' создана", "AccessViewModel");
                    MessageBox.Show($"Таблица '{TableName}' успешно создана!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка создания таблицы: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DropTableAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTable))
                    return;

                var result = MessageBox.Show($"Удалить таблицу '{SelectedTable}'?\nЭто действие необратимо!",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _accessService.DropTableAsync(SelectedTable);
                    await RefreshTablesAsync();
                    _logger.LogWarning($"Таблица '{SelectedTable}' удалена", "AccessViewModel");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления таблицы: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task BackupTableAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTable))
                    return;

                var result = await _accessService.BackupTableAsync(SelectedTable);
                if (result)
                {
                    await RefreshTablesAsync();
                    _logger.LogInfo($"Резервная копия таблицы '{SelectedTable}' создана", "AccessViewModel");
                    MessageBox.Show($"Резервная копия таблицы '{SelectedTable}' создана!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка создания резервной копии: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClearTableAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTable))
                    return;

                var result = MessageBox.Show($"Очистить таблицу '{SelectedTable}'?\nВсе данные будут удалены!",
                    "Подтверждение очистки", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    await _accessService.ClearTableAsync(SelectedTable);
                    _logger.LogWarning($"Таблица '{SelectedTable}' очищена", "AccessViewModel");
                    MessageBox.Show($"Таблица '{SelectedTable}' очищена!", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка очистки таблицы: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ImportFromAccessAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedTable))
                {
                    MessageBox.Show("Выберите таблицу для импорта", "Предупреждение", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var points = await _accessService.ReadPointsAsync(SelectedTable);
                var list = points.ToList();

                if (!list.Any())
                {
                    MessageBox.Show($"В таблице '{SelectedTable}' нет данных.", 
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                ImportRequested?.Invoke(this, new AccessImportEventArgs(list));
                _logger.LogInfo($"Импортировано {list.Count} точек из Access", "AccessViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка импорта из Access: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportToAccessAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(TableName))
                {
                    MessageBox.Show("Введите имя таблицы для экспорта", "Предупреждение", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ExportRequested?.Invoke(this, new AccessExportEventArgs(TableName));
                _logger.LogInfo("Запрос на экспорт в Access отправлен", "AccessViewModel");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка экспорта в Access: {ex.Message}", "AccessViewModel");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class AccessImportEventArgs : EventArgs
    {
        public List<IOPoint> Points { get; }
        public AccessImportEventArgs(List<IOPoint> points) => Points = points;
    }

    public class AccessExportEventArgs : EventArgs
    {
        public string TableName { get; }
        public AccessExportEventArgs(string tableName) => TableName = tableName;
    }
}