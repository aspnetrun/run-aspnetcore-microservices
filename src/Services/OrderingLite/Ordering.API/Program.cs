using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Ordering.API.Extensions;
using Ordering.Infrastructure.Persistence;
using Serilog;
using Common.Logging;

namespace Ordering.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                .MigrateDatabase<OrderContext>((context, services) =>
                    {
                        var logger = services.GetService<ILogger<OrderContextSeed>>();
                        OrderContextSeed
                            .SeedAsync(context, logger)
                            .Wait();
                    })
                .Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(SeriLogger.Configure)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
