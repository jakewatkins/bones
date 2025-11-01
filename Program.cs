using Bones;
using Bones.Configuration;
using Bones.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Get the assembly directory for configuration file location
var assemblyDirectory = AppContext.BaseDirectory;

// Build configuration from appsettings.json and environment variables
var configuration = new ConfigurationBuilder()
    .SetBasePath(assemblyDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("local.appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Validate that appsettings.json exists and is readable
var appSettingsPath = Path.Combine(assemblyDirectory, "appsettings.json");
if (!File.Exists(appSettingsPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: appsettings.json file not found");
    Console.ResetColor();
    Environment.Exit(1);
}

// Create host builder with dependency injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register configuration
        services.Configure<BonesConfig>(configuration);
        
        // Register HTTP client for file downloads
        services.AddHttpClient<IFileDownloadService, FileDownloadService>();
        
        // Register application services
        services.AddSingleton<IConsoleService, ConsoleService>();
        services.AddSingleton<IFileOperationsService, FileOperationsService>();
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ISpecKitService, SpecKitService>();
        
        // Register main application
        services.AddSingleton<BonesApplication>();
    })
    .Build();

// Get the main application and run it
var app = host.Services.GetRequiredService<BonesApplication>();
await app.RunAsync(args);