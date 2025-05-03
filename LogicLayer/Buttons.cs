using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LogicLayer
{
    public enum ButtonId
    {
        btnDm,
        btnShare
    }
    public class Buttons : IButtons
    {
        private readonly ILogger<IButtons> _logger;

        // Keeps track of the button states (key = button ID, value = disabled state)
        private readonly ConcurrentDictionary<ButtonId, bool> _buttonIsDisabledStates;

        public Buttons(ILogger<IButtons> logger)
        {
            _logger = logger;


            // Initialize button states in the constructor (default states can be customized here)
            _buttonIsDisabledStates = new ConcurrentDictionary<ButtonId, bool>
            {
                [ButtonId.btnDm] = false,   // DM button initially enabled
                [ButtonId.btnShare] = false  // Share button initially enabled
            };
        }
        public async Task BtnDm(SocketMessageComponent component, string message)
        {
            try
            {
                _logger.LogInformation("DM button clicked");

                // Update the message with the new button states
                await component.UpdateAsync(msg =>
                {
                    var builder = new ComponentBuilder();
                    // Disable DM button and enable Share button based on their states in the dictionary
                    CreateBtnDM(builder, isDisabled: true);
                    CreateBtnShare(builder, isDisabled: _buttonIsDisabledStates[ButtonId.btnShare]);
                    msg.Components = builder.Build();
                });

                // Mark the DM button as clicked
                _buttonIsDisabledStates[ButtonId.btnDm] = true;

                // send a DM to the user who clicked the button with the content of the message
                await component.User.SendMessageAsync(component.Message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the DM button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        public void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false)
        {
            // Create the DM button
            builder.WithButton("DM", ButtonId.btnDm.ToString(), ButtonStyle.Primary, disabled: isDisabled);
        }

        public async Task BtnShare(SocketMessageComponent component, string message)
        {
            try
            {
                _logger.LogInformation("Share button clicked");

                // Update the message with the new button states
                await component.UpdateAsync(msg =>
                {
                    var builder = new ComponentBuilder();
                    // Disable Share button and enable DM button based on their states in the dictionary
                    CreateBtnDM(builder, isDisabled: _buttonIsDisabledStates[ButtonId.btnDm]);
                    CreateBtnShare(builder, isDisabled: true);
                    msg.Components = builder.Build();
                });

                // Mark the Share button as clicked
                _buttonIsDisabledStates[ButtonId.btnShare] = true;

                // send a message to the channel where the button was clicked with the content of the message
                await component.Channel.SendMessageAsync($"{component.Message.Content} {component.User.Mention}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the Share button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }

        }

        public void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false)
        {
            builder.WithButton("Share", ButtonId.btnShare.ToString(), ButtonStyle.Success, disabled: isDisabled);
        }
    }
}
