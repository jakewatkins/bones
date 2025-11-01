using System.Diagnostics;

namespace Bones.Services;

/// <summary>
/// Implementation of Spec-Kit service for setting up project specifications.
/// Handles running the specify command with appropriate error handling for missing installations.
/// </summary>
public sealed class SpecKitService : ISpecKitService
{
    private readonly IConsoleService _console;
    private readonly string _currentDirectory;

    /// <summary>
    /// Initializes a new instance of the SpecKitService.
    /// </summary>
    /// <param name="console">Console service for status messages and error reporting</param>
    public SpecKitService(IConsoleService console)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _currentDirectory = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Sets up Spec-Kit using the specify command with copilot and shell script configuration.
    /// Gracefully handles cases where Spec-Kit is not installed and continues execution.
    /// </summary>
    /// <param name="projectName">The name of the current project directory</param>
    public void SetupSpecKit(string projectName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "specify",
                Arguments = $"init --ai copilot --script sh {projectName}",
                WorkingDirectory = _currentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                _console.WriteLineColored("Spec-kit has not been installed.", ConsoleColor.Red);
                return;
            }

            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                var output = process.StandardOutput.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(output))
                {
                    _console.WriteLineColored($"Spec-Kit setup completed: {output.Trim()}", ConsoleColor.Green);
                }
                else
                {
                    _console.WriteLineColored("Spec-Kit setup completed successfully.", ConsoleColor.Green);
                }
            }
            else
            {
                var error = process.StandardError.ReadToEnd();
                _console.WriteLineColored($"Spec-Kit setup failed: {error}", ConsoleColor.Red);
            }
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            // Handle "file not found" error when specify command is not installed
            _console.WriteLineColored("Spec-kit has not been installed.", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            _console.WriteLineColored($"Failed to setup Spec-Kit: {ex.Message}", ConsoleColor.Red);
        }
    }
}
