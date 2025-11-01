namespace Bones.Services;

/// <summary>
/// Service responsible for console output formatting and color display.
/// Provides consistent visual feedback throughout the application.
/// </summary>
public interface IConsoleService
{
    /// <summary>
    /// Writes text to the console in the specified color.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="color">The console color to use</param>
    void WriteColored(string message, ConsoleColor color);

    /// <summary>
    /// Writes a line to the console in the specified color.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="color">The console color to use</param>
    void WriteLineColored(string message, ConsoleColor color);

    /// <summary>
    /// Writes an error message in red and stops execution.
    /// </summary>
    /// <param name="message">The error message to display</param>
    void WriteError(string message);

    /// <summary>
    /// Writes a progress indicator (dot) in blue.
    /// </summary>
    void WriteProgress();

    /// <summary>
    /// Writes a skip indicator (#) in yellow.
    /// </summary>
    void WriteSkip();
}