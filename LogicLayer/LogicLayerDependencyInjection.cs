using DataAccessLayer;
using DataObjects;
using LogicLayerInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LogicLayer
{
    public static class LogicLayerDependencyInjection
    {
        public static IServiceCollection AddLogicLayer(this IServiceCollection services)
        {
            services.AddScoped<IChatGPTManager, ChatGPTManager>();
            services.AddScoped<IGuildMessageManager, GuildMessageManager>();
            services.AddScoped<IEventManager, EventManager>();
            services.AddScoped<ISlashCommandRegistrar, SlashCommandRegistrar>();
            services.AddScoped<ISlashCommandManager, SlashCommandManager>();
            services.AddScoped<ISlashCommands, SlashCommands>();
            services.AddScoped<IButtonManager, ButtonManager>();
            services.AddScoped<IButtons, Buttons>();
            services.AddScoped<IBactaConfigurationManager, BactaConfigurationManager>();
            services.AddScoped<IGuildManager, GuildManager>();
            services.AddScoped<IChannelManager, ChannelManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IBactaManager, BactaManager>();

            // Register dependencies from the DataAccessLayer
            services.AddDataAccessLayer();

            return services;
        }
    }
}
