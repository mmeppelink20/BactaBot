using DataObjects;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayer;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System.Reflection;


namespace BactaBot
{
    public class BactaBot
    {
        private static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {

            var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetExecutingAssembly())
                    .Build();

            // Configure NLog
            var nLogger = LogManager.Setup()
                .LoadConfigurationFromFile("Properties\\nlog.config");

            var serviceProvider = new ServiceCollection()
                .AddLogging(options =>
                {
                    options.ClearProviders();
                    options.AddNLog();
                    options.AddConsole();
                    options.SetMinimumLevel(configuration.GetSection("Logging").GetValue<Microsoft.Extensions.Logging.LogLevel>("Default"));
                })
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<DiscordSocketClient>(provider =>
                {
                    return new DiscordSocketClient(new DiscordSocketConfig
                    {
                        MessageCacheSize = 5000,
                        GatewayIntents =
                            GatewayIntents.AllUnprivileged
                            | GatewayIntents.MessageContent
                            | GatewayIntents.GuildMembers
                            | GatewayIntents.Guilds
                            | GatewayIntents.GuildMessages
                            | GatewayIntents.GuildPresences
                            | GatewayIntents.All

                    });
                })
                .AddSingleton<CommandService>()
                .AddSingleton<IBot, Bot>()
                .AddLogicLayer()       // Registers Logic Layer dependencies
                .BuildServiceProvider();

            try
            {
                IBot bot = serviceProvider.GetRequiredService<IBot>();
                var slashCommandRegistrar = serviceProvider.GetRequiredService<ISlashCommandRegistrar>();

                await bot.StartAsync(serviceProvider);

                // Register slash commands
                await slashCommandRegistrar.RegisterCommandsAsync();

                // wait forever
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                var logger = serviceProvider.GetRequiredService<ILogger<BactaBot>>();
                logger.LogError((int)BactaLogging.LogEvent.StartUpShutDown, ex, "An error occurred while starting the bot.");

                Environment.Exit(-1);
            }
        }
    }
}