using StructureMap;

namespace SFA.DAS.EmployerFinance.Api.DependencyResolution
{
    public static class IoC
    {
        public static void Initialize(Registry registry)
        {
            registry.IncludeRegistry<DefaultRegistry>();
        }
    }
}