using DataObjects;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using LogicLayerInterfaces.CommandHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer
{
    public class ButtonManager : IButtonManager
    {
        private readonly ILogger<IButtonManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IGuildMessageManager _messageManager;
        private readonly IButtons _buttons;
        private readonly IConfigCommandHandler _configCommandHandler;

        private readonly Dictionary<string, Func<SocketMessageComponent, Task>> _buttonList;

        public ButtonManager(
            ILogger<IButtonManager> logger,
            IConfiguration configuration,
            DiscordSocketClient client,
            CommandService commands,
            IGuildMessageManager messageManager,
            IButtons buttons,
            IConfigCommandHandler configCommandHandler)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _commands = commands;
            _messageManager = messageManager;
            _buttons = buttons;
            _configCommandHandler = configCommandHandler;

            // Map button IDs to handlers  
            _buttonList = new Dictionary<string, Func<SocketMessageComponent, Task>>
               {
                   { ButtonId.btnDm.ToString(), component => _buttons.BtnDm(component, null) },
                   { ButtonId.btnShare.ToString(), component => _buttons.BtnShare(component, null) },
                   { ButtonId.btnRespin.ToString(), component => _buttons.BtnRespin(component) },
                   { ButtonId.btnConfigListPrevious.ToString(), component => _configCommandHandler.HandleConfigListPaginationAsync(component, false) },
                   { ButtonId.btnConfigListNext.ToString(), component => _configCommandHandler.HandleConfigListPaginationAsync(component, true) }
               };
        }

        public Task ButtonExecutorAsync(SocketMessageComponent component)
        {
            if (_buttonList.TryGetValue(component.Data.CustomId, out var handler))
            {
                return handler(component);
            }

            _logger.LogWarning("Unknown button clicked: {ButtonId}", component.Data.CustomId);
            return component.RespondAsync("That button doesn't do anything.", ephemeral: true);
        }
    }
}
