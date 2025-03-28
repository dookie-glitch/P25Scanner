using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using P25Scanner.Services;
using P25Scanner.Services.Interfaces;
using P25Scanner.ViewModels;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace P25Scanner
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MessageBox.Show(
                    $"An unhandled exception occurred: {e.ExceptionObject}",
                    "Application Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            try
            {
                InitializeApplication(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to start the application: {ex.Message}\n\n{ex.StackTrace}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public static async Task InitializeApplication(string[] args)
        {
            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File(
                        configuration["Logging:File:Path"] ?? "logs/p25scanner-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: int.Parse(configuration["Logging:File:RetainedFileCountLimit"] ?? "7"))
                    .CreateLogger();

                // Log startup
                Log.Information("P25Scanner starting up...");

                // Create host builder
                var host = CreateHostBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        // Add configuration
                        services.AddSingleton<IConfiguration>(configuration);

                        // Add logging
                        services.AddLogging(builder =>
                        {
                            builder.ClearProviders();
                            builder.AddSerilog(dispose: true);
                        });

                        // Register services
                        services.AddSingleton<IP25Decoder, P25Decoder>();
                        
                        // Add UI services if we're on Windows
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            services.AddSingleton<MainWindow>();
                            services.AddSingleton<MainWindowViewModel>();
                        }
                    })
                    .UseSerilog()
                    .Build();

                try
                {
                    // Initialize systems
                    using var cts = new CancellationTokenSource();
                    Console.CancelKeyPress += (s, e) =>
                    {
                        e.Cancel = true;
                        cts.Cancel();
                    };

                    // Start based on platform
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var app = new Application();
                        var mainWindow = host.Services.GetRequiredService<MainWindow>();
                        app.Run(mainWindow);
                    }
                    else
                    {
                        // Initialize decoder for console mode
                        var decoder = host.Services.GetRequiredService<IP25Decoder>();
                        var logger = host.Services.GetRequiredService<ILogger<Program>>();

                        // Initialize decoder
                        uint sampleRate = uint.Parse(configuration["SDR:SampleRate"] ?? "2048000");
                        if (!await decoder.InitializeAsync(sampleRate))
                        {
                            throw new Exception("Failed to initialize P25 decoder");
                        }

                        // Start decoder
                        if (!await decoder.StartAsync(cts.Token))
                        {
                            throw new Exception("Failed to start P25 decoder");
                        }

                        // Console mode for non-Windows platforms
                        await host.RunAsync(cts.Token);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error running P25Scanner");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.Information("P25Scanner shutting down...");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args);
    }
}
