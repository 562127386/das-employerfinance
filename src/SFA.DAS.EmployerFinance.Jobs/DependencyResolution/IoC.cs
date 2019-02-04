﻿using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFinance.DependencyResolution;
using StructureMap;

namespace SFA.DAS.EmployerFinance.Jobs.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize(IConfiguration config, string environmentName)
        {
            return new Container(c =>
            {
                c.AddRegistry(new DasNonMvcHostingEnvironmentRegistry(environmentName));
                c.AddRegistry(new ConfigurationRegistry(config));
                c.AddRegistry<DataRegistry>();
                c.AddRegistry<StartupRegistry>();
                c.AddRegistry<DefaultRegistry>();
            });
        }
    }
}
