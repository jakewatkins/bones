# bones requirements

I want to create a tool like Spec-Kit.  I'll call it Bones.  It will be implemented in C# using .net 9 and it will be a console application.  We can use whatever nuget libraries are required. we don't need to worry about logging.  If there is an error, please display a helpful error message on the console in red and stop execution.

Bones will take 2 parameters
	- Work or Personal
	- Project name
	- SK

for example:
- bones work hermes 
- bones personal lifttracker sk
- bones Work accounting SK
- bones PERSONAL bones sK

the work or personal parameters will not be case sensitive
the SK flag will not be case sensitive
If the project is personal it will be created under ~/source/projects
If the project is for work it will be created under ~/source/csh
The folder will be [project name]
If the SK flag is included github spec kit will be included in the project and configured to use copilot, and shell scripts
It will add my custom copilot-instructions.md file to the .github folder
It will add any reusable prompts I've created to the .github/prompts folder
If SK flag is included my consitution.md file will be copied to the .specify/memory folder

I'll store my custom prompts in a Github repo called copilot-resources
Prompts will be in the prompts folder
Constitution.md will live in the root 
Copilot-instructions.md will live in the root

When Bones runs it will just copy everything to the project.

Run specify from command line w/o interaction:
Specify
--ai copilot
--script sh

specify --ai copilot --script sh [project name]
# error handling
All errors will be handled as generic errors
we will implement more specific error handling as the tool matures.

# configuration
Bones will use a json config file (appsettings.json) and use the standard Microsoft.Extensions.Configuration packages for configuration.
We'll also use dependency injection to setup services in the application

# requirement 0
we want to copy files from github without cloning the repo. We can do this by doing an HTTP get against the full URL to the file.
Bones will have a config file that contains a list of file URLs to retreive.  This will be a comprehensive list of files, including prompts, that we want in our project.
Assume the user already has authenticate with Github and the repo being accessed is publicly accessible 
# requirement 1
when the user runs bones we validate that we have at least 2 command line parameters as described above
if there is already a directory by the name of the project: show an error message and stop
if the command line is valid bones will create a directory named after the project name in the correct directory (work or personal)
then it will create a subdirectory called .github with another subdirectory called prompts and copy all of the prompts in the copilot-resources repo's prompt directory into the .github/prompts directory.
it will copy the copilot-resources copilot-instructions.md to the root of the project folder along with the .editorconfig and .gitignore files
it will do a git init to setup git for the project.
if the SK parameter is included bones will run the command specify init including the paramters --ai copilot, --script sh, and the --here parameters.
if the SK parameter is included copy the Constitution.md file from the copilot-resources repo to the .specify/memory folder of the project after the specify script has been run.

