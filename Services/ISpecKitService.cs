namespace Bones.Services;

/// <summary>
/// Service responsible for Spec-Kit integration.
/// Handles running the specify command when the SK flag is provided.
/// </summary>
public interface ISpecKitService
{
    /// <summary>
    /// Sets up Spec-Kit using the specify command with copilot and shell script configuration.
    /// Handles cases where Spec-Kit is not installed gracefully.
    /// </summary>
    /// <param name="projectName">The name of the current project directory</param>
    void SetupSpecKit(string projectName);
}
