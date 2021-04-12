using System;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyUniversity.Gateway.Api.MapperProfiles;
using MyUniversity.Gateway.Api.Settings;

namespace MyUniversity.Gateway.Api.Extensions
{
    public static class HostExtensions
    {
        public static void AddCustomConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<UserManagerSettings>(configuration.GetSection(nameof(UserManagerSettings)));
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        }

        public static void AddCustomServices(this IServiceCollection services)
        {
            var userManagerSettings = services.BuildServiceProvider().GetService<IOptions<UserManagerSettings>>().Value;

            services.AddGrpcClient<User.User.UserClient>(o =>
            {
                o.Address = new Uri($"{userManagerSettings.Host}:{userManagerSettings.Port}");
            });
            services.AddGrpcClient<Role.Role.RoleClient>(o =>
            {
                o.Address = new Uri($"{userManagerSettings.Host}:{userManagerSettings.Port}");
            });
            services.AddGrpcClient<University.University.UniversityClient>(o =>
            {
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
