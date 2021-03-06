﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using SFA.DAS.EmployerFinance.Startup;
using SFA.DAS.EmployerFinance.Web.Startup;
using StructureMap.AspNetCore;

namespace SFA.DAS.EmployerFinance.Web
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) => 
            WebHost.CreateDefaultBuilder(args)
                .ConfigureDasAppConfiguration()
                .ConfigureDasLogging()
                .UseApplicationInsights()
                .UseDasEnvironment()
                .UseKestrel(o => o.AddServerHeader = false)
                .UseStructureMap()
                .UseStartup<AspNetStartup>();
    }
}