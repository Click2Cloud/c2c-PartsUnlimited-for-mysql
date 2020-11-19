//// Copyright (c) Microsoft. All rights reserved.
//// Licensed under the MIT license. See LICENSE file in the project root for full license information.

//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using PartsUnlimited.Areas.Admin;
//using PartsUnlimited.Models;
//using PartsUnlimited.Queries;
//using PartsUnlimited.Recommendations;
//using PartsUnlimited.Search;
//using PartsUnlimited.Security;
//using PartsUnlimited.Telemetry;
//using PartsUnlimited.WebsiteConfiguration;
//using System;
//using System.Linq;

//namespace PartsUnlimited
//{
//    public class Startup
//    {
//        public IConfiguration Configuration { get; }
//        public IServiceCollection service { get; private set; }

//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public void ConfigureServices(IServiceCollection services)
//        {
//            service = services;

//            //If this type is present - we're on mono
//            var runningOnMono = Type.GetType("Mono.Runtime") != null;
//            var sqlConnectionString = Configuration.GetValue<string>("ConnectionStrings:DefaultConnectionString");
//            var useInMemoryDatabase = string.IsNullOrWhiteSpace(sqlConnectionString);

//            if (useInMemoryDatabase || runningOnMono)
//            {
//                sqlConnectionString = "";
//            }

//            // Add EF services to the services container
//            services.AddDbContext<PartsUnlimitedContext>();


//            // Add Identity services to the services container
//            services.AddIdentity<ApplicationUser, IdentityRole>()
//                .AddEntityFrameworkStores<PartsUnlimitedContext>()
//                .AddDefaultTokenProviders();

//            // Configure admin policies
//            services.AddAuthorization(auth =>
//            {
//                auth.AddPolicy(AdminConstants.Role,
//                    authBuilder =>
//                    {
//                        authBuilder.RequireClaim(AdminConstants.ManageStore.Name, AdminConstants.ManageStore.Allowed);
//                    });

//            });

//            // Add implementations
//            services.AddSingleton<IMemoryCache, MemoryCache>();
//            services.AddScoped<IOrdersQuery, OrdersQuery>();
//            services.AddScoped<IRaincheckQuery, RaincheckQuery>();

//            services.AddSingleton<ITelemetryProvider, EmptyTelemetryProvider>();
//            services.AddScoped<IProductSearch, StringContainsProductSearch>();

//            SetupRecommendationService(services);

//            services.AddScoped<IWebsiteOptions>(p =>
//            {
//                var telemetry = p.GetRequiredService<ITelemetryProvider>();

//                return new ConfigurationWebsiteOptions(Configuration.GetSection("WebsiteOptions"), telemetry);
//            });

//            services.AddScoped<IApplicationInsightsSettings>(p =>
//            {
//                return new ConfigurationApplicationInsightsSettings(Configuration.GetSection(ConfigurationPath.Combine("Keys", "ApplicationInsights")));
//            });

//            services.AddApplicationInsightsTelemetry(Configuration);

//            // Associate IPartsUnlimitedContext and PartsUnlimitedContext with context
//            services.AddTransient<IPartsUnlimitedContext>(x => new PartsUnlimitedContext(sqlConnectionString));
//            services.AddTransient(x => new PartsUnlimitedContext(sqlConnectionString));

//            // We need access to these settings in a static extension method, so DI does not help us :(
//            ContentDeliveryNetworkExtensions.Configuration = new ContentDeliveryNetworkConfiguration(Configuration.GetSection("CDN"));

//            // Add MVC services to the services container
//            services.AddMvc();

//            //Add InMemoryCache
//            services.AddSingleton<IMemoryCache, MemoryCache>();

//            // Add session related services.
//            //services.AddCaching();
//            services.AddSession();
//            services.AddSession(options =>
//            {
//                options.Cookie.SameSite = SameSiteMode.None;
//                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//                options.Cookie.IsEssential = true;
//            });

//            services.Configure<CookiePolicyOptions>(options =>
//            {
//                options.MinimumSameSitePolicy = SameSiteMode.None;
//                options.Secure = CookieSecurePolicy.Always;
//            });

//            services
//            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//            .AddCookie(options =>
//            {
//                options.Cookie.SameSite = SameSiteMode.None;
//                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//                options.Cookie.IsEssential = true;
//            });

