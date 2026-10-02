using System;
using System.Collections.Generic;
using System.Linq;
using Fallout.Common;
using Fallout.Common.CI.GitHubActions;
using Fallout.Common.Execution;
using Fallout.Common.IO;
using Fallout.Common.ProjectModel;
using Fallout.Common.Tooling;
using Fallout.Common.Tools.DotNet;
using Fallout.Common.Tools.GitVersion;
using Fallout.Common.Tools.ReportGenerator;
using Fallout.Common.Tools.Xunit;
using Fallout.Common.Utilities.Collections;
using static Fallout.Common.Tools.DotNet.DotNetTasks;
using static Fallout.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[UnsetVisualStudioEnvironmentVariables]
[DotNetVerbosityMapping]
class Build : FalloutBuild
{
    const string NetFrameworkVersion = "net472";

    public static int Main() => Execute<Build>(x => x.Push);

    GitHubActions GitHubActions => GitHubActions.Instance;

    string BranchSpec => GitHubActions?.Ref;

    string BuildNumber => GitHubActions?.RunNumber.ToString();

    [Parameter("The key to push to Nuget")]
    [Secret]
    readonly string ApiKey;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [GitVersion(Framework = "net10.0")]
    readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "Src";

    AbsolutePath TestsDirectory => RootDirectory / "Tests";

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";

    /// <summary>
    /// We need to provide test settings.
    /// By default, code with [DebuggerNonUserCode] is excluded.
    /// But this is used several times in our code.
    /// We can use the "runsettings" format (VSTest) also for the MTP platform tests.
    /// </summary>
    static AbsolutePath CoverageSettingsFile => RootDirectory / "Tests" / "CodeCoverage.runsettings";

    string SemVer;

    Target Clean => d => d
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(path => path.DeleteDirectory());
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target CalculateNugetVersion => d => d
        .Executes(() =>
        {
            SemVer = GitVersion.SemVer;
            if (IsPullRequest)
            {
                Serilog.Log.Information(
                    "Branch spec {BranchSpec} is a pull request. Adding build number {BuildNumber}",
                    BranchSpec, BuildNumber);

                SemVer = string.Join('.', GitVersion.SemVer.Split('.').Take(3).Union(new[] { BuildNumber }));
            }

            Serilog.Log.Information("SemVer = {SemVer}", SemVer);
        });

    bool IsPullRequest => GitHubActions?.IsPullRequest ?? false;

    Target Restore => d => d
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => d => d
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration("CI")
                .EnableNoLogo()
                .EnableNoRestore());
        });

    Target ApiChecks => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                    .SetConfiguration("Release")
                    .SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
                    .EnableNoBuild()
                    .SetResultsDirectory(TestResultsDirectory)
                    .CombineWith(cc => cc.SetProjectFile(Solution.Approval_Tests)),
                completeOnFailure: true);
        });

    Target UnitTests => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            IEnumerable<string> frameworks = Solution.AwesomeAssertions_Json_Specs.GetTargetFrameworks();
            if (!EnvironmentInfo.IsWin)
                frameworks = frameworks.Except([NetFrameworkVersion]);

            DotNetTest(s => s
                    .SetConfiguration("Debug")
                    .SetProjectFile(Solution.AwesomeAssertions_Json_Specs)
                    .SetProcessEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en-US")
                    .SetResultsDirectory(TestResultsDirectory)
                    .EnableNoBuild()
                    .CombineWith(
                        frameworks,
                        (settings, framework) => 
                        {
                            var coverageFile = $"{framework}.cobertura.xml";
                            return settings
                                .SetFramework(framework)
                                .SetProcessAdditionalArguments(
                                    "--coverage",
                                    $"--coverage-output={coverageFile}",
                                    "--report-trx",
                                    "--report-trx-filename",
                                    $"{framework}.trx",
                                    "--coverage-settings",
                                    CoverageSettingsFile);
                        }), 
                completeOnFailure: true);
        });

    Target CodeCoverage => d => d
        .DependsOn(UnitTests)
        .Executes(() =>
        {
            string generator = NuGetToolPathResolver.GetPackageExecutable(
                "ReportGenerator", "ReportGenerator.dll", framework: "net10.0");
            ReportGenerator(s => s
                .SetProcessToolPath(generator)
                .SetTargetDirectory(TestResultsDirectory / "reports")
                .AddReports(TestResultsDirectory / "**/*.cobertura.xml")
                .AddReportTypes("HtmlInline_AzurePipelines_Dark", "lcov")
                .SetClassFilters("-System.Diagnostics.CodeAnalysis.StringSyntaxAttribute")
                .SetAssemblyFilters("+AwesomeAssertions.Json"));

            string link = TestResultsDirectory / "reports" / "index.html";

            Serilog.Log.Information($"Code coverage report: \x1b]8;;file://{link.Replace('\\', '/')}\x1b\\{link}\x1b]8;;\x1b\\");
        });

    Target Pack => d => d
        .DependsOn(ApiChecks)
        .DependsOn(UnitTests)
        .DependsOn(CodeCoverage)
        .DependsOn(CalculateNugetVersion)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution.AwesomeAssertions_Json)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetConfiguration("Release")
                .EnableNoLogo()
                .EnableNoRestore()
                .EnableContinuousIntegrationBuild() // Necessary for deterministic builds
                .SetVersion(SemVer));
        });

    Target Push => d => d
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => IsTag)
        .Executes(() =>
        {
            var packages = ArtifactsDirectory.GlobFiles("*.nupkg");

            Assert.NotEmpty(packages);

            DotNetNuGetPush(s => s
                .SetApiKey(ApiKey)
                .EnableSkipDuplicate()
                .SetSource("https://api.nuget.org/v3/index.json")
                .EnableNoSymbols()
                .CombineWith(packages,
                    (v, path) => v.SetTargetPath(path)));
        });

    bool IsTag => BranchSpec != null && BranchSpec.Contains("refs/tags", StringComparison.OrdinalIgnoreCase);
}
