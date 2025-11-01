namespace Bones.Services;

/// <summary>
/// Service responsible for Git operations.
/// Handles repository initialization and status checking.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Initializes a Git repository in the current directory if one doesn't already exist.
    /// Shows appropriate status messages with color coding.
    /// </summary>
    void InitializeRepository();
}