//            services.AddSession(options =>
//            {
//                options.Cookie.SameSite = SameSiteMode.None;
//                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//                options.Cookie.IsEssential = true;
//            });

//            services.AddAntiforgery(o =>
//            {
//                o.SuppressXFrameOptionsHeader = true;
//                o.Cookie.SameSite = SameSiteMode.None;
//            });

//            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
//            {
//                builder
//                 .AllowAnyOrigin()
//                 .AllowAnyMethod()
//                 .AllowAnyHeader()
//                 .AllowCredentials();
//            }));
//        }

//        private void SetupRecommendationService(IServiceCollection services)
//        {
//            var azureMlConfig = new AzureMLFrequentlyBoughtTogetherConfig(Configuration.GetSection(ConfigurationPath.Combine("Keys", "AzureMLFrequentlyBoughtTogether")));

//            // If keys are not available for Azure ML recommendation service, register an empty recommendation engine
//            if (string.IsNullOrEmpty(azureMlConfig.AccountKey) || string.IsNullOrEmpty(azureMlConfig.ModelName))
//            {
//                services.AddSingleton<IRecommendationEngine, EmptyRecommendationsEngine>();
//            }
//            else
//            {
//                services.AddSingleton<IAzureMLAuthenticatedHttpClient, AzureMLAuthenticatedHttpClient>();
//                services.AddSingleton<IAzureMLFrequentlyBoughtTogetherConfig>(azureMlConfig);
//                services.AddScoped<IRecommendationEngine, AzureMLFrequentlyBoughtTogetherRecommendationEngine>();
//            }
//        }

//        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
//        //The allowed values are Development,Staging and Production
//        public void ConfigureDevelopment(IApplicationBuilder app)
//        {
//            //Display custom error page in production when error occurs
//            //During development use the ErrorPage middleware to display error information in the browser
//            app.UseDeveloperExceptionPage();
//            app.UseDatabaseErrorPage();

//            Configure(app);
//        }

//        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Staging'
//        //The allowed values are Development,Staging and Production
//        public void ConfigureStaging(IApplicationBuilder app)
//        {
//            app.UseExceptionHandler("/Home/Error");
//            Configure(app);
//        }

//        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Production'
//        //The allowed values are Development,Staging and Production
//        public void ConfigureProduction(IApplicationBuilder app)
//        {
//            app.UseExceptionHandler("/Home/Error");
//            Configure(app);
//        }

//        public void Configure(IApplicationBuilder app)
//        {
//            // Configure Session.
//            app.UseSession();

//            // Add static files to the request pipeline
//            app.UseStaticFiles();

//            app.UseCors("AllowAll");

//            app.UseAuthentication();

//            // Add cookie-based authentication to the request pipeline
//            app.UseCookiePolicy();

//            app.Use(async (ctx, next) =>
//            {
//                var schemes = ctx.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
//                var handlers = ctx.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
//                foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
//                {
//                    var handler = await handlers.GetHandlerAsync(ctx, scheme.Name) as IAuthenticationRequestHandler;
//                    if (handler != null && await handler.HandleRequestAsync())
//                    {
//                        // start same-site cookie special handling
//                        string location = null;
//                        if (ctx.Response.StatusCode == 302)
//                        {
//                            location = ctx.Response.Headers["location"];
//                        }
//                        else if (ctx.Request.Method == "GET" && !ctx.Request.Query["skip"].Any())
//                        {
//                            location = ctx.Request.Path + ctx.Request.QueryString + "&skip=1";
//                        }

//                        if (location != null)
//                        {
//                            ctx.Response.StatusCode = 200;
//                            var html = $@"
//                                    <html><head>
//                                        <meta http-equiv='refresh' content='0;url={location}' />
//                                    </head></html>";
//                            await ctx.Response.WriteAsync(html);
//                        }
//                        // end same-site cookie special handling

//                        return;
//                    }
//                }

//                await next();
//            });

//            AppBuilderLoginProviderExtensions.AddLoginProviders(service, new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));
//            // Add login providers (Microsoft/AzureAD/Google/etc).  This must be done after `app.UseIdentity()`
//            //app.AddLoginProviders( new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));

//            // Add MVC to the request pipeline
//            app.UseMvc(routes =>
//            {
//                routes.MapRoute(
//                    name: "areaRoute",
//                    template: "{area:exists}/{controller}/{action}",
//                    defaults: new { action = "Index" });

