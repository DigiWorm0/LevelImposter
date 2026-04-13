var target = Argument("target", "Build");

var workflow = BuildSystem.GitHubActions.Environment.Workflow;

Task("Build").Does(() =>
{
    var settings = new DotNetBuildSettings
    {
        Configuration = "Release",
        MSBuildSettings = new DotNetMSBuildSettings()
        {
            VersionSuffix = "ci." + workflow.RunNumber
        }
    };

    DotNetBuild(".", settings);
});

RunTarget(target);