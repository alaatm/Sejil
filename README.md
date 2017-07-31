# Sejil

Sejil is a library that will enable you to view all your ASP.net core app's log events. It supports structured logging, querying as well as saving log event queries.

### Getting started

1. Installing package

`Install-Package Sejil`

2. Adding code

    Add highlted code below in your program.cs

    <pre>
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            <b>.AddSejil("/logs", LogLevel.Debug)</b>
            .UseStartup<Startup>()
            .Build();
    </pre>

    Add highlited code below in your startup.cs

    <pre>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        ...
        <b>app.UseSejil();</b>
        ...
    }
    </pre>