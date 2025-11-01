using Bones.Models;

namespace Bones.Services;

/// <summary>
/// Service responsible for copying files to the local project directory.
/// Handles directory creation, file writing, and progress indication.
/// </summary>
public interface IFileOperationsService
{
    /// <summary>
    /// Ensures the required directory structure exists (.github, .github/prompts).
    /// Creates directories if they don't exist.
    /// </summary>
    void EnsureDirectoryStructure();

    /// <summary>
    /// Copies files from the file list to their specified destination paths.
    /// Shows progress indication and handles existing files appropriately.
    /// </summary>
    /// <param name="fileEntries">List of files to copy</param>
    /// <param name="downloadService">Service to download file content</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task CopyFilesAsync(IReadOnlyList<FileEntry> fileEntries, IFileDownloadService downloadService, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies prompt files to the .github/prompts directory.
    /// Ignores the DestinationPath field and uses only the filename from SourcePath.
    /// </summary>
    /// <param name="promptEntries">List of prompt files to copy</param>
    /// <param name="downloadService">Service to download file content</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    Task CopyPromptsAsync(IReadOnlyList<FileEntry> promptEntries, IFileDownloadService downloadService, CancellationToken cancellationToken = default);
}
