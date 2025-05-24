using Discord;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer
{
    public class Buttons : IButtons
    {
        private readonly ILogger<IButtons> _logger;

        private class ButtonState
        {
            public bool DmDisabled { get; set; }
            public bool ShareDisabled { get; set; }
        }

        // Key = messageId, Value = button states for that message
        private readonly ConcurrentDictionary<ulong, ButtonState> _buttonStates = new();

        public Buttons(ILogger<IButtons> logger)
        {
            _logger = logger;
        }

        public async Task BtnDm(SocketMessageComponent component)
        {
            try
            {
                _logger.LogInformation("DM button clicked");

                var messageId = component.Message.Id;
                var state = _buttonStates.GetOrAdd(messageId, new ButtonState());

                state.DmDisabled = true;

                await UpdateComponentState(component, state);
                await component.User.SendMessageAsync(component.Message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the DM button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        public async Task BtnShare(SocketMessageComponent component)
        {
            try
            {
                _logger.LogInformation("Share button clicked");

                var messageId = component.Message.Id;
                var state = _buttonStates.GetOrAdd(messageId, new ButtonState());

                state.ShareDisabled = true;

                await UpdateComponentState(component, state);
                await component.Channel.SendMessageAsync($"{component.Message.Content} {component.User.Mention}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the Share button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        private async Task UpdateComponentState(SocketMessageComponent component, ButtonState state)
        {
            await component.UpdateAsync(msg =>
            {
                var builder = new ComponentBuilder();
                CreateBtnDM(builder, state.DmDisabled);
                CreateBtnShare(builder, state.ShareDisabled);
                msg.Components = builder.Build();
            });
        }

        public void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false)
        {
            builder.WithButton(ButtonId.btnDm.GetLabel(), ButtonId.btnDm.ToString(), ButtonId.btnDm.GetStyle(), disabled: isDisabled);
        }

        public void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false)
        {
            builder.WithButton(ButtonId.btnShare.GetLabel(), ButtonId.btnShare.ToString(), ButtonId.btnShare.GetStyle(), disabled: isDisabled);
        }
    }
}
