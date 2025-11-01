using System.Diagnostics;

namespace Bones.Services;

/// <summary>
/// Implementation of Git service for repository initialization.
/// Handles checking for existing repositories and initializing new ones.
/// </summary>
public sealed class GitService : IGitService
{
    private readonly IConsoleService _console;
    private readonly string _currentDirectory;

    /// <summary>
    /// Initializes a new instance of the GitService.
    /// </summary>
    /// <param name="console">Console service for status messages and error reporting</param>
    public GitService(IConsoleService console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _currentDirectory = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Initializes a Git repository in the current directory if one doesn't already exist.
    /// Shows appropriate status messages with color coding based on the operation result.
    /// </summary>
    public void InitializeRepository()
    {
        try
        {
            var gitDirectory = Path.Combine(_currentDirectory, ".git");
            
            // Check if Git repository already exists
            if (Directory.Exists(gitDirectory))
            {
                _console.WriteLineColored("Git has already been initialized.", ConsoleColor.Green);
                return;
            }

            // Initialize new Git repository
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = _currentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _console.WriteError("Failed to start git process");
                return;
            }

            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                _console.WriteLineColored("Git repository initialized successfully.", ConsoleColor.Green);
            }
            else
            {
                var error = process.StandardError.ReadToEnd();
                _console.WriteError($"Git initialization failed: {error}");
            }
        }
        catch (Exception ex)
        {
            _console.WriteError($"Failed to initialize Git repository: {ex.Message}");
        }
    }
}
