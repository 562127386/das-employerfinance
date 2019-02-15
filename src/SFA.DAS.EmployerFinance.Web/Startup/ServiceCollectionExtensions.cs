using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.Authorization.Mvc;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.NServiceBus;
using SFA.DAS.EmployerFinance.Startup;
using SFA.DAS.EmployerFinance.Web.Authentication;
using SFA.DAS.EmployerFinance.Web.Filters;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.NServiceBus.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus;
using StructureMap;
using Endpoint = NServiceBus.Endpoint;

namespace SFA.DAS.EmployerFinance.Web.Startup
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDasCookiePolicy(this IServiceCollection services)
        {
            return services.Configure<CookiePolicyOptions>(o =>
            {
                o.CheckConsentNeeded = c => true;
                o.MinimumSameSitePolicy = SameSiteMode.None;
            });
        }
        
        public static IServiceCollection AddDasMvc(this IServiceCollection services)
        {
            services
                .AddMvc(o =>
                {
                    o.AddDasAuthorization();
                    o.Filters.Add<UrlsViewBagFilter>();
                    o.RequireAuthorizationByDefault();
                })
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            return services;
        }

        public static IServiceCollection AddDasNServiceBus(this IServiceCollection services)
        {
            return services
                .AddSingleton(s =>
                {
                    var configuration = s.GetService<IConfiguration>();
                    var container = s.GetService<IContainer>();
                    var hostingEnvironment = s.GetService<IHostingEnvironment>();
                    var configurationSection = configuration.GetEmployerFinanceSection<EmployerFinanceConfiguration>();
                    var isDevelopment = hostingEnvironment.IsDevelopment();
                    
                    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinanceV2.Web")
                        .UseAzureServiceBusTransport(() => configurationSection.ServiceBusConnectionString, isDevelopment)
                        .UseErrorQueue()
                        .UseInstallers()
                        .UseLicense(configurationSection.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseStructureMapBuilder(container)
                        .UseUnitOfWork();
                    
                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
                    
                    return endpoint;
                })
                .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}