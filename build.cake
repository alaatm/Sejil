#addin nuget:?package=AngleSharp&Version=0.14.0
#addin nuget:?package=Cake.Coverlet&Version=2.5.1
#addin nuget:?package=Cake.Git&Version=0.22.0
#addin nuget:?package=Cake.Npm&Version=0.17.0
#tool nuget:?package=ReportGenerator&Version=4.7.1

using System.Xml.Linq;
using AngleSharp.Html.Parser;
using IOFile = System.IO.File;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "release");
var rbv = Argument("rbv", "");  // On windows invoke as: ./build -t release -rbv=patch
                                // On linux invoke as  : ./build.sh -t release --rbv=patch

var clientDir = Directory("./src/Sejil.Client");
var packPrj = Directory("./src/Sejil.Server/Sejil.csproj");
var packDir = Directory(".nupkg");
var coverageDir = Directory(".coverage");
var lcovFile = "./lcov.info";


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

ICakeContext _context;

Setup(context =>
{
	_context = context;
	var env = configuration.ToLower() == "debug"
		? "development"
		: "production";
	System.Environment.SetEnvironmentVariable("BUILD_ENV", env);
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
    CleanDirectories(packDir);
    CleanDirectories(coverageDir);
    if (FileExists(lcovFile)) DeleteFile(lcovFile);    
});

Task("ClientBuild")
    .IsDependentOn("ClientClean")
	.Does(() =>
{
	//./src/Sejil.Client/> npm install
    NpmInstall(settings => settings.FromPath(clientDir));
	//./src/Sejil.Client/> npm run build
    NpmRunScript("build", settings => settings.FromPath(clientDir));
});

Task("CopyEmbeddedHtml")
    .IsDependentOn("ClientBuild")
	.Does(() =>
{
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
	
	IOFile.WriteAllText("./src/Sejil.Server/index.html", html);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore(".");
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    // Create a dummy index.html so that build doesn't fail only if the target isn't pack.
    if (target.ToLower() != "pack" && !FileExists("./src/Sejil.Server/index.html"))
    {
        IOFile.WriteAllText("./src/Sejil.Server/index.html", "");
    }

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
	.IsDependentOn("CopyEmbeddedHtml")
	.IsDependentOn("Test")
	.Does(() =>
{
	DotNetCorePack(packPrj, new DotNetCorePackSettings 
	{ 
		Configuration = configuration,
		OutputDirectory = packDir,
		NoBuild = true
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
    };

    var coverletSettings = new CoverletSettings
    {
        CollectCoverage = true,
        CoverletOutputFormat = CoverletOutputFormat.lcov | CoverletOutputFormat.opencover,
        CoverletOutputDirectory = coverageDir,
        CoverletOutputName = $"{DateTime.UtcNow.Ticks}",
    };

    DotNetCoreTest(".", testSettings, coverletSettings);

    // Copy lcov file to root for code coverage highlight in vscode
    var lcov = GetFiles(coverageDir.Path + "/*.info").Single();
    CopyFile(lcov, lcovFile);

    // Generate coverage report
    if (IsRunningOnWindows())
    {
        var opencover = GetFiles(coverageDir.Path + "/*.opencover.xml").Single();
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

    var csproj = File("./src/Sejil.Server/Sejil.csproj").Path.FullPath;

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

    var tag = "v" + releaseVersion;
    var name = "Alaa Masoud";
    var email = "alaa.masoud@live.com";

    GitAdd(".", csproj);
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