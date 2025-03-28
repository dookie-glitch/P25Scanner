using System;
using System.Windows;
using P25Scanner.ViewModels;
using MahApps.Metro.Controls;
namespace P25Scanner
{
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize window state
            Title = $"P25Scanner - v{GetType().Assembly.GetName().Version}";
        }

        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_viewModel.IsScanning)
                {
                    e.Cancel = true;
                    await _viewModel.ToggleScanCommand.ExecuteAsync();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during shutdown: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

