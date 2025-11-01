using Discord;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IEventManager
    {
        Func<Task>? ShutdownRequested { get; set; }
        Task MessageRecieved(SocketMessage message);
        Task MessageDeleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel);
        Task MessageUpdated(SocketMessage oldMessage, SocketMessage newMessage);
        Task SlashCommandExecuted(SocketSlashCommand command);
        Task ButtonExecuted(SocketMessageComponent component);
        Task AutocompleteExecuted(SocketAutocompleteInteraction interaction);
    }
}
