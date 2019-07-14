// adapted from https://github.com/Wyamio/Wyam/blob/develop/build.cake 

#addin "Cake.FileHelpers"
#addin "Octokit"
#tool "nuget:?package=NUnit.ConsoleRunner&version=3.7.0"
#tool "nuget:?package=NuGet.CommandLine&version=4.9.2"
#tool "AzurePipelines.TestLogger&version=1.0.2"

using Octokit;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isLocal = BuildSystem.IsLocalBuild;
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();
var isRunningOnBuildServer = !string.IsNullOrEmpty(EnvironmentVariable("AGENT_NAME")); // See https://github.com/cake-build/cake/issues/1684#issuecomment-397682686
var isPullRequest = !string.IsNullOrWhiteSpace(EnvironmentVariable("SYSTEM_PULLREQUEST_PULLREQUESTID"));  // See https://github.com/cake-build/cake/issues/2149
var buildNumber = (EnvironmentVariable("APPVEYOR_BUILD_NUMBER") ?? string.Empty).Replace('.', '-');
var branch = EnvironmentVariable("APPVEYOR_REPO_BRANCH");

var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

var version = releaseNotes.Version.ToString();
var semVersion = version + (isLocal ? string.Empty : string.Concat("-build-", buildNumber));

var msBuildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("Version", semVersion)
    .WithProperty("AssemblyVersion", version)
    .WithProperty("FileVersion", version);

var buildDir = Directory("./src/engine/shapeflow/bin") + Directory(configuration);
var buildResultDir = Directory("./build");
var nugetRoot = buildResultDir + Directory("nuget");
var chocoRoot = buildResultDir + Directory("choco");
var binDir = buildResultDir + Directory("bin");

var zipFile = "shapeflow-v" + semVersion + ".zip";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(context =>
{
    Information("Building version {0} of shapeflow.", semVersion);
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectories(new DirectoryPath[] { buildDir, buildResultDir, binDir, nugetRoot, chocoRoot });
    });

Task("Patch-Assembly-Info")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        var file = "./src/engine/SolutionInfo.cs";
        CreateAssemblyInfo(file, new AssemblyInfoSettings {
            Product = "SHAPEFLOW",
            Copyright = "Copyright \xa9 shapeflow Contributors",
            Version = version,
            FileVersion = version,
            InformationalVersion = semVersion
        });
    });

Task("Restore-Packages")
    .IsDependentOn("Patch-Assembly-Info")
    .Does(() =>
    {
        DotNetCoreRestore("./src/engine/shapeflow.sln", new DotNetCoreRestoreSettings
        {
            MSBuildSettings = msBuildSettings
        });
    });

Task("Build")
    .IsDependentOn("Restore-Packages")
    .Does(() =>
    {
        DotNetCoreBuild("./src/engine/shapeflow.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
            NoRestore = true,
            MSBuildSettings = msBuildSettings
        });
    });

Task("Publish-Client")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetCorePublish("./src/engine/shapeflow/shapeflow.csproj", new DotNetCorePublishSettings
        {
            Configuration = configuration,
            NoBuild = true,
            NoRestore = true,            
        });
    });

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .DoesForEach(GetFiles("./src/engine/tests/**/*.csproj"), project =>
    {
        DotNetCoreTestSettings testSettings = new DotNetCoreTestSettings()
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = configuration
        };
        if (isRunningOnBuildServer)
        {
            testSettings.Filter = "TestCategory!=ExcludeFromBuildServer";
            testSettings.Logger = "AzurePipelines";
            testSettings.TestAdapterPath = GetDirectories($"./tools/AzurePipelines.TestLogger.*/contentFiles/any/any").First();
        }

        Information($"Running tests in {project}");
        DotNetCoreTest(MakeAbsolute(project).ToString(), testSettings);
    })
    .DeferOnError();

Task("Copy-Files")
    .IsDependentOn("Publish-Client")
    .Does(() =>
    {
        CopyDirectory(buildDir.Path.FullPath + "/netcoreapp2.1/publish", binDir);
        CopyFiles(new FilePath[] { "LICENSE.txt", "README.md", "ReleaseNotes.md" }, binDir);
    });

Task("Zip-Files")
    .IsDependentOn("Copy-Files")
    .Does(() =>
    {
        var zipPath = buildResultDir + File(zipFile);
        var files = GetFiles(binDir.Path.FullPath + "/**/*");
        Zip(binDir, zipPath, files);
    });

