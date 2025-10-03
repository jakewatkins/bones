using Bones.Models;

namespace Bones.Services;

public interface IFileDownloadService
{
    Task<string> DownloadFileAsync(string url);
    Task DownloadFileToPathAsync(string url, string destinationPath);
}

public class FileDownloadService : IFileDownloadService
{
    private readonly HttpClient _httpClient;

    public FileDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> DownloadFileAsync(string url)
    {
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to download file from {url}: {ex.Message}", ex);
        }
    }

    public async Task DownloadFileToPathAsync(string url, string destinationPath)
    {
        var content = await DownloadFileAsync(url);
        
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(destinationPath, content);
    }
}