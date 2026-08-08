using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using IOPointManager.Interfaces;
using IOPointManager.Models;
using IOPointManager.ViewModels;

namespace IOPointManager.Views
{
    public partial class AccessDialog : Window
    {
        private readonly AccessViewModel _viewModel;
        public List<IOPoint>? ImportedPoints { get; private set; }
        public string? ExportTableName { get; private set; }

        public AccessDialog(IAccessService accessService, IEventLogger logger)
        {
            InitializeComponent();

            _viewModel = new AccessViewModel(accessService, logger);
            _viewModel.ImportRequested += OnImportRequested;
            _viewModel.ExportRequested += OnExportRequested;
            _viewModel.CloseRequested += (s, e) => DialogResult = false;

            DataContext = _viewModel;

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    DialogResult = false;
                    Close();
                }
            };
        }

        private void OnImportRequested(object? sender, AccessImportEventArgs e)
        {
            ImportedPoints = e.Points;
            DialogResult = true;
            Close();
        }

        private void OnExportRequested(object? sender, AccessExportEventArgs e)
        {
            ExportTableName = e.TableName;
            DialogResult = true;
            Close();
        }

        public AccessViewModel ViewModel => _viewModel;
    }
}