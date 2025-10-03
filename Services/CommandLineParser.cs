using Bones.Models;

namespace Bones.Services;

public interface ICommandLineParser
{
    CommandLineArgs Parse(string[] args);
}

public class CommandLineParser : ICommandLineParser
{
    public CommandLineArgs Parse(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("At least 2 parameters are required: [work/personal] [project-name] [optional: sk]");
        }

        var result = new CommandLineArgs();

        // Parse project type (case insensitive)
        var projectTypeArg = args[0].ToLowerInvariant();
        result.ProjectType = projectTypeArg switch
        {
            "work" => ProjectType.Work,
            "personal" => ProjectType.Personal,
            _ => throw new ArgumentException($"Invalid project type '{args[0]}'. Must be 'work' or 'personal'.")
        };

        // Parse project name
        result.ProjectName = args[1];
        if (string.IsNullOrWhiteSpace(result.ProjectName))
        {
            throw new ArgumentException("Project name cannot be empty.");
        }

        // Parse optional SK flag (case insensitive)
        if (args.Length >= 3)
        {
            var skArg = args[2].ToLowerInvariant();
            if (skArg == "sk")
            {
                result.IncludeSpecKit = true;
            }
            else
            {
                throw new ArgumentException($"Invalid third parameter '{args[2]}'. Must be 'sk' or omitted.");
            }
        }

        return result;
    }
}