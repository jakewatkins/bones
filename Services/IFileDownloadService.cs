using Bones.Models;

namespace Bones.Services;

/// <summary>
/// Service responsible for downloading files from the GitHub repository.
/// Handles HTTP requests to fetch raw file content without cloning the repository.
/// </summary>
public interface IFileDownloadService
{
    /// <summary>
    /// Downloads the specified list configuration files (file-list.json and prompt-list.json).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A tuple containing the file list and prompt list entries</returns>
    Task<(IReadOnlyList<FileEntry> FileList, IReadOnlyList<FileEntry> PromptList)> GetFileListsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a single file from the repository and returns its content.
    /// </summary>
    /// <param name="sourcePath">The source path of the file in the repository</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The file content as a string</returns>
    Task<string> DownloadFileAsync(string sourcePath, CancellationToken cancellationToken = default);
}