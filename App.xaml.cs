using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace P25Scanner
{
    public partial class App : Application
    {
        private IHost _host;

        public App()
        {
            
        }

        public async Task InitializeAsync()
        {
            _host = Program.CreateHostBuilder(new string[] { }).Build();
            await _host.StartAsync();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                using (_host)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                }
            }

            base.OnExit(e);
        }
    }
}

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using P25Scanner.Services;
using P25Scanner.ViewModels;
using P25Scanner.Views;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Serilog;
using MaterialDesignThemes.Wpf;

namespace P25Scanner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        private ILogger _logger;

        public App()
        {
            // Initialize logger early to capture startup issues
            ConfigureLogging();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _logger.Information("Application starting up");

            // Set up theme and resources
            ConfigureMaterialDesignTheme();

            // Configure dependency injection
            ConfigureServices();

            // Show the main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            // Initialize services after window is shown
            Task.Run(() => InitializeServicesAsync())
                .ContinueWith(t => 
                {
                    if (t.IsFaulted)
                    {
                        _logger.Error(t.Exception, "Failed to initialize services");
                        ShowErrorMessageBox("Service Initialization Error", 
                            "Failed to initialize one or more services. The application may not function correctly.");
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Clean up services when shutting down
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _logger.Information("Application shutting down");
                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                // Can't really do much here since we're already exiting
                Console.Error.WriteLine($"Error during shutdown: {ex}");
            }
            finally
            {
                base.OnExit(e);
            }
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<P25Decoder>();
            services.AddSingleton<RtlSdrService>();
            services.AddSingleton<AudioService>();

            // Register ViewModels
            services.AddSingleton<MainWindowViewModel>();

            // Register Views
            services.AddSingleton<MainWindow>();

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
        }

        private async Task InitializeServicesAsync()
        {
            try
            {
                _logger.Information("Initializing application services");

                // Initialize RTL-SDR service
                var rtlSdrService = _serviceProvider.GetRequiredService<RtlSdrService>();
                await rtlSdrService.InitializeAsync();

                // Initialize audio service
                var audioService = _serviceProvider.GetRequiredService<AudioService>();
                await audioService.InitializeAsync();

                // Connect P25Decoder to other services
                var p25Decoder = _serviceProvider.GetRequiredService<P25Decoder>();
                p25Decoder.Initialize(rtlSdrService, audioService);

                _logger.Information("Services initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing services");
                throw;
            }
        }

        private void ConfigureMaterialDesignTheme()
        {
            // Set primary color scheme
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            
            // Allow customization of theme at runtime if needed
            theme.SetBaseTheme(MaterialDesignThemes.Wpf.Theme.Dark);
            theme.SetPrimaryColor(System.Windows.Media.Color.FromRgb(96, 125, 139)); // BlueGrey500
            theme.SetSecondaryColor(System.Windows.Media.Color.FromRgb(3, 169, 244)); // LightBlue500
            
            paletteHelper.SetTheme(theme);
        }

        private void ConfigureLogging()
        {
            // Create logs directory if it doesn't exist
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "P25Scanner");
            
            var logPath = Path.Combine(appDataPath, "logs");
            Directory.CreateDirectory(logPath);

            var logFile = Path.Combine(logPath, "p25scanner-.log");

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
                .Enrich.WithProperty("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString())
                .CreateLogger();

            _logger = Log.ForContext<App>();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.Fatal(e.ExceptionObject as Exception, "Unhandled AppDomain exception");
            ShowErrorMessageBox("Fatal Error", "A fatal error has occurred and the application needs to close.");
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.Error(e.Exception, "Unhandled dispatcher exception");
            ShowErrorMessageBox("Application Error", "An error has occurred in the UI thread.");
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        }

        private void ShowErrorMessageBox(string title, string message)
        {
            // Ensure we're on the UI thread
            if (Dispatcher.CheckAccess())
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Dispatcher.Invoke(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }
    }
}

