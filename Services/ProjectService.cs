using Bones.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Bones.Services;

public interface IProjectService
{
    Task CreateProjectAsync(CommandLineArgs args);
}

public class ProjectService : IProjectService
{
    private readonly BonesConfig _config;
    private readonly IFileDownloadService _downloadService;

    public ProjectService(IOptions<BonesConfig> config, IFileDownloadService downloadService)
    {
        _config = config.Value;
        _downloadService = downloadService;
    }

    public async Task CreateProjectAsync(CommandLineArgs args)
    {
        // Determine project path
        var basePath = args.ProjectType == ProjectType.Personal 
            ? _config.Directories.PersonalBase 
            : _config.Directories.WorkBase;
        
        // Expand home directory (~)
        if (basePath.StartsWith("~/"))
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                                   basePath.Substring(2));
        }

        var projectPath = Path.Combine(basePath, args.ProjectName);

        // Check if directory already exists
        if (Directory.Exists(projectPath))
        {
            throw new InvalidOperationException($"Directory '{projectPath}' already exists.");
        }

        // Create project directory
        Directory.CreateDirectory(projectPath);

        // Create .github/prompts directory
        var githubPromptsPath = Path.Combine(projectPath, ".github", "prompts");
        Directory.CreateDirectory(githubPromptsPath);

        // Download core files
        await DownloadCoreFilesAsync(projectPath, args.IncludeSpecKit);

        // Download prompt files
        await DownloadPromptFilesAsync(projectPath);

        // Initialize git repository
        await InitializeGitRepositoryAsync(projectPath);

        // Handle Spec-Kit integration if requested
        if (args.IncludeSpecKit)
        {
            await SetupSpecKitAsync(projectPath);
        }
    }

    private async Task DownloadCoreFilesAsync(string projectPath, bool includeSpecKit)
    {
        var baseUrl = _config.GitHubFiles.BaseUrl;

        foreach (var file in _config.GitHubFiles.Files)
        {
            // Skip files that are only for SpecKit if SpecKit is not included
            if (file.OnlyWhenSpecKit && !includeSpecKit)
                continue;

            var sourceUrl = $"{baseUrl}/{file.SourcePath}";
            var destinationPath = Path.Combine(projectPath, file.DestinationPath);

            try
            {
                await _downloadService.DownloadFileToPathAsync(sourceUrl, destinationPath);
            }
            catch (Exception ex)
            {
                if (file.IsRequired)
                {
                    throw new InvalidOperationException($"Failed to download required file '{file.SourcePath}': {ex.Message}", ex);
                }
                // For non-required files, we can continue
                Console.WriteLine($"Warning: Could not download optional file '{file.SourcePath}': {ex.Message}");
            }
        }
    }

    private async Task DownloadPromptFilesAsync(string projectPath)
    {
        var baseUrl = _config.GitHubFiles.BaseUrl;

        foreach (var promptFile in _config.GitHubFiles.PromptFiles)
        {
            var sourceUrl = $"{baseUrl}/{promptFile.SourcePath}";
            var destinationPath = Path.Combine(projectPath, promptFile.DestinationPath);

            try
            {
                await _downloadService.DownloadFileToPathAsync(sourceUrl, destinationPath);
            }
            catch (Exception ex)
            {
                // Prompt files are not strictly required for basic functionality
                Console.WriteLine($"Warning: Could not download prompt file '{promptFile.SourcePath}': {ex.Message}");
            }
        }
    }

    private async Task InitializeGitRepositoryAsync(string projectPath)
    {
        var gitProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        gitProcess.Start();
        await gitProcess.WaitForExitAsync();

        if (gitProcess.ExitCode != 0)
        {
            var error = await gitProcess.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Failed to initialize git repository: {error}");
        }
    }

    private async Task SetupSpecKitAsync(string projectPath)
    {
        // Run specify command
        var specifyProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "specify",
                Arguments = "--ai copilot --script sh --here",
                WorkingDirectory = projectPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        specifyProcess.Start();
        await specifyProcess.WaitForExitAsync();

        if (specifyProcess.ExitCode != 0)
        {
            var error = await specifyProcess.StandardError.ReadToEndAsync();
            throw new InvalidOperationException($"Failed to run specify command: {error}");
        }

        // Download constitution.md to .specify/memory/
        await DownloadConstitutionFileAsync(projectPath);
    }

    private async Task DownloadConstitutionFileAsync(string projectPath)
    {
        var baseUrl = _config.GitHubFiles.BaseUrl;
        var constitutionFile = _config.GitHubFiles.Files.FirstOrDefault(f => f.OnlyWhenSpecKit && f.SourcePath == "constitution.md");
        
        if (constitutionFile != null)
        {
            var sourceUrl = $"{baseUrl}/{constitutionFile.SourcePath}";
            var destinationPath = Path.Combine(projectPath, constitutionFile.DestinationPath);

            try
            {
                await _downloadService.DownloadFileToPathAsync(sourceUrl, destinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not download constitution.md: {ex.Message}");
            }
        }
    }
}