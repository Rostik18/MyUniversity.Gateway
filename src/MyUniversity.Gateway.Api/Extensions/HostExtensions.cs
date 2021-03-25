using System;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyUniversity.Gateway.Api.MapperProfiles;

namespace MyUniversity.Gateway.Api.Extensions
{
    public static class HostExtensions
    {
        public static void AddCustomConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<UserManagerSettings>(configuration.GetSection(nameof(UserManagerSettings)));
        }

        public static void AddCustomServices(this IServiceCollection services)
        {
            services.AddGrpcClient<User.UserClient>(o =>
            {
                var userManagerSettings = services.BuildServiceProvider().GetService<IOptions<UserManagerSettings>>().Value;
                o.Address = new Uri($"{userManagerSettings.Host}:{userManagerSettings.Port}");
            });
        }

        public static void AddMapper(this IServiceCollection services)
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new ControllerMapperProfile());
                mc.AllowNullCollections = true;
            });

            var mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }
    }
}
