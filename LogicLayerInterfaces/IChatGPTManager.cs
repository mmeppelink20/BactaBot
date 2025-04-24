using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IChatGPTManager
    {
        Task<string> RetrieveQuestionAboutConversationFromChatGPTAsync(string question, List<SocketMessage> messages);
        Task<string> RetrieveConversationSummaryFromChatGPTAsync(List<SocketMessage> messages);
        Task<string> RetrieveChatBotCompletionFromChatGPTAsync(SocketMessage userMessage, int minutes);
        Task<string> RetrieveChatBotCompletionFromChatGPTAsync(ulong channelID, int minutes, SocketMessage userMessage);

    }
}
