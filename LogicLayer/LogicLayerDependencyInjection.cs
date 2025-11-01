using DataAccessLayer;
using DataObjects;
using LogicLayerInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Discord.WebSocket;
using LogicLayerInterfaces.CommandHandlers;
using LogicLayer.CommandHandlers;
using LogicLayer.Helpers;
using LogicLayerInterfaces.Helpers;

namespace LogicLayer
{
    public static class LogicLayerDependencyInjection
    {
        public static IServiceCollection AddLogicLayer(this IServiceCollection services)
        {
            services.AddScoped<IChatGPTManager, ChatGPTManager>();
            services.AddSingleton<DiscordSocketClient>();
            services.AddScoped<IGuildMessageManager, GuildMessageManager>();
            services.AddScoped<IEventManager, EventManager>();
            services.AddScoped<ISlashCommandRegistrar, SlashCommandRegistrar>();
            services.AddScoped<ISlashCommandManager, SlashCommandManager>();
            services.AddScoped<ISlashCommands, SlashCommands>();
            services.AddScoped<IButtonManager, ButtonManager>();
            services.AddScoped<IButtons, Buttons>();
            services.AddScoped<IGuildManager, GuildManager>();
            services.AddScoped<IChannelManager, ChannelManager>();
            services.AddScoped<IUserManager, UserManager>();
            services.AddScoped<IBactaManager, BactaManager>();
            services.AddScoped<IConfigCommandHandler, ConfigCommandHandler>();
            services.AddScoped<IDeveloperCommandHandler, DeveloperCommandHandler>();
            services.AddScoped<ICommandPermissionHelper, CommandPermissionHelper>();

            // Add encryption manager
            services.AddSingleton<IEncryptionManager, EncryptionManager>();
            
            // Register secure configuration manager instead of basic one
            services.AddScoped<IBactaConfigurationManager, SecureConfigurationManager>();

            // Register dependencies from the DataAccessLayer
            services.AddDataAccessLayer();

            return services;
        }
    }
}
