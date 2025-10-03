namespace Bones.Services;

public interface IConsoleService
{
    void WriteError(string message);
    void WriteSuccess(string message);
    void WriteInfo(string message);
}

public class ConsoleService : IConsoleService
{
    public void WriteError(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ForegroundColor = originalColor;
    }

    public void WriteSuccess(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    public void WriteInfo(string message)
    {
        Console.WriteLine(message);
    }
}