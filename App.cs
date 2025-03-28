using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using P25Scanner.Models;
using P25Scanner.Services;
using P25Scanner.ViewModels;
using P25Scanner.Views;
using ReactiveUI;

namespace P25Scanner
{
    /// <summary>
    /// The main application class that coordinates all services and handles the application lifecycle.
    /// </summary>
    public class App : Application
    {
        private static readonly Lazy<App> _instance = new Lazy<App>(() => new App());
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<App> _logger;
        private bool _isInitialized;

        /// <summary>
        /// Gets the singleton instance of the App.
        /// </summary>
        public static App Instance => _instance.Value;

        /// <summary>
        /// Constructor for the App class.
        /// </summary>
        private App()
        {
            // Configure services
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Set up logging
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
            _logger.LogInformation("P25Scanner application is starting up...");
        }

        /// <summary>
        /// Configures all application services.
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs/p25scanner-{Date}.log"));
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Register application services
            services.AddSingleton<P25Decoder>();
            services.AddSingleton<RtlSdrService>();
            services.AddSingleton<AudioService>();
            services.AddSingleton<ScannerModel>();

            // Register view models
            services.AddTransient<MainWindowViewModel>();

            // Register views
            services.AddTransient<MainWindow>();
        }

        /// <summary>
        /// Initializes the application and all services.
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                _logger.LogInformation("Initializing application services...");

                // Get required services
                var scannerModel = _serviceProvider.GetRequiredService<ScannerModel>();
                var rtlSdrService = _serviceProvider.GetRequiredService<RtlSdrService>();
                var audioService = _serviceProvider.GetRequiredService<AudioService>();
                var p25Decoder = _serviceProvider.GetRequiredService<P25Decoder>();

                // Initialize services in the correct order
                await InitializeServicesAsync(rtlSdrService, p25Decoder, audioService, scannerModel);

                // Set up global exception handling
                SetupGlobalExceptionHandling();

                _isInitialized = true;
                _logger.LogInformation("Application services initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize application services.");
                throw;
            }
        }

        /// <summary>
        /// Initializes all services in the correct order.
        /// </summary>
        private async Task InitializeServicesAsync(
            RtlSdrService rtlSdrService,
            P25Decoder p25Decoder,
            AudioService audioService,
            ScannerModel scannerModel)
        {
            _logger.LogInformation("Initializing P25 decoder...");
            await Task.Run(() => p25Decoder.Initialize());

            _logger.LogInformation("Initializing audio service...");
            await Task.Run(() => audioService.Initialize());

            _logger.LogInformation("Connecting P25 decoder to audio service...");
            p25Decoder.DecodedAudioAvailable += audioService.HandleDecodedAudio;

            _logger.LogInformation("Initializing RTL-SDR service...");
            await Task.Run(() => rtlSdrService.Initialize());

            _logger.LogInformation("Connecting RTL-SDR service to P25 decoder...");
            rtlSdrService.IQDataReceived += p25Decoder.ProcessIQData;

            _logger.LogInformation("Initializing scanner model...");
            await Task.Run(() => scannerModel.Initialize(rtlSdrService, p25Decoder, audioService));
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        public async Task StartAsync()
        {
            if (!_isInitialized)
                await InitializeAsync();

            _logger.LogInformation("Starting application...");

            // Create and show the main window
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.Show();

            _logger.LogInformation("Application started successfully.");
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public async Task ShutdownAsync()
        {
            _logger.LogInformation("Shutting down application...");

            try
            {
                // Get required services
                var scannerModel = _serviceProvider.GetRequiredService<ScannerModel>();
                var rtlSdrService = _serviceProvider.GetRequiredService<RtlSdrService>();
                var audioService = _serviceProvider.GetRequiredService<AudioService>();
                var p25Decoder = _serviceProvider.GetRequiredService<P25Decoder>();

                // Disconnect events
                rtlSdrService.IQDataReceived -= p25Decoder.ProcessIQData;
                p25Decoder.DecodedAudioAvailable -= audioService.HandleDecodedAudio;

                // Shutdown services in reverse order
                await Task.Run(() => scannerModel.Shutdown());
                await Task.Run(() => rtlSdrService.Shutdown());
                await Task.Run(() => audioService.Shutdown());
                await Task.Run(() => p25Decoder.Shutdown());

                _logger.LogInformation("Application shut down successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during application shutdown.");
            }
            finally
            {
                // Force application to exit
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Sets up global exception handling.
        /// </summary>
        private void SetupGlobalExceptionHandling()
        {
            // Handle exceptions in the UI thread
            Current.DispatcherUnhandledException += (sender, e) =>
            {
                _logger.LogError(e.Exception, "Unhandled exception in UI thread.");
                MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            // Handle exceptions in background threads
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                _logger.LogError(exception, "Unhandled exception in background thread.");
                MessageBox.Show($"A fatal error occurred: {exception?.Message ?? "Unknown error"}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Handle exceptions in tasks
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                _logger.LogError(e.Exception, "Unhandled exception in task.");
                e.SetObserved();
            };
        }

        /// <summary>
        /// Gets a service from the service provider.
        /// </summary>
        public T GetService<T>() where T : class
        {
            return _serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Gets a required service from the service provider.
        /// </summary>
        public T GetRequiredService<T>() where T : class
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Displays a notification to the user.
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Information)
        {
            _logger.LogInformation($"Notification: {message}");
            
            // Implement custom notification system here if needed
            switch (type)
            {
                case NotificationType.Information:
                    MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case NotificationType.Warning:
                    MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
                case NotificationType.Error:
                    MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }

    /// <summary>
    /// Notification types for user notifications.
    /// </summary>
    public enum NotificationType
    {
        Information,
        Warning,
        Error
    }
}

