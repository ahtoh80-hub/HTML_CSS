using System.Windows;
using System.Windows.Controls;
using IOPointManager.Interfaces;
using IOPointManager.ViewModels;

namespace IOPointManager.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnLogAdded(object? sender, LogEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var logListBox = FindName("LogListBox") as ListBox;
                if (logListBox != null && logListBox.Items.Count > 0)
                {
                    logListBox.ScrollIntoView(logListBox.Items[0]);
                }
            });
        }
    }
}