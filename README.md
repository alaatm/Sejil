# Sejil

[![Build status](https://ci.appveyor.com/api/projects/status/5eci12hmv92dd8i6?svg=true)](https://ci.appveyor.com/project/alaatm/sejil)

Sejil is a library that enables you to capture, view and filter your ASP.net core app's log events right from your app. It supports structured logging, querying as well as saving log event queries.

## Quick Links

- [Getting started](#getting-started)
- [Building](#building)
- [Features and Screenshots](#features-and-screenshots)
- [License](#license)

## Getting started

1. Installing package

    `Install-Package Sejil`

2. Adding code

    For ASP.net Core 1.x.x:

    <pre>
    public static void Main(string[] args)
    {
        var host = new WebHostBuilder()
            <b>.AddSejil("/sejil", LogLevel.Debug)</b>
            .UseKestrel()
        ...
    }
    </pre>

    For ASP.net core 2.x.x:

    <pre>
    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            <b>.AddSejil("/sejil", LogLevel.Debug)</b>
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

3. Navigate to *http://your-app-url/sejil* to view your app's logs.

## Building

To build the project, you just need to clone the repo then run the build command:

<pre>
git clone https://github.com/alaatm/Sejil.git
cd ./Sejil
./build.ps1  # If running Windows
./build.sh   # If running Linux/OSX
</pre>

You can run one of the sample apps afterwards, `Sample1.0` targets `netcoreapp1.1` while `Sample2.0` targets `netcoreapp2.0`:

<pre>
cd ./sample/Sample2.0
dotnet run
</pre>

## Features and Screenshots

- View your app's logs

    <img src="./assets/001-screenshot-main_opt.jpg" width="800">

- View properties specific to a certain log entry

    <img src="./assets/002-screenshot-properties_opt.jpg" width="800">

- Query your logs

    <img src="./assets/003-screenshot-query_opt.jpg" width="800">

- Mix multiple filters with your query to further limit the results

    <img src="./assets/004-screenshot-query-and-filter_opt.jpg" width="800">

- Save your queries for later use

    <img src="./assets/005-screenshot-save-query_opt.jpg" width="800">

- Load your saved queries

    <img src="./assets/006-screenshot-load-query_opt.jpg" width="800">

## License

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Copyright &copy; Alaa Masoud.

This project is provided as-is under the Apache 2.0 license. For more information see the [LICENSE file](https://github.com/alaatm/Sejil/blob/master/LICENSE).