//                routes.MapRoute(
//                    name: "default",
//                    template: "{controller}/{action}/{id?}",
//                    defaults: new { controller = "Home", action = "Index" });

//                routes.MapRoute(
//                    name: "api",
//                    template: "{controller}/{id?}");
//            });
//        }

//    }
//}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PartsUnlimited.Areas.Admin;
using PartsUnlimited.Models;
using PartsUnlimited.Queries;
using PartsUnlimited.Recommendations;
using PartsUnlimited.Search;
using PartsUnlimited.Security;
using PartsUnlimited.Telemetry;
using PartsUnlimited.WebsiteConfiguration;
using Microsoft.Extensions.Hosting;
using System;
using Stripe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PartsUnlimited
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IServiceCollection service { get; private set;}

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

        }

        public void ConfigureServices(IServiceCollection services)
        {
            service = services;
            //If this type is present - we're on mono
            var runningOnMono = Type.GetType("Mono.Runtime") != null;
            var sqlConnectionString = Configuration[ConfigurationPath.Combine("ConnectionStrings", "DefaultConnectionString")];
            var useInMemoryDatabase = string.IsNullOrWhiteSpace(sqlConnectionString);


            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //if (useInMemoryDatabase || runningOnMono)
            //{
            //    sqlConnectionString = "";
            //}

            // Add EF services to the services container
            services.AddDbContext<PartsUnlimitedContext>();


            // Add Identity services to the services container
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<PartsUnlimitedContext>()
                .AddDefaultTokenProviders();

            // Configure admin policies
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(AdminConstants.Role,
                    authBuilder =>
                    {
                        authBuilder.RequireClaim(AdminConstants.ManageStore.Name, AdminConstants.ManageStore.Allowed);
                    });

            });

            // Add implementations
            //services.AddSingleton<IMemoryCache, MemoryCache>();
            //services.AddScoped<IOrdersQuery, OrdersQuery>();
            //services.AddScoped<IRaincheckQuery, RaincheckQuery>();
            //services.AddSingleton<ITelemetryProvider, EmptyTelemetryProvider>();
            //services.AddScoped<IProductSearch, StringContainsProductSearch>();

            //SetupRecommendationService(services);

            //services.AddScoped<IWebsiteOptions>(p =>
            //{
            //    var telemetry = p.GetRequiredService<ITelemetryProvider>();

            //    return new ConfigurationWebsiteOptions(Configuration.GetSection("WebsiteOptions"), telemetry);
            //});

            services.AddScoped<IApplicationInsightsSettings>(p =>
            {
                return new ConfigurationApplicationInsightsSettings(Configuration.GetSection(ConfigurationPath.Combine("Keys", "ApplicationInsights")));
            });
            services.AddApplicationInsightsTelemetry(Configuration);

            // Associate IPartsUnlimitedContext and PartsUnlimitedContext with context
            services.AddTransient<IPartsUnlimitedContext>(x => new PartsUnlimitedContext(sqlConnectionString));
            services.AddTransient(x => new PartsUnlimitedContext(sqlConnectionString));

            // We need access to these settings in a static extension method, so DI does not help us :(
            //ContentDeliveryNetworkExtensions.Configuration = new ContentDeliveryNetworkConfiguration(Configuration.GetSection("CDN"));
            services.Configure<PaymentSettings>(Configuration.GetSection("Stripe"));
            // Add MVC services to the services container
            services.AddMvc();

            //Add InMemoryCache
            //services.AddSingleton<IMemoryCache, MemoryCache>();

            // Add session related services.
            //services.AddCaching();
            services.AddSession();

        }

        //private void SetupRecommendationService(IServiceCollection services)
        //{
        //    var azureMlConfig = new AzureMLFrequentlyBoughtTogetherConfig(Configuration.GetSection(ConfigurationPath.Combine("Keys", "AzureMLFrequentlyBoughtTogether")));

        //    // If keys are not available for Azure ML recommendation service, register an empty recommendation engine
        //    if (string.IsNullOrEmpty(azureMlConfig.AccountKey) || string.IsNullOrEmpty(azureMlConfig.ModelName))
        //    {
        //        services.AddSingleton<IRecommendationEngine, EmptyRecommendationsEngine>();
        //    }
        //    else
        //    {
        //        services.AddSingleton<IAzureMLAuthenticatedHttpClient, AzureMLAuthenticatedHttpClient>();
        //        services.AddSingleton<IAzureMLFrequentlyBoughtTogetherConfig>(azureMlConfig);
        //        services.AddScoped<IRecommendationEngine, AzureMLFrequentlyBoughtTogetherRecommendationEngine>();
        //    }
        //}

        ////This method is invoked when ASPNETCORE_ENVIRONMENT is 'Development' or is not defined
        ////The allowed values are Development,Staging and Production
        //public void ConfigureDevelopment(IApplicationBuilder app)
        //{
        //    //Display custom error page in production when error occurs
        //    //During development use the ErrorPage middleware to display error information in the browser
        //    app.UseDeveloperExceptionPage();
        //    app.UseDatabaseErrorPage();

        //    Configure(app);
        //}

        //This method is invoked when ASPNETCORE_ENVIRONMENT is 'Staging'
        //The allowed values are Development,Staging and Production
        //public void ConfigureStaging(IApplicationBuilder app)
        //{
        //    app.UseExceptionHandler("/Home/Error");
        //    Configure(app);
        //}

        ////This method is invoked when ASPNETCORE_ENVIRONMENT is 'Production'
        //The allowed values are Development,Staging and Production
        //public void ConfigureProduction(IApplicationBuilder app)
        //{
        //    app.UseExceptionHandler("/Home/Error");
        //    Configure(app);
        //}

        public void Configure(IApplicationBuilder app)
        {
            //var builder = new ConfigurationBuilder()
            //   .AddJsonFile("config.json")
            //   .AddJsonFile($"config.{env.EnvironmentName}.json", optional: true);
            //if (env.IsProduction())
            //{
            //    //app.UseDeveloperExceptionPage();
            //    builder.AddUserSecrets("AdminRole");
            //    builder.AddApplicationInsightsSettings(developerMode: true);
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];
            // Configure Session.
            app.UseSession();
            //app.UseCookiePolicy();
            // Add static files to the request pipeline
            app.UseStaticFiles();

            // Add cookie-based authentication to the request pipeline
            app.UseAuthentication();

            AppBuilderLoginProviderExtensions.AddLoginProviders(service, new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));
            // Add login providers (Microsoft/AzureAD/Google/etc).  This must be done after `app.UseIdentity()`
            //app.AddLoginProviders( new ConfigurationLoginProviders(Configuration.GetSection("Authentication")));




            // Add MVC to the request pipeline
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "api",
                    template: "{controller}/{id?}");
            });
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.HttpsPolicy;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using PartsUnlimited.Models;


