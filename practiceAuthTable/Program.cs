using DbUp;
using DbUp.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using practiceAuthTable.Core.Extensions;
using practiceAuthTable.Core.Models;
using practiceAuthTable.DI;
using Sentry;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace practiceAuthTable
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(@"logs\log.txt", rollingInterval: RollingInterval.Day,
                 restrictedToMinimumLevel: LogEventLevel.Information,
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        shared: true)
                .CreateLogger();
            var host = CreateHostBuilder(args).Build();
            var serviceProvider = host.Services;

            SeedDatabase(serviceProvider).Wait();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((context, builder) =>
                    {
                        builder.AddJsonFile($"appsettings.json", optional: true);
                        builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                        builder.AddEnvironmentVariables();
                    })

                    .UseStartup<Startup>();
                });

        public static async Task SeedDatabase(IServiceProvider _serviceProvider)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ModelDbContext>();
            await context.Database.EnsureCreatedAsync();

            TableMigrationScript(scope);
            StoredProcedureMigrationScript(scope);

         
        }

        public static void TableMigrationScript(IServiceScope scope)
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            string dbConnStr = configuration.GetConnectionString("Default");
            EnsureDatabase.For.SqlDatabase(dbConnStr);

            var _logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();

            var upgrader = DeployChanges.To.SqlDatabase(dbConnStr)
            .WithScriptsFromFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql", "Tables"))
            .WithTransactionPerScript()
            .JournalToSqlTable("dbo", "TableMigration")
            .LogTo(new SerilogDbUpLog(_logger))
            .LogToConsole()
            .Build();

            upgrader.PerformUpgrade();
        }
        public static void StoredProcedureMigrationScript(IServiceScope scope)
        {
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            string dbConnStr = configuration.GetConnectionString("Default");
            EnsureDatabase.For.SqlDatabase(dbConnStr);

            var _logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
            var upgrader = DeployChanges.To.SqlDatabase(dbConnStr)
            .WithScriptsFromFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql", "Sprocs"))
            .WithTransactionPerScript()
            .JournalTo(new NullJournal())
            .LogTo(new SerilogDbUpLog(_logger))
            .LogToConsole()
            .Build();

            upgrader.PerformUpgrade();
        }
    }

}