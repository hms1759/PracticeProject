

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace practiceAuthTable.Core.Extensions
{
    public static class EnvironmentExtensions
    {
        const string UATEnvironment = "UAT";
        const string QAEnvironment = "QA";

        public static bool IsUAT(this IHostEnvironment env)
        {
            return env.IsEnvironment(UATEnvironment);
        }

        public static bool IsQA(this IHostEnvironment env)
        {
            return env.IsEnvironment(QAEnvironment);
        }

        /// <summary>
        /// This method helps to prevent unnecessary conflicts with local appsettings.development.json.
        /// to use this extension, you must  declare your ASPNET_SUBENVIRONMENT in your launch.json or System Env settings. I.e ASPNET_SUBENVIRONMENT="GB"
        /// </summary>
        /// <param name="env"></param>
        /// <returns>IWebHostBuilder</returns>
        public static IWebHostBuilder ConfigureSubEnvironment(this IWebHostBuilder env) => env.ConfigureAppConfiguration((context, builder) =>
        {
            if (context.HostingEnvironment.IsDevelopment())
            {
                string subenv = Environment.GetEnvironmentVariable("ASPNET_SUBENVIRONMENT");
                subenv ??= context.Configuration["ASPNET_SUBENVIRONMENT"];

                if (!string.IsNullOrEmpty(subenv))
                {
                    string subEnvironmentJsonRootFilePath = Path.Combine(context.HostingEnvironment.ContentRootPath, $"appsettings.Development.{subenv}.json");

                    if (File.Exists(subEnvironmentJsonRootFilePath))
                     builder.AddJsonFile($"appsettings.Development.{subenv}.json", optional: true, reloadOnChange: true);
                }
            }
        });
    }

}
