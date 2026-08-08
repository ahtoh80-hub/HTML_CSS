using System;
using System.Windows;
using IOPointManager.Interfaces;
using IOPointManager.Services;
using IOPointManager.ViewModels;
using IOPointManager.Views;
using Microsoft.Extensions.DependencyInjection;

namespace IOPointManager
{
    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            // Регистрация сервисов
            services.AddSingleton<IEventLogger, EventLogger>();
            services.AddSingleton<IIOPointRepository, InMemoryRepository>();
            services.AddSingleton<IExcelMappingService, ExcelMappingService>();
            services.AddSingleton<IAccessService, AccessService>();
            
            // Регистрация ViewModel
            services.AddSingleton<MainViewModel>();
            services.AddTransient<MappingViewModel>();
            services.AddTransient<AccessViewModel>();

            _serviceProvider = services.BuildServiceProvider();

            // Получаем ViewModel
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            
            // Создаем окно
            _mainWindow = new MainWindow();
            _mainWindow.DataContext = mainViewModel;
            
            // Подписываемся на события логгера
            var logger = _serviceProvider.GetRequiredService<IEventLogger>();
            if (logger is EventLogger eventLogger)
            {
                eventLogger.LogAdded += (s, args) =>
                {
                    _mainWindow?.Dispatcher.Invoke(() =>
                    {
                        _mainWindow.OnLogAdded(s, args);
                    });
                };
            }
            
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}