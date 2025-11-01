namespace Bones.Configuration;

/// <summary>
/// Configuration settings for the Bones application.
/// Loaded from appsettings.json and environment variables.
/// </summary>
public sealed class BonesConfig
{
    /// <summary>
    /// The base URL of the GitHub repository containing the copilot resources.
    /// Default: "https://github.com/jakewatkins/copilot-resources"
    /// </summary>
    public required string ResourceRepo { get; init; }
}
