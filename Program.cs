using Bones.Models;
using Bones.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bones;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
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
        catch (Exception ex)
        {
            var consoleService = host.Services.GetRequiredService<IConsoleService>();
            consoleService.WriteError(ex.Message);
            return 1;
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
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