using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

        private readonly Dictionary<string, Func<SocketMessageComponent, Task>> _buttonList;

        public ButtonManager(ILogger<IButtonManager> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IGuildMessageManager messageManager, IButtons buttons)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _commands = commands;
            _messageManager = messageManager;
            _buttons = buttons;

            _buttonList = new Dictionary<string, Func<SocketMessageComponent, Task>>
            {
                { "btnDm", component => _buttons.BtnDm(component, "DefaultMessage") },
                { "btnShare", component => _buttons.BtnShare(component, "DefaultMessage") }
            };

        }

        public Task ButtonExecutorAsync(SocketMessageComponent component)
        {
            if (_buttonList.TryGetValue(component.Data.CustomId, out Func<SocketMessageComponent, Task>? value))
            {
                return value(component);
            }
            else
            {
                _logger.LogError("Unknown button clicked");
                return Task.CompletedTask;
            }
        }
    }
}
