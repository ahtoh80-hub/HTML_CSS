using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using IOPointManager.Interfaces;
using IOPointManager.Models;
using IOPointManager.ViewModels;

namespace IOPointManager.Views
{
    public partial class MappingDialog : Window
    {
        private readonly MappingViewModel _viewModel;
        private List<IOPoint> _importedPoints = new();

        public MappingDialog(IExcelMappingService excelService, IEventLogger logger)
        {
            InitializeComponent();
            
            _viewModel = new MappingViewModel(excelService, logger);
            _viewModel.MappingApplied += OnMappingApplied;
            _viewModel.Cancelled += (s, e) => DialogResult = false;
            
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

        public void LoadFile(string filePath, ExcelStructureInfo structure)
        {
            _viewModel.FileName = filePath;
            _viewModel.IsFileSelected = true;
            _viewModel.IsAnalyzed = true;
            
            _viewModel.ExcelColumns.Clear();
            foreach (var column in structure.Columns)
            {
                _viewModel.ExcelColumns.Add(column);
            }
        }

        private void OnMappingApplied(object? sender, List<IOPoint> points)
        {
            _importedPoints = points;
            DialogResult = true;
            Close();
        }

        public List<IOPoint> GetImportedPoints() => _importedPoints;
    }
}