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
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets(Assembly.GetExecutingAssembly())
                    .Build();

            // Configure services
            var serviceProvider = new ServiceCollection()
                .AddLogging(options =>
                {
                    options.ClearProviders();
                    options.AddNLog();
                    options.AddConsole();
                    options.SetMinimumLevel(configuration.GetSection("Logging").GetValue<Microsoft.Extensions.Logging.LogLevel>(ConfigurationKeys.LoggingDefault));
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
                .AddSingleton<ConfigurationSeeder>()
                .AddLogicLayer()       // Registers Logic Layer dependencies
                .BuildServiceProvider();

            try
            {
                var logger = serviceProvider.GetRequiredService<ILogger<BactaBot>>();
                
                // Seed all required configuration
                var seeder = serviceProvider.GetRequiredService<ConfigurationSeeder>();
                
                // Show diagnostic information
                seeder.DiagnoseConfiguration();
                
                // Seed missing configuration values
                var seedSuccess = await seeder.SeedRequiredConfigurationAsync();
                
                if (!seedSuccess)
                {
                    logger.LogError("Failed to seed required configuration. Cannot start bot.");
                    logger.LogError("Please ensure all required keys are set in user secrets or environment variables.");
                    Environment.Exit(-1);
                    return;
                }

                // Load database configuration (includes seeded values)
                var bactaConfigManager = serviceProvider.GetRequiredService<IBactaConfigurationManager>();
                await bactaConfigManager.RegisterConfigurationAsync();

                // Validate configuration before starting
                var validationResult = ConfigurationValidator.ValidateConfiguration(configuration, logger);
                if (!validationResult.IsSuccess)
                {
                    logger.LogError("Configuration validation failed: {ErrorMessage}", validationResult.ErrorMessage);
                    logger.LogError("Please ensure all required configuration keys are set correctly.");
                    Environment.Exit(-1);
                    return;
                }

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