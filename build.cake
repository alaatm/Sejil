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
const string CLIENT_DIR = "./src/LogsExplorer.Client";
const string SERVER_DIR = "./src/LogsExplorer.Server";
const string SAMPLE_DIR = "./Sample";

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
	 Func<IFileSystemInfo, bool> exclude_node_modules = 
		fsi => !fsi.Path.FullPath.Contains("node_modules");

	DeleteDirectories(GetDirectories("./**/bin", exclude_node_modules), true);
	DeleteDirectories(GetDirectories("./**/obj", exclude_node_modules), true);
	if (DirectoryExists(_packFolder))
	{
		DeleteDirectory(_packFolder, true);
	}
	if (DirectoryExists(System.IO.Path.Combine(CLIENT_DIR, "dist")))
	{
		DeleteDirectory(System.IO.Path.Combine(CLIENT_DIR, "dist"), true);
	}
});

Task("ClientBuild")
	.Does(() =>
{
	// ./src/LogsExplorer.Client/> npm install
	Npm.FromPath(CLIENT_DIR).Install();
	// ./src/LogsExplorer.Client/> webpack
	Webpack.FromPath(CLIENT_DIR).Local();
	// ./src/LogsExplorer.Client/> gulp
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

Task("Restore")
	.Does(() =>
{
	DotNetCoreRestore(SERVER_DIR);
	DotNetCoreRestore(SAMPLE_DIR);
});

Task("Pack")
	.IsDependentOn("Clean")
	.IsDependentOn("ClientBuild")
	.IsDependentOn("CopyEmbeddedHtml")
	.IsDependentOn("Restore")
	.Does(() =>
{
	DotNetCorePack(SERVER_DIR, new DotNetCorePackSettings 
	{ 
		Configuration = configuration,
		OutputDirectory = Directory(_packFolder)
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
