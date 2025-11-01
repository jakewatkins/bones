using Bones.Models;

namespace Bones.Services;

/// <summary>
/// Implementation of file operations service for local file system operations.
/// Handles directory creation, file writing, and progress indication during file copying.
/// </summary>
public sealed class FileOperationsService : IFileOperationsService
{
    private readonly IConsoleService _console;
    private readonly string _currentDirectory;

    /// <summary>
    /// Initializes a new instance of the FileOperationsService.
    /// </summary>
    /// <param name="console">Console service for progress indication and error reporting</param>
    public FileOperationsService(IConsoleService console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _currentDirectory = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Ensures the required directory structure exists for the project.
    /// Creates .github and .github/prompts directories if they don't exist.
    /// </summary>
    public void EnsureDirectoryStructure()
    {
        try
        {
            var githubPath = Path.Combine(_currentDirectory, ".github");
            var promptsPath = Path.Combine(githubPath, "prompts");

            if (!Directory.Exists(githubPath))
            {
                Directory.CreateDirectory(githubPath);
            }

            if (!Directory.Exists(promptsPath))
            {
                Directory.CreateDirectory(promptsPath);
            }
        }
        catch (Exception ex)
        {
            _console.WriteError($"Failed to create directory structure: {ex.Message}");
        }
    }

    /// <summary>
    /// Copies files from the file list to their specified destination paths.
    /// Shows progress dots for each file and skips existing files with yellow hash indicators.
    /// </summary>
    /// <param name="fileEntries">List of files to copy</param>
    /// <param name="downloadService">Service to download file content</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    public async Task CopyFilesAsync(IReadOnlyList<FileEntry> fileEntries, IFileDownloadService downloadService, CancellationToken cancellationToken = default)
    {
        foreach (var fileEntry in fileEntries)
        {
            try
            {
                var destinationPath = Path.Combine(_currentDirectory, fileEntry.DestinationPath);
                
                // Skip if file already exists
                if (File.Exists(destinationPath))
                {
                    _console.WriteSkip();
                    continue;
                }

                // Ensure the destination directory exists
                var destinationDir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }

                // Download and write the file
                var content = await downloadService.DownloadFileAsync(fileEntry.SourcePath, cancellationToken);
                await File.WriteAllTextAsync(destinationPath, content, cancellationToken);
                
                _console.WriteProgress();
            }
            catch (Exception ex)
            {
                _console.WriteError($"Failed to copy file '{fileEntry.SourcePath}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Copies prompt files to the .github/prompts directory.
    /// Ignores the DestinationPath field and extracts the filename from the SourcePath.
    /// This ensures all prompts are organized in a consistent directory structure.
    /// </summary>
    /// <param name="promptEntries">List of prompt files to copy</param>
    /// <param name="downloadService">Service to download file content</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    public async Task CopyPromptsAsync(IReadOnlyList<FileEntry> promptEntries, IFileDownloadService downloadService, CancellationToken cancellationToken = default)
    {
        var promptsDirectory = Path.Combine(_currentDirectory, ".github", "prompts");

        foreach (var promptEntry in promptEntries)
        {
            try
            {
                // Extract filename from source path (ignore DestinationPath as per requirements)
                var fileName = Path.GetFileName(promptEntry.SourcePath);
                var destinationPath = Path.Combine(promptsDirectory, fileName);
                
                // Skip if file already exists
                if (File.Exists(destinationPath))
                {
                    _console.WriteSkip();
                    continue;
                }

                // Download and write the prompt file
                var content = await downloadService.DownloadFileAsync(promptEntry.SourcePath, cancellationToken);
                await File.WriteAllTextAsync(destinationPath, content, cancellationToken);
                
                _console.WriteProgress();
            }
            catch (Exception ex)
            {
                _console.WriteError($"Failed to copy prompt '{promptEntry.SourcePath}': {ex.Message}");
            }
        }
    }
}
