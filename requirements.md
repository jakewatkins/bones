# Bones 

## high level description
I want to create a tool like Spec-Kit.  I'll call it Bones.  It will be implemented in C# using .net 9 and it will be a console application.  We can use whatever nuget libraries are required. we don't need to worry about logging.  If there is an error, please display a helpful error message on the console in red and stop execution.

Bones will take 1 optional parameter

	- SK

for example:
- bones 
- bones -sk
- bones -SK

the SK flag will not be case sensitive
If the SK flag is included github spec kit will be included in the project and configured to use copilot, and shell scripts

Bones will copy files from my repo called "copilot-resources" whcih can be found at: https://github.com/jakewatkins/copilot-resources/tree/main
The copilot-resources repo is public so no authentication is necassary.

If .github directory does not exist, bones will create it.
If .github/prompts does not exist, bones will create it.

When Bones runs it will download file-list.json and prompt-list.json from copilot-resources. Both files use the same JSON schema:
    [
        {
            "SourcePath": "prompts/unittest.prompt.md",
            "DestinationPath": ".github/prompts/unittest.prompt.md"
        },
        {
            "SourcePath": "prompts/review.prompt.md",
            "DestinationPath": ".github/prompts/review.prompt.md"
        }
    ]

Bones will download and copy all of the files in file-list.json to their specified DestinationPath inside the current working directory.
Bones will download and copy all of the files in prompt-list.json to the .github/prompts directory in the current working directory, ignoring the DestinationPath field.  Use the SourcePath to determine the filename.
In both cases (files and prompts) if the file already exists - skip it.  Print a "#" in yellow and continue on.
Bones will print dots (".") in blue on the screen as it copies each file and prompt to let the user know it is working.   One dot per file and one dot per prompt.
After all of the files and prompts have been downloaded Bones will do a git init to the project to setup source control.  This should only be done if the .git directory is missing.
If git has already been initialized for the project print a message saying git has already been initialized.  Use green.

If the user has included the SK flag Bones will setup spec-kit using the following command:
    specify --ai copilot --script sh [project name]
    the [project name] will be the name of the current working directory.  for example if we're in "/Users/jakewatkins/source/projects/bones" then project name would be "bones".
If spec-kit has not been installed, print an error message saying so (in red) and continue.

Once everything has completed print "done" in green and exit.

# Non-functional requirements

## error handling
All errors will be handled as generic errors
we will implement more specific error handling as the tool matures.
All errors includes: network failures, permissions, invalid json and file not found errors.

## configuration
Bones will use a json config file (appsettings.json) and use the standard Microsoft.Extensions.Configuration packages for configuration.
To get the current directory and find the appsettings.json file use:
    string assemblyDirectory = AppContext.BaseDirectory;
Then to load the configuration file, use the following code:
    var configuration = builder.Configuration
    .SetBasePath(assemblyDirectory)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"local.appsettings.json", true, true)
    .AddEnvironmentVariables()
    .Build();
If appsettings.json is missing print an error message in red and exit.
The appsettings.json file will contain 1 setting: "ResourceRepo" and the value will be: "https://github.com/jakewatkins/copilot-resources"



## IoC/DI
Use dependency injection to setup services in the application

# Functional Requirements

## Requirement 0
we want to copy files from github without cloning the repo. We can do this by doing an HTTP get against the full URL to the file.
The repo we are copying from is public so no authentication is needed.
Bones will download and copy all of the files listed in file-list.json to their specified DestinationPath inside the current working directory.  For example:
     {
        "SourcePath": ".editorconfig",
        "DestinationPath": ".editorconfig"
    }
    and Bones is run in /Users/jakewatkins/source/projects/test, then it would download https://raw.githubusercontent.com/jakewatkins/copilot-resources/main/.editorconfig and copy it to /Users/jakewatkins/source/projects/test/.editorconfig

## Requirement 1
We want to copy the resusable prompts that are stored in the repo to the project's .github/prompts directory.
Bones will download and copy all of the files in prompt-list.json to the .github/prompts directory in the current working directory.
Bones will download and copy all of the files listed in prompt-list.json to .github/prompts directory in the current project directory.  Bones will ignore the DestinationPath field in this case. Use the SourcePath to determine the filename.
For example:
    {
        "SourcePath": "prompts/unittest.prompt.md",
        "DestinationPath": ".githubs/unittest.prompt.md"
    }
    and Bones is run in /Users/jakewatkins/source/projects/test, then it would download https://raw.githubusercontent.com/jakewatkins/copilot-resources/main/prompts/unittest.prompt.md and copy it to /Users/jakewatkins/source/projects/test/.github/prompts/unittest.prompt.md
