using DbUp;
using DbUp.Engine.Output;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using practiceAuthTable.Core.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace practiceAuthTable
{
    public class Startup
    {

        private ILogger<Startup> _logger;
        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public static IConfiguration Configuration { get; set; }
        private static IHostEnvironment HostingEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "practiceAuthTable", Version = "v1" });
            });
            services.AddDbContext<ModelDbContext>(options =>
            {
                var connectionstring = Configuration.GetConnectionString("Default");
                options
                //.UseLazyLoadingProxies()
                .UseSqlServer(connectionstring, b =>
                {
                    b.MigrationsAssembly("practiceAuthTable.Core");
                    b.EnableRetryOnFailure();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "practiceAuthTable v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            TableMigrationScript();
            StoredProcedureMigrationScript();
        }

        public void TableMigrationScript()
        {
            string dbConnStr = Configuration.GetConnectionString("Default");
            EnsureDatabase.For.SqlDatabase(dbConnStr);

            var upgrader = DeployChanges.To.SqlDatabase(dbConnStr)
            .WithScriptsFromFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql", "Tables"))
            .WithTransactionPerScript()
            .JournalToSqlTable("dbo", "TableMigration")
             .LogToConsole()
            .LogTo(new SerilogDbUpLog(_logger))
            .WithVariablesDisabled()
            .Build();

            upgrader.PerformUpgrade();
        }

        public void StoredProcedureMigrationScript()
        {
            string dbConnStr = Configuration.GetConnectionString("Default");
            EnsureDatabase.For.SqlDatabase(dbConnStr);

            var upgrader = DeployChanges.To.SqlDatabase(dbConnStr)
            .WithScriptsFromFileSystem(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sql", "Sprocs"))
            .WithTransactionPerScript()
            .JournalToSqlTable("dbo", "SprocsMigration")
             .LogTo(new SerilogDbUpLog(_logger))
            .LogToConsole()
            .Build();

            upgrader.PerformUpgrade();
        }
    }

    public class SerilogDbUpLog : IUpgradeLog
    {
        private readonly ILogger<Startup> _logger;

        public SerilogDbUpLog(ILogger<Startup> logger)
        {
            _logger = logger;
        }

        public void WriteError(string format, params object[] args)
        {
            Log.Error(format, args);
        }

        public void WriteInformation(string format, params object[] args)
        {
            Log.Information(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }
    }
}
