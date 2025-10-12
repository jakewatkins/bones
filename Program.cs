using Bones.Models;
using Bones.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.Json;

namespace Bones;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Step 1: Pre-validate configuration file existence
        string configDirectory;
        
        // Check for environment variable override (used by wrapper scripts)
        var envConfigDir = Environment.GetEnvironmentVariable("BONES_CONFIG_DIR");
        if (!string.IsNullOrEmpty(envConfigDir))
        {
            configDirectory = envConfigDir;
        }
        else
        {
            // Default: use the directory where the executable is located
            var executablePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
            configDirectory = Path.GetDirectoryName(executablePath)!;
        }
        
        var configFilePath = Path.Combine(configDirectory, "appsettings.json");

        if (!File.Exists(configFilePath))
        {
            var consoleService = new ConsoleService();
            consoleService.WriteError($"appsettings.json not found at: {configFilePath}");
            return 1;
        }

        IHost host;
        try
        {
            host = CreateHostBuilder(args).Build();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Invalid JSON"))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            return 1;
        }
        catch (FileNotFoundException ex) when (ex.FileName?.Contains("appsettings.json") == true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: appsettings.json not found at: {ex.FileName}");
            Console.ResetColor();
            return 1;
        }
        
        try
        {
            var consoleService = host.Services.GetRequiredService<IConsoleService>();
            var commandLineParser = host.Services.GetRequiredService<ICommandLineParser>();
            var projectService = host.Services.GetRequiredService<IProjectService>();

            // Parse command line arguments
            var parsedArgs = commandLineParser.Parse(args);

            // Create the project
            await projectService.CreateProjectAsync(parsedArgs);

            // Success message
            var projectTypeText = parsedArgs.ProjectType == ProjectType.Personal ? "personal" : "work";
            var specKitText = parsedArgs.IncludeSpecKit ? " with Spec-Kit integration" : "";
            consoleService.WriteSuccess($"Successfully created {projectTypeText} project '{parsedArgs.ProjectName}'{specKitText}");

            return 0;
        }
        catch (ArgumentException ex)
        {
            var consoleService = host.Services.GetRequiredService<IConsoleService>();
            consoleService.WriteError(ex.Message);
            PrintUsage(host.Services.GetRequiredService<IConsoleService>());
            return 1;
        }
        catch (FileNotFoundException ex) when (ex.FileName?.Contains("appsettings.json") == true)
        {
            var consoleService = host.Services.GetRequiredService<IConsoleService>();
            consoleService.WriteError($"appsettings.json not found at: {ex.FileName}");
            return 1;
        }
        catch (Exception ex)
        {
            var consoleService = host.Services.GetRequiredService<IConsoleService>();
            consoleService.WriteError(ex.Message);
            return 1;
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        // Get the directory where the configuration should be located
        string configDirectory;
        
        // Check for environment variable override (used by wrapper scripts)
        var envConfigDir = Environment.GetEnvironmentVariable("BONES_CONFIG_DIR");
        if (!string.IsNullOrEmpty(envConfigDir))
        {
            configDirectory = envConfigDir;
        }
        else
        {
            // Default: use the directory where the executable is located
            var executablePath = Environment.ProcessPath ?? Assembly.GetExecutingAssembly().Location;
            configDirectory = Path.GetDirectoryName(executablePath)!;
        }
        
        var configFilePath = Path.Combine(configDirectory, "appsettings.json");
        
        // Validate JSON format before attempting to use it
        try
        {
            var jsonContent = File.ReadAllText(configFilePath);
            using var document = JsonDocument.Parse(jsonContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON in {configFilePath}: {ex.Message}");
        }

        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Clear default configuration sources to remove fallbacks
                config.Sources.Clear();
                
                // Add only our specific appsettings.json from executable directory
                config.AddJsonFile(configFilePath, optional: false, reloadOnChange: false);
            })
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.Configure<BonesConfig>(context.Configuration);

                // HTTP Client
                services.AddHttpClient<IFileDownloadService, FileDownloadService>();

                // Services
                services.AddSingleton<ICommandLineParser, CommandLineParser>();
                services.AddSingleton<IConsoleService, ConsoleService>();
                services.AddScoped<IProjectService, ProjectService>();
            });
    }

    static void PrintUsage(IConsoleService consoleService)
    {
        consoleService.WriteInfo("");
        consoleService.WriteInfo("Usage: bones [work/personal] [project-name] [optional: sk]");
        consoleService.WriteInfo("");
        consoleService.WriteInfo("Examples:");
        consoleService.WriteInfo("  bones work hermes");
        consoleService.WriteInfo("  bones personal lifttracker sk");
        consoleService.WriteInfo("  bones Work accounting SK");
        consoleService.WriteInfo("  bones PERSONAL bones sK");
        consoleService.WriteInfo("");
        consoleService.WriteInfo("Parameters:");
        consoleService.WriteInfo("  work/personal  - Project type (case insensitive)");
        consoleService.WriteInfo("  project-name   - Name of the project to create");
        consoleService.WriteInfo("  sk             - Optional flag to include Spec-Kit integration (case insensitive)");
    }
}