Task("Create-Library-Packages")
    .IsDependentOn("Build")
    .IsDependentOn("Publish-Client")
    .Does(() =>
    {        
        // Get the set of projects to package
        List<FilePath> projects = new List<FilePath>(GetFiles("./src/engine/**/*.csproj"));
        
        // Package all nuspecs
        foreach (var project in projects)
        {
            // temporary patch to prevent from trying
            // to pack the test bed projects
            if(project.FullPath.Contains("tests"))
            {
                continue;
            }

            DotNetCorePack(
                MakeAbsolute(project).ToString(),
                new DotNetCorePackSettings
                {
                    Configuration = configuration,
                    NoBuild = true,
                    NoRestore = true,
                    OutputDirectory = nugetRoot,
                    MSBuildSettings = msBuildSettings
                });
        }
    });
    
Task("Create-Tools-Package")
    .IsDependentOn("Publish-Client")
    .WithCriteria(() => isRunningOnWindows)
    .Does(() =>
    {   
        DotNetCorePack("./src/engine/shapeflow/shapeflow.csproj", new DotNetCorePackSettings {
            Configuration = configuration,
            OutputDirectory = nugetRoot,
            MSBuildSettings = msBuildSettings
        });
    });

Task("Publish-MyGet")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => !isLocal)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRunningOnWindows)
    .WithCriteria(() => branch == "development")
    .Does(() =>
    {
        // Resolve the API key.
        var apiKey = EnvironmentVariable("MYGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve MyGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                Source = "https://www.myget.org/F/shapeflow/api/v2/package",
                ApiKey = apiKey,
                Timeout = TimeSpan.FromSeconds(600)
            });
        }
    });
    
Task("Publish-Packages")
    .IsDependentOn("Create-Packages")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var apiKey = EnvironmentVariable("SHAPEFLOW_NUGET_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Could not resolve NuGet API key.");
        }

        foreach (var nupkg in GetFiles(nugetRoot.Path.FullPath + "/*.nupkg"))
        {
            NuGetPush(nupkg, new NuGetPushSettings 
            {
                ApiKey = apiKey,
                Source = "https://api.nuget.org/v3/index.json"
            });
        }
    });
	
Task("Publish-Release")
    .IsDependentOn("Zip-Files")
    .WithCriteria(() => isLocal)
    .WithCriteria(() => isRunningOnWindows)
    // TODO: Add criteria that makes sure this is the master branch
    .Does(() =>
    {
        var githubToken = EnvironmentVariable("SHAPEFLOW_GITHUB_TOKEN");
        if (string.IsNullOrEmpty(githubToken))
        {
            throw new InvalidOperationException("Could not resolve SHAPEFLOW GitHub token.");
        }
        
        var github = new GitHubClient(new ProductHeaderValue("SHAPEFLOWCakeBuild"))
        {
            Credentials = new Credentials(githubToken)
        };
        var release = github.Repository.Release.Create("SHAPEFLOWio", "SHAPEFLOW", new NewRelease("v" + semVersion) 
        {
            Name = semVersion,
            Body = string.Join(Environment.NewLine, releaseNotes.Notes) + Environment.NewLine + Environment.NewLine
                + @"### Please see https://SHAPEFLOW.io/docs/usage/obtaining for important notes about downloading and installing.",
            TargetCommitish = "master"
        }).Result; 
        
        var zipPath = buildResultDir + File(zipFile);
        using (var zipStream = System.IO.File.OpenRead(zipPath.Path.FullPath))
        {
            var releaseAsset = github.Repository.Release.UploadAsset(release, new ReleaseAssetUpload(zipFile, "application/zip", zipStream, null)).Result;
        }
    });
    
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Create-Packages")
    .IsDependentOn("Create-Library-Packages")    
    .IsDependentOn("Create-Tools-Package");
    
Task("Package")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Zip-Files")
    .IsDependentOn("Create-Packages");

Task("Default")
    .IsDependentOn("Package");    

Task("Publish")
    .IsDependentOn("Publish-Packages")    
    .IsDependentOn("Publish-Release");
    
Task("BuildServer")
    .IsDependentOn("Run-Unit-Tests")
    .IsDependentOn("Publish-MyGet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
