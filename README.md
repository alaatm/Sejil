# Sejil

![Build](https://github.com/alaatm/Sejil/workflows/Build/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/Sejil.svg)](https://www.nuget.org/packages/Sejil/)

Sejil is a library that enables you to capture, view and filter your ASP.net core app's log events right from your app. It supports structured logging, querying as well as saving log event queries.

## Quick Links

- [Getting started](#getting-started)
- [Additional logging configuration](#additional-logging-configuration)
- [Filtering logs](#filtering-logs)
- [Features and Screenshots](#features-and-screenshots)
- [Building](#building)
- [License](#license)

## Getting started

1. Installing [Sejil](https://www.nuget.org/packages/Sejil/) package

    ```powershell
    dotnet add package Sejil --version 3.0.4
    ```

2. Adding code

    Add below code to **Program.cs**:

    ```csharp
    public static IWebHost BuildWebHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSejil()  // <-- Add this line
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());            
    ```

    Add below code to **Startup.cs**

    ```csharp
    using Sejil;

    public class Startup
    {    
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ...
            app.UseSejil();  // <-- Add this line
            // ...
        }
    }
    ```

    (Optional) To require authentication for viewing logs:

    ```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureSejil(options =>
            {
                options.AuthenticationScheme = /* Your authentication scheme */
            });
        }
    ```

    (Optional) To change the logs page title (Defaults to *Sejil* if not set):

    ```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureSejil(options =>
            {
                options.Title = "My title";
            });
        }
    ```    

3. Navigate to *http://your-app-url/sejil* to view your app's logs.

## Additional logging configuration

* If you want to enable loggers registered through the Microsoft.Extensions.Logging API. Specify `true` to the `writeToProviders` parameter:

    ```csharp
    public static IWebHost BuildWebHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSejil(writeToProviders: true)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());            
    ```

* If you want to enable other Serilog sinks, use the `sinks` parameter to do that. For exmaple, the following code adds the console Serilog sink:

    ```csharp
    public static IWebHost BuildWebHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSejil(sinks: sinks =>
            {
                sinks.Console();
            }
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());            
    ```

* You can use Serilog's configuration to specifiy minimum log level overrides for certain namespaces. For example, to limit logging from `Microsoft.AspNetCore` to `Warning`:

    ```json
    {
        "Serilog": {
            "MinimumLevel": {
                "Override": {
                    "Microsoft.AspNetCore": "Warning"
                }
            }
        },
        "AllowedHosts": "*"
    }    
    ```
## Filtering logs

In addition to filtering logs by level; exceptions only; and date range via the UI, you can also search by text to filter your logs:

* To filter logs by a specific search term, just type it in the search box and hit enter. If the search term contains spaces or any of the following `=`, `!`, `(`, `)` then make sure to enclose it in double quotes.

    Examples:
    ```
    AboutController
    "some message"
    "ZGFzZGE="
    ```

* To filter logs by a specific property, use the following pattern:

    * `property-name` [ ` = | != | like | not like ` ] `search-term`

        Examples:
        ```sql
        SourceContext = 'Microsoft.AspNetCore.Hosting.Internal.WebHost'
        SourceContext like '%Hosting%'
        ElapsedMilliseconds >= 500
        ```

* To filter logs by a built-in property (`message`, `messageTemplate`, `level`, `timestamp`, `exception`), prefix it with `@`

    Examples:
    ```sql
    @message = 'Request did not match any routes'
    @exception like '%ArgumentNullException%'
    ```

* You can combine multiple search expressions with `and` `or` and use `(` `)` for precedence.

    Examples:
    ```sql
    StatusCode like '%' and StatusCode != 200   -- Get all logs that contain a 'StatusCode' property and that the property value does not equal 200.    
    ```

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

## Building

To build the project, you just need to clone the repo then run the following commands:

```powershell
git clone https://github.com/alaatm/Sejil.git
cd ./Sejil
dotnet tool restore
dotnet cake
```

You can run one of the sample apps afterwards:

```powershell
cd ./sample/SampleBasic
dotnet run
```

## License

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Copyright &copy; Alaa Masoud.

This project is provided as-is under the Apache 2.0 license. For more information see the [LICENSE file](https://github.com/alaatm/Sejil/blob/master/LICENSE).