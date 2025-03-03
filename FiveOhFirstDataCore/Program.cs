using FiveOhFirstDataCore.Core.Extensions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

using Newtonsoft.Json;

using NReco.Logging.File;

using System;
using System.IO;
using System.Text;

namespace FiveOhFirstDataCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {                    
                    try
                    {
                        services.Configure<EventLogSettings>(settings =>
                        {
                            settings.SourceName = "501st Data Core";
                        });
                    }
                    catch { /* not on windows */ }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseWindowsService();
    }
}
