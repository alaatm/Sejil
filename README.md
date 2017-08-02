# Sejil [![Build status](https://ci.appveyor.com/api/projects/status/5eci12hmv92dd8i6?svg=true)](https://ci.appveyor.com/project/alaatm/sejil)

Sejil is a library that will enable you to view all your ASP.net core app's log events. It supports structured logging, querying as well as saving log event queries.

### Getting started

1. Installing package

    `Install-Package Sejil`

2. Adding code

    For ASP.net Core 1.x.x:

    <pre>
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
            <b>.AddSejil("/logs", LogLevel.Debug)</b>
            .UseKestrel()
        ...
    }
    </pre>

    For ASP.net core 2.x.x:

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