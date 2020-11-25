// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace PartsUnlimited.Models
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            //Below code demonstrates usage of multiple configuration sources. For instance a setting say 'setting1' is found in both the registered sources, 
            //then the later source will win. By this way a Local config can be overridden by a different setting while deployed remotely.
            var builder = new ConfigurationBuilder()
              .AddJsonFile("config.json")
                .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets("AdminRole");

                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var DBPassword = new object(); var DBName = new object(); var DBSecurityInfo = new object(); var DBServerIP = new object(); var DBUserId = new object();

            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry de in environmentVariables)
            {
                //Console.WriteLine("  {0} = {1}", de.Key, de.Value);
                if (de.Key.ToString() == "WWI_PASSWORD")
                {
                    DBPassword = de.Value;
                }
                if (de.Key.ToString() == "WWI_DB_NAME")
                {
                    DBName = de.Value;
                }
                if (de.Key.ToString() == "WWI_SECURITY_INFO")
                {
                    DBSecurityInfo = de.Value;
                }
                if (de.Key.ToString() == "WWI_SERVER_NAME")
                {
                    DBServerIP = de.Value;
                }
                if (de.Key.ToString() == "WWI_USER_NAME")
                {
                    DBUserId = de.Value;
                }
            }

            var sqlConnectionString = "server=" + DBServerIP + ";User Id=" + DBUserId + ";password=" + DBPassword + ";database=" + DBName + ";persistsecurityinfo=" + DBSecurityInfo + ";";
            //var sqlConnectionString =Configuration[ConfigurationPath.Combine("Data", "DefaultConnection", "ConnectionString")];
            //var sqlConnectionString = Configuration[ConfigurationPath.Combine("Data", "server=" + DBServerIP + ";User Id=" + DBUserId + ";password=" + DBPassword + ";database=" + DBName + ";persistsecurityinfo=" + DBSecurityInfo+";", "ConnectionString")];
            if (!String.IsNullOrEmpty(sqlConnectionString))
            {
                services.AddEntityFrameworkSqlServer()
                      .AddDbContext<PartsUnlimitedContext>(options =>
                      {
                          //options.UseSqlServer(sqlConnectionString);
                          options.UseMySql(sqlConnectionString);
                      });
            }


        }

        //Configure is required by 'ef migrations add' command.
        public void Configure(IApplicationBuilder app)
        {
        }
    }
}
