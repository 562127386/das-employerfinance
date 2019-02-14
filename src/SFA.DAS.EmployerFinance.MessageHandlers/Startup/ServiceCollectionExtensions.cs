using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.EmployerFinance.Configuration;
using SFA.DAS.EmployerFinance.NServiceBus;
using SFA.DAS.EmployerFinance.Startup;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.NServiceBus.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus;
using StructureMap;

namespace SFA.DAS.EmployerFinance.MessageHandlers.Startup
{
    public static class ServiceCollectionExtensions
    {
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
                
                    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.EmployerFinanceV2.MessageHandlers")
                        .UseAzureServiceBusTransport(() => configurationSection.ServiceBusConnectionString, isDevelopment)
                        .UseInstallers()
                        .UseLicense(configurationSection.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseInstallers()
                        .UseStructureMapBuilder(container)
                        .UseUnitOfWork();
                    
                    endpointConfiguration.EnableCallbacks(makesRequests: false);
                    
                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
                    
                    return endpoint;
                })
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}