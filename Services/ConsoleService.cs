namespace Bones.Services;

/// <summary>
/// Implementation of console output service with color formatting.
/// Provides consistent visual feedback throughout the application execution.
/// </summary>
public sealed class ConsoleService : IConsoleService
{
    /// <summary>
    /// Writes text to the console in the specified color without a newline.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="color">The console color to use</param>
    public void WriteColored(string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Writes a line to the console in the specified color.
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="color">The console color to use</param>
    public void WriteLineColored(string message, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    /// <summary>
    /// Writes an error message in red and exits the application.
    /// Used for fatal errors that should stop execution immediately.
    /// </summary>
    /// <param name="message">The error message to display</param>
    public void WriteError(string message)
    {
        WriteLineColored($"Error: {message}", ConsoleColor.Red);
        Environment.Exit(1);
    }

    /// <summary>
    /// Writes a blue progress dot to indicate ongoing file operations.
    /// One dot is shown per file or prompt being processed.
    /// </summary>
    public void WriteProgress()
    {
        WriteColored(".", ConsoleColor.Blue);
    }

    /// <summary>
    /// Writes a yellow hash symbol to indicate a file was skipped.
    /// Used when a file already exists at the destination path.
    /// </summary>
    public void WriteSkip()
    {
        WriteColored("#", ConsoleColor.Yellow);
    }
}
