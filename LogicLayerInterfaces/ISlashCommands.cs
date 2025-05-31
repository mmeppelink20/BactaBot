using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface ISlashCommands
    {
        Task HandleBactaCommand(SocketSlashCommand command);
        Task HandleQuestionCommand(SocketSlashCommand command);
        Task HandleSummarizeCommand(SocketSlashCommand command);
        Task HandlePingCommand(SocketSlashCommand command);
        Task HandleCreditsCommand(SocketSlashCommand command);
        Task HandleLeaderboardCommand(SocketSlashCommand command);
    }
}
