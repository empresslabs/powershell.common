using System.Diagnostics.CodeAnalysis;
using Nuke.Common;

class Build : NukeBuild {
  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Parameter("NuGet API key for package publishing")]
  [Secret]
  readonly string NuGetApiKey;

  [Parameter("The NuGet source to publish to")]
  readonly string NuGetSource = "https://api.nuget.org/v3/index.json";

  Target Clean => _ => _
    .Before(Restore)
    .Executes(() => { });

  Target Restore => _ => _
    .Executes(() => { });

  Target Compile => _ => _
    .DependsOn(Restore)
    .Executes(() => { });

  public static int Main() => Execute<Build>(x => x.Compile);
}
