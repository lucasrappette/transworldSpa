using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpaFramework.Web;
using SpaFramework.Web.Hubs;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using Serilog.Filters;
using Serilog.Formatting.Compact;
using Serilog.Sinks.AspNetCore.SignalR.Extensions;

namespace SpaFramework.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Logger(l => l
                    .MinimumLevel.Information()
                    .WriteTo.File("Logs\\Startup.txt")
                    .WriteTo.Console()
                )
                .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, service, loggerConfig) =>
                {
                    loggerConfig
                        .Enrich.FromLogContext()
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Information()
                            .Filter.ByIncludingOnly(Matching.FromSource("SpaFramework"))
                            .WriteTo.Console()
                            .WriteTo.ApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces)
                        )
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Information()
                            .Filter.ByIncludingOnly(Matching.FromSource("Microsoft.AspNetCore.Hosting"))
                            .WriteTo.Console()
                        )
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Warning()
                            .Filter.ByIncludingOnly(Matching.FromSource("Microsoft.AspNetCore.Hosting"))
                            .WriteTo.ApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces)
                        )
                        .WriteTo.Logger(l => l
                            .MinimumLevel.Warning()
                            .WriteTo.ApplicationInsights(hostingContext.Configuration["ApplicationInsights:InstrumentationKey"], TelemetryConverter.Traces)
                        );
                })
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
