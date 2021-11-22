#addin nuget:?package=AngleSharp&Version=0.16.1
#addin nuget:?package=Cake.Coverlet&Version=2.5.4
#addin nuget:?package=Cake.Git&Version=1.1.0
#addin nuget:?package=Cake.Npm&Version=1.0.0
#tool nuget:?package=ReportGenerator&Version=4.8.13

using System.Xml.Linq;
using AngleSharp.Html.Parser;
using IOFile = System.IO.File;
using IOPath = System.IO.Path;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "release");
var rbv = Argument("rbv", "");  // On windows invoke as: ./build -t release -rbv=patch
                                // On linux invoke as  : ./build.sh -t release --rbv=patch

var clientDir = Directory("./src/Sejil.Client");
var packDir = Directory(".nupkg");
var coverageDir = Directory(".coverage");
var lcovFile = "./lcov.info";

var tstProjects = GetSubDirectories("./test").Select(p => IOPath.Combine(p.FullPath, IOPath.GetFileName(p.FullPath)) + ".csproj").ToList();

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("ClientClean")
    .Does(() =>
{
	CleanDirectories(clientDir.Path.Combine("build").FullPath);
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./src/**/bin/" + configuration);
    CleanDirectories("./src/**/obj");
    CleanDirectory(packDir);
    CleanDirectory(coverageDir);
    if (FileExists(lcovFile)) DeleteFile(lcovFile);
});

Task("ClientBuild")
    .IsDependentOn("ClientClean")
	.Does(() =>
{
    if (target.ToUpper() == "COVER") { return; }

	//./src/Sejil.Client/> npm install
    NpmInstall(settings => settings.FromPath(clientDir));
	//./src/Sejil.Client/> npm run build
    NpmRunScript("build", settings => settings.FromPath(clientDir));
});

Task("CopyEmbeddedHtml")
    .IsDependentOn("ClientBuild")
	.Does(() =>
{
    if (target.ToUpper() == "COVER") { return; }

	var clientBuildDir = clientDir.Path.Combine("build");
    var htmlPath = clientBuildDir.Combine("index.html").FullPath;
	var html = IOFile.ReadAllText(htmlPath);

    var tagValueList = new Dictionary<string, string>();

    var parser = new HtmlParser();
    var doc = parser.ParseDocument(html);

    foreach (var css in doc.All.Where(p => p.TagName == "LINK" && p.Attributes["href"].Value.StartsWith("/static")))
    {
        var tag = css.OuterHtml;
        var relPath = css.Attributes["href"].Value.Substring(1);
        var fullPath = clientBuildDir.Combine(relPath).FullPath;

        tagValueList.Add(tag, "<style>" + IOFile.ReadAllText(fullPath) + "</style>");
    }
    foreach (var js in doc.Scripts.Where(p => p.Source?.StartsWith("/static") ?? false))
    {
        var tag = js.OuterHtml;
        var relPath = js.Source.Substring(1);
        var fullPath = clientBuildDir.Combine(relPath).FullPath;

        tagValueList.Add(tag, "<script>" + IOFile.ReadAllText(fullPath) + "</script>");
    }

    foreach (var kvp in tagValueList)
    {
        html = html.Replace(kvp.Key, kvp.Value);
    }

	IOFile.WriteAllText("./src/Sejil.Server/Sejil/index.html", html);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(".");
});

Task("Build")
	.IsDependentOn("CopyEmbeddedHtml")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild(".", new DotNetCoreBuildSettings
    {
        NoRestore = true,
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest(".", new DotNetCoreTestSettings
    {
        NoRestore = true,
        NoBuild = true,
        Configuration = configuration,
    });
});

Task("Pack")
	.IsDependentOn("Test")
	.Does(() =>
{
	DotNetCorePack(".", new DotNetCorePackSettings
	{
		Configuration = configuration,
		OutputDirectory = packDir,
        NoRestore = true,
        NoBuild = true,
	});
});

Task("Cover")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testSettings = new DotNetCoreTestSettings
    {
        NoRestore = true,
        NoBuild = true,
        Configuration = configuration,
        Framework = "net6.0",
    };

    var mergeWith = File(coverageDir.Path + $"/{DateTime.UtcNow.Ticks}.json");

    foreach (var prj in tstProjects)
    {
        var outputName = IOPath.GetFileNameWithoutExtension(prj).Replace(".Test", "").Replace(".","");

        var coverletSettings = new CoverletSettings
        {
            CollectCoverage = true,
            CoverletOutputFormat = CoverletOutputFormat.lcov | CoverletOutputFormat.opencover | CoverletOutputFormat.json,
            CoverletOutputDirectory = coverageDir,
            CoverletOutputName = outputName,
            MergeWithFile = mergeWith,
        };

        DotNetCoreTest(prj, testSettings, coverletSettings);
        IOFile.Copy(File(coverageDir.Path + $"/{outputName}.net6.0.json"), mergeWith.Path.FullPath, true);
    }

    // Copy lcov file to root for code coverage highlight in vscode
    var lcov = GetFiles(coverageDir.Path + "/*.info").First();
    CopyFile(lcov, lcovFile);

    // Generate coverage report
    if (IsRunningOnWindows())
    {
        var opencover = GetFiles(coverageDir.Path + "/*.opencover.xml").First();
        ReportGenerator(File(opencover.FullPath), coverageDir);
    }
});

// This will create and push an annotated tagged commit to create a release.
// The version is determined by the passed rbv argument (major, minor or patch).
Task("Release")
    .IsDependentOn("Test")
    .Does(() =>
{
    if (GitHasStagedChanges(".") || GitHasUncommitedChanges(".") || GitHasUntrackedFiles("."))
    {
        Error("Cannot release when staged changes, uncommited changes or untracked files exist.");
        return;
    }

    var csproj = File("./src/Sejil.Server/Directory.Build.props").Path.FullPath;

    var version = XDocument.Load(csproj).Root
        .Element("PropertyGroup")
        .Element("VersionPrefix").Value;

    var split = version.Split('.');
    var major = int.Parse(split[0]);
    var minor = int.Parse(split[1]);
    var patch = int.Parse(split[2]);

    var releaseVersion = "";
    switch (rbv)
    {
        case "major": releaseVersion = $"{major + 1}.0.0"; break;
        case "minor": releaseVersion = $"{major}.{minor + 1}.0"; break;
        case "patch": releaseVersion = $"{major}.{minor}.{patch + 1}"; break;
        default: Error("Invalid rbv switch. Must be one of the following: major, minor, patch."); return;
    }

    // Using WriteAllText instead of XDocument so that format isn't screwed.
    IOFile.WriteAllText(
        csproj,
        IOFile.ReadAllText(csproj)
            .Replace(
                $"<VersionPrefix>{version}</VersionPrefix>",
                $"<VersionPrefix>{releaseVersion}</VersionPrefix>"));

    IOFile.WriteAllText(
        "./README.md",
        IOFile.ReadAllText("./README.md")
            .Replace(
                $"dotnet add package Sejil --version {version}",
                $"dotnet add package Sejil --version {releaseVersion}"));

    var tag = "v" + releaseVersion;
    var name = "Alaa Masoud";
    var email = "alaa.masoud@live.com";

    GitAdd(".", csproj);
    GitAdd(".", "./README.md");
    GitCommit(".", name, email, tag);
    GitTag(".", tag, name, email, tag);
    GitPush(".");
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
