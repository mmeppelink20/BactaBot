using Discord;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer
{
    public class Buttons(ILogger<IButtons> logger, IBactaManager bactaManager) : IButtons
    {
        private readonly ILogger<IButtons> _logger = logger;
        private readonly IBactaManager _bactaManager = bactaManager;

        public async Task BtnDm(SocketMessageComponent component, string? message)
        {
            try
            {
                _logger.LogInformation("DM button clicked");

                var messageId = component.Message.Id;

                await component.UpdateAsync(msg =>
                {
                    msg.Content = $"DM sent!";
                    msg.Components = new ComponentBuilder().Build();
                });

                await component.User.SendMessageAsync(component.Message.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the DM button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        public async Task BtnShare(SocketMessageComponent component, string? message)
        {
            try
            {
                _logger.LogInformation("Share button clicked");

                var messageId = component.Message.Id;

                await component.RespondAsync($"{component.Message.Content} {component.User.Mention}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the Share button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        public async Task BtnRespin(SocketMessageComponent component)
        {
            try
            {
                // if someone who clicked the button is not the author of the command invoker, then don't allow
                if (component.User.Id != component.Message.Interaction.User.Id)
                {
                    await component.RespondAsync("You can't spin someone else's bacta!", ephemeral: true);
                    return;
                }

                _logger.LogInformation("Respin button clicked");

                var messageId = component.Message.Id;

                var builder = new ComponentBuilder();
                var respin = _bactaManager.GenerateBactaResponseAsync(builder);

                await component.UpdateAsync(msg =>
                {   
                    msg.Embed = BactaManager.BactaEmbedBuilder(respin).Build();
                    msg.Components = builder.Build();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the Respin button click.");
                await component.RespondAsync("An error occurred while processing your command.", ephemeral: true);
            }
        }

        public void CreateBtnDM(ComponentBuilder builder, bool isDisabled = false)
        {
            builder.WithButton(ButtonId.btnDm.GetLabel(), ButtonId.btnDm.ToString(), ButtonId.btnDm.GetStyle(), disabled: isDisabled);
        }

        public void CreateBtnShare(ComponentBuilder builder, bool isDisabled = false)
        {
            builder.WithButton(ButtonId.btnShare.GetLabel(), ButtonId.btnShare.ToString(), ButtonId.btnShare.GetStyle(), disabled: isDisabled);
        }

        public void CreateBtnRespin(ComponentBuilder builder, bool isDisabled = false)
        {
            builder.WithButton(ButtonId.btnRespin.GetLabel(), ButtonId.btnRespin.ToString(), ButtonId.btnRespin.GetStyle(), disabled: isDisabled);
        }
    }
}
