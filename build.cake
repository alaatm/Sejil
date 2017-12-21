#addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json"
#tool nuget:?package=OpenCover
#tool nuget:?package=ReportGenerator
#r "cake-tools\Cake.Npm.dll"

using Newtonsoft.Json.Linq;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
const string CLIENT_DIR = "./src/Sejil.Client";
const string SERVER_DIR = "./src/Sejil.Server";
const string SERVER_TESTS_DIR = "./test/Sejil.Server.Test";

var _packFolder = "./nuget-build/";
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

Task("Clean")
	.Does(() =>
{
	DeleteDir("./src/Sejil.Server/bin");
	DeleteDir("./src/Sejil.Server/bin");
	DeleteDir("./src/Sejil.Server/obj");
	DeleteDir("./test/Sejil.Server.Test/bin");
	DeleteDir("./test/Sejil.Server.Test/obj");

	DeleteDir(_packFolder);
	DeleteDir(CombinePaths(CLIENT_DIR, "build"));
});

Task("ClientBuild")
	.Does(() =>
{
	//./src/Sejil.Client/> npm install
	Npm.FromPath(CLIENT_DIR).Install();
	// ./src/Sejil.Client/> npm run build
	Npm.FromPath(CLIENT_DIR).RunScript("build");
});

Task("CopyEmbeddedHtml")
	.Does(() =>
{
	var html = ReadFile(CombinePaths(CLIENT_DIR, "build", "index.html"));
	var manifest = JObject.Parse(ReadFile(CombinePaths(CLIENT_DIR, "build", "asset-manifest.json")));
	var cssPath = manifest["main.css"].ToString();
	var jsPath = manifest["main.js"].ToString();
	var appCss = ReadFile(CombinePaths(CLIENT_DIR, "build", cssPath));
	appCss = appCss.Substring(0, appCss.IndexOf("/*# sourceMappingURL=main."));
	var appJs = ReadFile(CombinePaths(CLIENT_DIR, "build", jsPath));
	appJs = appJs.Substring(0, appJs.IndexOf("//# sourceMappingURL=main"));

	html = html.Replace("<script type=\"text/javascript\" src=\"/" + jsPath +"\"></script>", "<script>" + appJs + "</script>");
	html = html.Replace("<link href=\"/" + cssPath  +"\" rel=\"stylesheet\">", "<style>" + appCss + "</style>");

	System.IO.File.WriteAllText(CombinePaths(SERVER_DIR, "index.html"), html);
});

Task("Build")
	.Does(() =>
{
	DotNetCoreRestore(".");
	DotNetCoreBuild(".", new DotNetCoreBuildSettings
	{
		Configuration = configuration,
	});
});

Task("Test")
	.Does(() =>
{
	DotNetCoreTest(SERVER_TESTS_DIR, new DotNetCoreTestSettings 
	{ 
		Configuration = configuration,
	});
});

Task("Pack")
	.IsDependentOn("Clean")
	.IsDependentOn("ClientBuild")
	.IsDependentOn("CopyEmbeddedHtml")
	.IsDependentOn("Build")
	.Does(() =>
{
	DotNetCorePack(SERVER_DIR, new DotNetCorePackSettings 
	{ 
		Configuration = configuration,
		OutputDirectory = Directory(_packFolder),
		IncludeSymbols = true,
		NoBuild = true
	});
});

Task("CoverageReport")
	.Does(() =>
{
	var reportFileName = System.DateTime.UtcNow.ToString("ddMMMyy-HHmmss") + ".xml";

	var projName = "Sejil.Test";
	var coverageRootPath = System.IO.Path.Combine(SERVER_TESTS_DIR, "coverage");
	var coverageXmlFilePath = System.IO.Path.Combine(coverageRootPath, projName + "-" + reportFileName);

	if (!System.IO.Directory.Exists(coverageRootPath)) System.IO.Directory.CreateDirectory(coverageRootPath);

	OpenCover(
		tool => tool.DotNetCoreTest(SERVER_TESTS_DIR, new DotNetCoreTestSettings 
		{ 
			Configuration = configuration, 
		}),
		new FilePath(coverageXmlFilePath),
		new OpenCoverSettings { OldStyle = true }.WithFilter("+[" + projName.Replace(".Test", "") + "*]* -[*.Test]*")
	);

	ReportGenerator(
		coverageXmlFilePath, 
		System.IO.Path.Combine(coverageRootPath, "report"), 
		new ReportGeneratorSettings 
		{ 
			HistoryDirectory = coverageRootPath,
			ReportTypes = new List<ReportGeneratorReportType> { ReportGeneratorReportType.Html, ReportGeneratorReportType.Badges }
		});
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// HELPER METHODS
//////////////////////////////////////////////////////////////////////

void DeleteDir(string path)
{
	if (_context.DirectoryExists(path))
	{
		_context.DeleteDirectory(path, new DeleteDirectorySettings
		{
			Force = true,
			Recursive = true
		});
	}
}

string CombinePaths(params string[] paths)
{
	return System.IO.Path.Combine(paths);
}

string ReadFile(string path)
{
	return System.IO.File.ReadAllText(path);
}


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);