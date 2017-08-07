#tool nuget:?package=OpenCover
#tool nuget:?package=ReportGenerator
#r "cake-tools\Cake.Npm.dll"
#r "cake-tools\Cake.Webpack.dll"
#r "cake-tools\Cake.Gulp.dll"

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
	if (DirectoryExists(System.IO.Path.Combine(CLIENT_DIR, "dist"))) DeleteDirectory(System.IO.Path.Combine(CLIENT_DIR, "dist"), true);
});

Task("ClientBuild")
	.Does(() =>
{
	// ./src/Sejil.Client/> npm install
	Npm.FromPath(CLIENT_DIR).Install();
	// ./src/Sejil.Client/> webpack
	Webpack.FromPath(CLIENT_DIR).Local();
	// ./src/Sejil.Client/> gulp
	Gulp.FromPath(CLIENT_DIR).Local();
});

Task("CopyEmbeddedHtml")
	.Does(() =>
{
	var html = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, "index.html"));
	var appCss = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, "dist/app.min.css"));
	var vendorScript = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, "dist/vendor.js"));
	var appScript = System.IO.File.ReadAllText(System.IO.Path.Combine(CLIENT_DIR, "dist/app.js"));

	html = html.Replace("<script src=\"./dist/vendor.js\"></script>", "<script>" + vendorScript + "</script>");
	html = html.Replace("<script src=\"./dist/app.js\"></script>", "<script>" + appScript + "</script>");
	html = html.Replace("<link rel=\"stylesheet\" href=\"app.css\">", "<style>" + appCss + "</style>");

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
