﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmployerFinance.Web.DependencyResolution;
using SFA.DAS.EmployerFinance.Web.Filters;
using SFA.DAS.EmployerFinance.Web.Urls;
using StructureMap;

namespace SFA.DAS.EmployerFinance.Web
{
    //todo: plug in authentication using https://github.com/dotnet/core-setup/blob/master/Documentation/design-docs/host-startup-hook.md
    // ^^ hopefully should allow very easy plugin in of generic oidc support!
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // options: https://stackoverflow.com/questions/32459670/resolving-instances-with-asp-net-core-di
            var serviceProvider = services.BuildServiceProvider();
            
            services.AddMvc(options =>
                {
//                    options.Filters.Add(new UrlsViewBagFilter(() => serviceProvider.GetService<IEmployerUrls>()));
                    options.Filters.Add(new UrlsViewBagFilter(serviceProvider.GetService<IContainer>()));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }
        
        public void ConfigureContainer(Registry registry)
        {
            IoC.Initialize(registry);
        }
    }
}