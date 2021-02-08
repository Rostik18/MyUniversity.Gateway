using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyUniversity.Gateway.Api.Configs;

namespace MyUniversity.Gateway.Api.Extensions
{
    public static class HostExtensions
    {
        public static void AddCustomConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection(nameof(MessageClientConfigs)).Get<MessageClientConfigs>());
        }

        public static void AddCustomServices(this IServiceCollection serviceCollection)
        {

        }
    }
}
