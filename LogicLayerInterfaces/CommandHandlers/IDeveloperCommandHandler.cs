using Discord.WebSocket;

namespace LogicLayerInterfaces.CommandHandlers
{
    public interface IDeveloperCommandHandler
    {
        Task HandleAddCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand);
        Task HandleRemoveCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand);
        Task HandleListCommand(SocketSlashCommand command);
    }
}