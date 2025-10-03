namespace Bones.Models;

public class GitHubFileConfig
{
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = true;
    public bool OnlyWhenSpecKit { get; set; } = false;
}

public class GitHubFilesConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public List<GitHubFileConfig> Files { get; set; } = new();
    public List<GitHubFileConfig> PromptFiles { get; set; } = new();
}

public class DirectoriesConfig
{
    public string PersonalBase { get; set; } = string.Empty;
    public string WorkBase { get; set; } = string.Empty;
}

public class BonesConfig
{
    public GitHubFilesConfig GitHubFiles { get; set; } = new();
    public DirectoriesConfig Directories { get; set; } = new();
}

public class CommandLineArgs
{
    public ProjectType ProjectType { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public bool IncludeSpecKit { get; set; }
}

public enum ProjectType
{
    Personal,
    Work
}