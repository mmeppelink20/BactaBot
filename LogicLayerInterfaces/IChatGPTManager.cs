using DataObjects;
using Discord.WebSocket;

namespace LogicLayerInterfaces
{
    public interface IChatGPTManager
    {
        Task<string> RetrieveQuestionAboutConversationFromChatGPTAsync(string question, List<DiscordMessageVM> messages);
        Task<string> RetrieveConversationSummaryFromChatGPTAsync(List<SocketMessage> messages);
        Task<string> RetrieveChatBotCompletionFromChatGPTAsync(SocketMessage userMessage, int minutes);

    }
}
