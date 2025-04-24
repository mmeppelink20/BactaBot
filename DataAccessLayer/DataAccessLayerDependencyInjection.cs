using DataAccessLayerInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccessLayer
{
    public static class DataAccessLayerDependencyInjection
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
        {
            services.AddScoped<IConfigurationAccessor, ConfigurationAccessor>();
            services.AddScoped<IChannelAccessor, ChannelAccessor>();
            services.AddScoped<IGuildAccessor, GuildAccessor>();
            services.AddScoped<IUserAccessor, UserAccessor>();
            services.AddScoped<IDiscordMessageAccessor, DiscordMessageAccessor>();
            services.AddScoped<IDBConnection, DBConnection>();

            return services;
        }
    }
}
