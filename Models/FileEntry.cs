namespace Bones.Models;

/// <summary>
/// Represents a file entry in the file-list.json and prompt-list.json files.
/// Used to map source files from the repository to destination paths in the project.
/// </summary>
public sealed class FileEntry
{
    /// <summary>
    /// The source path of the file in the copilot-resources repository.
    /// </summary>
    public required string SourcePath { get; init; }

    /// <summary>
    /// The destination path where the file should be copied in the target project.
    /// For prompts, this field is ignored and files are placed in .github/prompts/.
    /// </summary>
    public required string DestinationPath { get; init; }
}