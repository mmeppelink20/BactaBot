using Discord.WebSocket;

namespace LogicLayerInterfaces.CommandHandlers
{
    public interface IConfigCommandHandler
    {
        Task HandleSetCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand);
        Task HandleGetCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand);
        Task HandleListCommand(SocketSlashCommand command);
        Task HandleDeleteCommand(SocketSlashCommand command, SocketSlashCommandDataOption subCommand);
        Task HandleReloadCommand(SocketSlashCommand command);
        Task HandleConfigListPaginationAsync(SocketMessageComponent component, bool isNext);
    }
}