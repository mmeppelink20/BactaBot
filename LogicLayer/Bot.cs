﻿using DataObjects;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LogicLayer
{
    public class Bot(ILogger<IBot> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IEventManager eventHandler, IBactaConfigurationManager bactaConfigurationManager, IGuildManager guildManager, IChannelManager channelManager, IUserManager userManager) : IBot
    {
        private ServiceProvider? _serviceProvider;

        private readonly ILogger<IBot> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly DiscordSocketClient _client = client;
        private readonly CommandService _commands = commands;
        private readonly IEventManager _eventHandler = eventHandler;
        private readonly IBactaConfigurationManager _bactaConfigurationManager = bactaConfigurationManager;
        private readonly IGuildManager _guildManager = guildManager;
        private readonly IChannelManager _channelManager = channelManager;
        private readonly IUserManager _userManager = userManager;

        public async Task StartAsync(ServiceProvider services)
        {
            string discordToken = _configuration["profiles:BactaBot:environmentVariables:DISCORD_TOKEN"] ?? throw new Exception("Missing Discord token");

            _bactaConfigurationManager.RegisterConfiguration();

            _serviceProvider = services;

            // Register the command modules
            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            _bactaConfigurationManager.RegisterConfiguration();

            int retryCount = int.Parse(_configuration["AUTHENTICATION_RETRY_COUNT"] ?? "15");

            // Wait for the client to connect
            for (int i = 1; i <= retryCount; i++)
            {
                if (_client.ConnectionState == ConnectionState.Connected)
                {
                    break;
                }
                // log that it's retrying
                _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Retrying connection to Discord... [{RetryCount}/{MaxRetryCount}]", i, retryCount);
                await Task.Delay(1000);
            }

            if (_client.ConnectionState != ConnectionState.Connected)
            {
                // determine whether it's an internect connection issue, or an invalid api token
                if (_client.ConnectionState == ConnectionState.Disconnected)
                {
                    throw new Exception("Failed to connect to Discord. Check your internet connection.");
                }
                else
                {
                    throw new InvalidOperationException("Failed to connect to Discord. Check your API token.");
                }
            }

            // Set the client's activity
            await _client.SetActivityAsync(new Game("in the Bacta Pod..."));

            // Register event handlers
            _client.MessageReceived += _eventHandler.MessageRecieved;
            _client.SlashCommandExecuted += _eventHandler.SlashCommandExecuted;
            _client.MessageDeleted += _eventHandler.MessageDeleted;
            _client.ButtonExecuted += _eventHandler.ButtonExecuted;
            _eventHandler.ShutdownRequested += StopAsync;

            _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown,
        @$"Starting Bacta Bot
-----------------------------------------------------------
-----------------------------------------------------------
----|    ____             _          ____        _     |---
----|   |  _ \           | |        |  _ \      | |    |---
----|   | |_) | __ _  ___| |_ __ _  | |_) | ___ | |_   |---
----|   |  _ < / _` |/ __| __/ _` | |  _ < / _ \| __|  |---
----|   | |_) | (_| | (__| || (_| | | |_) | (_) | |_   |---
----|   |____/ \__,_|\___|\__\__,_| |____/ \___/ \__|  |---
-----------------------------------------------------------
-----------------------------------------------------------
"
            );

            await _guildManager.RegisterGuildsAsync();

            await _channelManager.RegisterChannelsAsync();

            await _userManager.RegisterUsersAsync();

            await _userManager.RegisterGuildUsersAsync();

        }
        public async Task StopAsync()
        {
            _logger.LogInformation((int)BactaLogging.LogEvent.StartUpShutDown, "Shutting down");

            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
                // close program
                // Environment.Exit(0);
            }
        }

    }
}