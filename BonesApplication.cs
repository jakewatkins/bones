using Bones.Services;

namespace Bones;

/// <summary>
/// Main application class that orchestrates the Bones tool execution.
/// Handles command-line argument parsing and coordinates all services to complete the workflow.
/// </summary>
public sealed class BonesApplication
{
    private readonly IFileDownloadService _fileDownloadService;
    private readonly IFileOperationsService _fileOperationsService;
    private readonly IGitService _gitService;
    private readonly ISpecKitService _specKitService;
    private readonly IConsoleService _console;

    /// <summary>
    /// Initializes a new instance of the BonesApplication.
    /// </summary>
    /// <param name="fileDownloadService">Service for downloading files from GitHub</param>
    /// <param name="fileOperationsService">Service for local file operations</param>
    /// <param name="gitService">Service for Git operations</param>
    /// <param name="specKitService">Service for Spec-Kit integration</param>
    /// <param name="console">Console service for user feedback</param>
    public BonesApplication(
        IFileDownloadService fileDownloadService,
        IFileOperationsService fileOperationsService,
        IGitService gitService,
        ISpecKitService specKitService,
        IConsoleService console)
    {
        _fileDownloadService = fileDownloadService ?? throw new ArgumentNullException(nameof(fileDownloadService));
        _fileOperationsService = fileOperationsService ?? throw new ArgumentNullException(nameof(fileOperationsService));
        _gitService = gitService ?? throw new ArgumentNullException(nameof(gitService));
        _specKitService = specKitService ?? throw new ArgumentNullException(nameof(specKitService));
        _console = console ?? throw new ArgumentNullException(nameof(console));
    }

    /// <summary>
    /// Runs the main application logic based on command-line arguments.
    /// Processes the SK flag and executes the complete workflow.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    public async Task RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse command-line arguments
            var showUsage = ShouldShowUsage(args);
            var includeSpecKit = ShouldIncludeSpecKit(args);
            var initGit = ShouldInitGit(args);

            if (showUsage)
            {
                _console.WriteLineColored("Usage: bones [options]", ConsoleColor.Green);
                _console.WriteLineColored("Options:", ConsoleColor.Green);
                _console.WriteLineColored("  -h, --help       Show this help message and exit", ConsoleColor.Green);
                _console.WriteLineColored("  -sk, -SK        Include Spec-Kit setup", ConsoleColor.Green);
                _console.WriteLineColored("  -nogit, -NOGIT  Do not initialize a Git repository", ConsoleColor.Green);
                return;
            }

            // Ensure directory structure exists
            _fileOperationsService.EnsureDirectoryStructure();

            // Download file lists from repository
            var (fileList, promptList) = await _fileDownloadService.GetFileListsAsync(cancellationToken);

            // Copy files to their destination paths
            await _fileOperationsService.CopyFilesAsync(fileList, _fileDownloadService, cancellationToken);

            // Copy prompts to .github/prompts directory
            await _fileOperationsService.CopyPromptsAsync(promptList, _fileDownloadService, cancellationToken);

            // Initialize Git repository if needed
            if (initGit)
            {
                _gitService.InitializeRepository();
            }

            // Set up Spec-Kit if requested
            if (includeSpecKit)
            {
                var projectName = GetProjectName();
                _specKitService.SetupSpecKit(projectName);
            }

            // Show completion message
            _console.WriteLineColored("done", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            _console.WriteError($"Application failed: {ex.Message}");
        }
    }

    private static bool ShouldShowUsage(string[] args)
    {
        return args.Any(arg => string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase));
    }
    /// <summary>
    /// Determines if the SK flag was provided in the command-line arguments.
    /// The flag is case-insensitive and can be -sk or -SK.
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>True if the SK flag was provided, false otherwise</returns>
    private static bool ShouldIncludeSpecKit(string[] args)
    {
        return args.Any(arg => string.Equals(arg, "-sk", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(arg, "-SK", StringComparison.OrdinalIgnoreCase));
    }

    private static bool ShouldInitGit(string[] args)
    {
        return !args.Any(arg => string.Equals(arg, "-nogit", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(arg, "-NOGIT", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the project name from the current working directory.
    /// Extracts the directory name to use as the project identifier for Spec-Kit.
    /// </summary>
    /// <returns>The name of the current directory</returns>
    private static string GetProjectName()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        return Path.GetFileName(currentDirectory) ?? "project";
    }
}
