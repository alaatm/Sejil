#addin "nuget:https://www.nuget.org/api/v2?package=Newtonsoft.Json"
#tool nuget:?package=OpenCover
#tool nuget:?package=ReportGenerator
#r "cake-tools\Cake.Npm.dll"
#r "cake-tools\Cake.Webpack.dll"
#r "cake-tools\Cake.Gulp.dll"

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
const string CLIENT_DIR = "./src/Sejil.Client/build";
const string SERVER_DIR = "./src/Sejil.Server";
const string SERVER_TESTS_DIR = "./test/Sejil.Server.Test";

var _packFolder = "./nuget-build/";

Setup(context =>
{
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
	if (DirectoryExists("./src/Sejil.Server/bin")) DeleteDirectory("./src/Sejil.Server/bin", true);
	if (DirectoryExists("./src/Sejil.Server/obj")) DeleteDirectory("./src/Sejil.Server/obj", true);
	if (DirectoryExists("./test/Sejil.Server.Test/bin"))DeleteDirectory("./test/Sejil.Server.Test/bin", true);
	if (DirectoryExists("./test/Sejil.Server.Test/obj")) DeleteDirectory("./test/Sejil.Server.Test/obj", true);

	if (DirectoryExists(_packFolder)) DeleteDirectory(_packFolder, true);
	if (DirectoryExists(CLIENT_DIR) DeleteDirectory(CLIENT_DIR, true);
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
	var html = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, "index.html"));
	var manifest = JObject.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, "asset-manifest.json")));
	var cssPath = manifest["main.css"].ToString();
	var jsPath = manifest["main.js"].ToString();
	var appCss = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, cssPath));
	appCss = appCss.Substring(0, appCss.IndexOf("/*# sourceMappingURL=main."));
	var appJs = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, jsPath));
	appJs = appJs.Substring(0, appJs.IndexOf("//# sourceMappingURL=main"));

	html = html.Replace("<script type=\"text/javascript\" src=\"/" + jsPath +"\"></script>", "<script>" + appJs + "</script>");
	html = html.Replace("<link href=\"/" + cssPath  +"\" rel=\"stylesheet\">", "<style>" + appCss + "</style>");

	System.IO.File.WriteAllText(System.IO.Path.Combine(SERVER_DIR, "index.html"), html);
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
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
