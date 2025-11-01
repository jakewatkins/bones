using System.Text.Json;
using Bones.Configuration;
using Bones.Models;
using Microsoft.Extensions.Options;

namespace Bones.Services;

/// <summary>
/// Implementation of file download service using HttpClient.
/// Downloads files directly from GitHub raw content URLs without cloning the repository.
/// </summary>
public sealed class FileDownloadService : IFileDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly BonesConfig _config;
    private readonly IConsoleService _console;

    /// <summary>
    /// Initializes a new instance of the FileDownloadService.
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests to GitHub</param>
    /// <param name="config">Configuration containing the repository URL</param>
    /// <param name="console">Console service for error reporting</param>
    public FileDownloadService(HttpClient httpClient, IOptions<BonesConfig> config, IConsoleService console)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    /// <summary>
    /// Downloads and parses both file-list.json and prompt-list.json from the repository.
    /// These files contain the mapping of source files to destination paths.
    /// If file-list.json doesn't exist, returns an empty list for files.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A tuple containing parsed file entries for both lists</returns>
    public async Task<(IReadOnlyList<FileEntry> FileList, IReadOnlyList<FileEntry> PromptList)> GetFileListsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Download file-list.json if it exists, otherwise use empty list
            var fileListTask = DownloadAndParseJsonAsync<FileEntry[]>("file-list.json", cancellationToken, allowMissing: true);
            var promptListTask = DownloadAndParseJsonAsync<FileEntry[]>("prompt-list.json", cancellationToken, allowMissing: false);

            await Task.WhenAll(fileListTask, promptListTask);

            var fileList = await fileListTask ?? Array.Empty<FileEntry>();
            var promptList = await promptListTask ?? Array.Empty<FileEntry>();

            return (fileList, promptList);
        }
        catch (Exception ex)
        {
            _console.WriteError($"Failed to download file lists: {ex.Message}");
            return (Array.Empty<FileEntry>(), Array.Empty<FileEntry>());
        }
    }

    /// <summary>
    /// Downloads a single file from the repository using the raw GitHub content URL.
    /// Converts the repository URL to the appropriate raw content format.
    /// </summary>
    /// <param name="sourcePath">The source path of the file in the repository</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The file content as a string</returns>
    public async Task<string> DownloadFileAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var rawUrl = BuildRawUrl(sourcePath);
            var response = await _httpClient.GetAsync(rawUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _console.WriteError($"Failed to download file '{sourcePath}'. Status: {response.StatusCode}");
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _console.WriteError($"Failed to download file '{sourcePath}': {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Downloads and parses a JSON file from the repository.
    /// Uses System.Text.Json for parsing with case-insensitive property matching.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content to</typeparam>
    /// <param name="fileName">The name of the JSON file to download</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="allowMissing">If true, returns null when file is not found instead of exiting</param>
    /// <returns>The deserialized object, or null if file is missing and allowMissing is true</returns>
    private async Task<T?> DownloadAndParseJsonAsync<T>(string fileName, CancellationToken cancellationToken, bool allowMissing = false) where T : class
    {
        try
        {
            var rawUrl = BuildRawUrl(fileName);
            var response = await _httpClient.GetAsync(rawUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (allowMissing && response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                _console.WriteError($"Failed to download file '{fileName}'. Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
            if (string.IsNullOrWhiteSpace(content))
            {
                if (allowMissing)
                {
                    return null;
                }
                _console.WriteError($"Downloaded JSON file '{fileName}' is empty or invalid");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<T>(content, options);
            return result ?? (allowMissing ? null : throw new InvalidOperationException($"Failed to deserialize {fileName}"));
        }
        catch (JsonException ex)
        {
            if (allowMissing)
            {
                return null;
            }
            _console.WriteError($"Invalid JSON in file '{fileName}': {ex.Message}");
            return default!;
        }
    }

    /// <summary>
    /// Builds the raw GitHub content URL from the repository URL and source path.
    /// Converts from github.com/user/repo format to raw.githubusercontent.com format.
    /// </summary>
    /// <param name="sourcePath">The source path of the file in the repository</param>
    /// <returns>The complete raw URL for downloading the file</returns>
    private string BuildRawUrl(string sourcePath)
    {
        // Convert from https://github.com/jakewatkins/copilot-resources
        // to https://raw.githubusercontent.com/jakewatkins/copilot-resources/main/{sourcePath}
        var baseUrl = _config.ResourceRepo.Replace("github.com", "raw.githubusercontent.com");
        return $"{baseUrl}/main/{sourcePath}";
    }
}
