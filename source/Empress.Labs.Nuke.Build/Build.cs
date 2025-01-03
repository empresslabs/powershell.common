using LibGit2Sharp;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Serilog;

class Build : NukeBuild {
  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Parameter("NuGet API key for package publishing")]
  [Secret]
  readonly string NuGetApiKey;

  [Parameter("The NuGet source to publish to")]
  readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

  readonly Repository Repository = new(RootDirectory);
  string CurrentBranch;

  AbsolutePath SolutionPath => RootDirectory / "Empress.Labs.PowerShell.Common.sln";
  AbsolutePath ProjectPath => RootDirectory / "source" / "Empress.Labs.PowerShell.Common/Empress.Labs.PowerShell.Common.csproj";
  AbsolutePath PublishPath => RootDirectory / "publish";

  Target Clean => _ => _
    .Executes(() => {
      Log.Information("Cleaning up...");

      PublishPath.CreateOrCleanDirectory();
    });

  Target Restore => _ => _
    .DependsOn(Clean)
    .Executes(() => {
      Log.Information("Restoring packages...");

      DotNetTasks
        .DotNetRestore();
    });

  Target Test => _ => _
    .DependsOn(Restore)
    .Executes(() => {
      Log.Information("Running tests...");

      DotNetTasks
        .DotNetTest(s => s
          .SetConfiguration(Configuration)
          .SetNoRestore(true)
          .SetProjectFile(SolutionPath));
    });

  Target Pack => _ => _
    .DependsOn(Test)
    .Executes(() => {
      Log.Information("Packing...");

      var version = Repository.Describe(Repository.Head.Tip, new DescribeOptions {
        Strategy = DescribeStrategy.Tags,
        MinimumCommitIdAbbreviatedSize = 60,
        AlwaysRenderLongFormat = true
      });

      var formattedVersion = version.Substring(1, version.IndexOf('-') - 1);

      Log.Warning($"Version: {formattedVersion}");

      DotNetTasks
        .DotNetPack(s => s
          .SetConfiguration(Configuration)
          .SetOutputDirectory(PublishPath)
          .SetNoBuild(true)
          .SetNoRestore(true)
          .SetProject(ProjectPath)
          .SetProperty("EmpressLabsSemVer", version)
          .SetVersion(formattedVersion));
    });

  Target Publish => _ => _
    .DependsOn(Pack)
    .Requires(() => NuGetApiKey)
    .Executes(() => {
      Log.Information("Publishing...");

      DotNetTasks
        .DotNetNuGetPush(s => s
          .SetSource(NuGetSource)
          .SetApiKey(NuGetApiKey)
          .SetTargetPath(PublishPath / "*.nupkg")
          .EnableSkipDuplicate());
    });

  public static int Main()
    => Execute<Build>(build => build.Publish);
}
