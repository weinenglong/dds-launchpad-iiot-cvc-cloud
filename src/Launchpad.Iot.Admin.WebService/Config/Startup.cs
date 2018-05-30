﻿// ------------------------------------------------------------
//  Copyright (c) Dover Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Launchpad.Iot.Admin.WebService
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using global::Iot.Common;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            ServiceEventSource.Current.Message($"Launchpad Admin WebService  - Startup");
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
                this.Configuration = builder.Build();


            string message = "Configuration=[";

            foreach (var section in this.Configuration.GetChildren())
            {
                ManageAppSettings.AddUpdateAppSettings(section.Key, section.Value);

                string value = section.Value;

                if (section.Key.ToLower().Contains("password"))
                {
                    value = "****************";
                }

                message += "Key=" + section.Key + " Path=" + section.Path + " Value=" + value + " To String=" + section.ToString() + "\n";
            }
            ServiceEventSource.Current.Message("On Launchpad Admin Web Sevice " + message + "]");

        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ServiceEventSource.Current.Message($"Launchpad Admin WebService  - Startup - Configure Services");
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ServiceEventSource.Current.Message($"Launchpad Admin WebService  - Startup - Configure");
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }

            app.UseExceptionHandler(
                errorApp =>
                    errorApp.Run(
                        context =>
                        {
                            context.Response.StatusCode = 500;
                            context.Response.ContentType = "text/plain";

                            IExceptionHandlerFeature feature = context.Features.Get<IExceptionHandlerFeature>();
                            if (feature != null)
                            {
                                Exception ex = feature.Error;

                                return context.Response.WriteAsync(ex.Message);
                            }

                            return Task.FromResult(true);
                        }));

            app.UseStaticFiles();

            app.UseMvc(
                routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });
        }
    }
}