//namespace PartsUnlimited
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            var runningOnMono = Type.GetType("Mono.Runtime") != null;
//            var sqlConnectionString = Configuration[ConfigurationPath.Combine("ConnectionStrings", "DefaultConnectionString")];
//            var useInMemoryDatabase = string.IsNullOrWhiteSpace(sqlConnectionString);
//            services.AddDbContext<PartsUnlimitedContext>();
//            services.AddTransient<IPartsUnlimitedContext>(x => new PartsUnlimitedContext(sqlConnectionString));
//            services.AddTransient(x => new PartsUnlimitedContext(sqlConnectionString));

//            services.AddIdentity<ApplicationUser, IdentityRole>()
//             .AddEntityFrameworkStores<PartsUnlimitedContext>()
//             .AddDefaultTokenProviders();
//            services.Configure<CookiePolicyOptions>(options =>
//            {
//                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
//                options.CheckConsentNeeded = context => true;
//                options.MinimumSameSitePolicy = SameSiteMode.None;
//            });




//            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IHostingEnvironment env, PartsUnlimitedContext db)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseExceptionHandler("/Home/Error");
//                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//                app.UseHsts();
//            }
//            //db.Database.EnsureCreated();
//            app.UseHttpsRedirection();
//            app.UseStaticFiles();
//            app.UseCookiePolicy();

//            app.UseMvc(routes =>
//            {
//                routes.MapRoute(
//                    name: "areaRoute",
//                    template: "{area:exists}/{controller}/{action}",
//                    defaults: new { action = "Index" });

//                routes.MapRoute(
//                    name: "default",
//                    template: "{controller}/{action}/{id?}",
//                    defaults: new { controller = "Home", action = "Index" });

//                routes.MapRoute(
//                    name: "api",
//                    template: "{controller}/{id?}");
//            });
//        }
//    }
//}
