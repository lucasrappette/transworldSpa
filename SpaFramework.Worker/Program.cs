using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaFramework.App;
using SpaFramework.Worker.BackgroundServices;
using SpaFramework.Worker.Processors;
using Serilog;
using Serilog.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SpaFramework.Worker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .CreateLogger();

            var builder = new HostBuilder()
                .UseSerilog((hostingContext, service, loggerConfig) =>
                {
                    loggerConfig
                        .Enrich.FromLogContext()
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Debug()
                            .Filter.ByIncludingOnly(Matching.FromSource("SpaFramework"))
                            .WriteTo.Console()
                        )
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Information()
                            .Filter.ByIncludingOnly(Matching.FromSource("SpaFramework"))
                            .WriteTo.ApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces)
                        )
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Warning()
                            .WriteTo.Console()
                            .WriteTo.File("Logs\\Warnings.txt")
                            .WriteTo.ApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces)
                        );
                })
                .ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddAzureStorage(x =>
                    {
                        x.MaxPollingInterval = TimeSpan.FromSeconds(10);
                    });
                    b.AddTimers();
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddEnvironmentVariables();

                    configApp.SetBasePath(Directory.GetCurrentDirectory());

                    string environmentName = hostContext.HostingEnvironment.EnvironmentName;

                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")))
                        environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

                    Console.WriteLine(environmentName);

                    List<string> paths = new List<string>()
                    {
                        // Dev environment, if run from bin\Debug\net5.0
                        Path.GetFullPath(@"..\..\..\..\SpaFramework.Web\appsettings.json"),
                        Path.GetFullPath($@"..\..\..\..\SpaFramework.Web\appsettings.{environmentName}.json"),

                        // Dev environment, if run from solution root
                        Path.GetFullPath(@"SpaFramework.Web\appsettings.json"),
                        Path.GetFullPath($@"SpaFramework.Web\appsettings.{environmentName}.json"),
                    };

                    // If we're running in an Azure Web Job, this environment variable will be set to the path of the main web app, whose appsettings.json files we want to use
                    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBROOT_PATH")))
                    {
                        paths.Add(Environment.GetEnvironmentVariable("WEBROOT_PATH") + "\\appsettings.json");
                        paths.Add(Environment.GetEnvironmentVariable("WEBROOT_PATH") + $"\\appsettings.{environmentName}.json");
                    }

                    // Find the parent web app's appsettings.json files
                    foreach (string path in paths)
                    {
                        if (File.Exists(path))
                        {
                            Console.Out.WriteLine("Adding appsettings.json: " + path);
                            configApp.AddJsonFile(path, true);
                        }
                        else
                        {
                            Console.Out.WriteLine("Path doesn't exist: " + path);
                        }
                    }

                    configApp.AddJsonFile($"appsettings.json", true);
                    configApp.AddJsonFile($"appsettings.{environmentName}.json", true);
                    configApp.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, b) =>
                {
                    //Microsoft.Extensions.Logging.ConsoleLoggerExtensions.AddConsole(b);
                    //b.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDataServices(hostContext.Configuration);
                    services.AddAppServices();


                    services.AddSingleton<SingletonBackgroundServicesHost, SingletonBackgroundServicesHost>();

                    //services.AddTransient<SignInManager<ApplicationUser>, SignInManager<ApplicationUser>>();
                    //services.AddTransient<UserManager<ApplicationUser>, UserManager<ApplicationUser>>();
                    //services.AddIdentity<ApplicationUser, ApplicationRole>()
                    //    .AddEntityFrameworkStores<ApplicationDbContext>();

                    services.AddTransient<WorkItemData, WorkItemData>();
                });

            var host = builder.Build();

            /*
             * This is a bit tricky. We're trying to run both a continuous, singleton Web Job (the monitor) and our triggered Web Jobs. We create a new cancellation token; an attempt to cancel
             * the built-in Web Jobs token will trigger ours. A Ctrl+C will also cancel ours. When this runs, the host.StartAsync doesn't block, but jobHost.CallAsync does. So ultimately that's
             * where the cancellation token is likely triggered.
             * 
             * We don't include the token when calling StopAsync because that throws an exception - since by the time we reach that point we've already cancelled.
             */
            var cancellationTokenSource = new CancellationTokenSource();

            var webJobsShutdownWatcher = new WebJobsShutdownWatcher();
            var webJobsCancellationToken = webJobsShutdownWatcher.Token;

            webJobsCancellationToken.Register(() =>
            {
                cancellationTokenSource.Cancel();
            });

            Console.CancelKeyPress += (sender, args) =>
            {
                cancellationTokenSource.Cancel();
            };

            using (host)
            {
                //await host.RunAsync();
                await host.StartAsync(cancellationTokenSource.Token);

                var jobHost = host.Services.GetService<IJobHost>();

                try
                {
                    await jobHost.CallAsync(nameof(SingletonBackgroundServicesHost.Run), cancellationToken: cancellationTokenSource.Token);
                }
                catch (InvalidOperationException)
                {
                    // This exception is called if the cancellation token is invoked?
                }

                await host.StopAsync();
            }
        }
    }
}
