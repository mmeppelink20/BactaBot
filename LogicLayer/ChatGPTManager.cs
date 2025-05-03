using DataObjects;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace LogicLayer
{
    public class ChatGPTManager : IChatGPTManager
    {
        private readonly ILogger<IChatGPTManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IGuildMessageManager _messageManager;

        public ChatGPTManager(ILogger<IChatGPTManager> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IGuildMessageManager messageManager)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _commands = commands;
            _messageManager = messageManager;
        }

        public async Task<string> RetrieveChatBotCompletionFromChatGPTAsync(SocketMessage userMessage, int minutes)
        {
            var response = string.Empty;
            try
            {
                ChatClient client = new(model: _configuration["BACTA_BOT_MODEL"], apiKey: _configuration["OPEN_AI_API_KEY"]);


                var messages = await _messageManager.RetrieveDiscordMessagesByChannelIDAndMinutesAsync(userMessage.Channel.Id, minutes);

                var prompt = $"{_configuration["BACTA_BOT_PROMPT"]}\n\n";

                foreach(var message in messages)
                {

                    var role = message.UserName != null && message.UserName.Equals(_configuration["BACTA_BOT_NAME"])
                        ? "[ASSISTANT]"
                        : "[USER]";

                    if(message.MessageId == userMessage.Id)
                    {
                        role = "[USER (QUESTION TO RESPOND TO TAKE ACTION ON WHAT'S IN THIS MESSAGE)]";
                    }

                    prompt +=
                        $" {role} USERNAME: {message.UserName} | " +
                        $"NICKNAME: {message.NickName} | " +
                        $"MESSAGE CONTENT: {message.CleanContent} | " +
                        $"MESSAGE DATETIME: {message.MessageDatetime} | " +
                        $"MESSAGE ID: {message.MessageId} | " +
                        $"REPLIED TO MESSAGE ID (nullable): {message.RepliedToMessageId} \n\n";
                }


                ChatCompletion completionResult = await client.CompleteChatAsync(prompt);

                response = completionResult.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, "Failed to retrieve Bacta bot mention response from ChatGPT");
            }
            // if null or empty, return a default message
            return response ?? "Failed to retrieve Bacta bot mention response";
        }

        public Task<string> RetrieveChatBotCompletionFromChatGPTAsync(ulong channelID, int minutes, SocketMessage userMessage)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RetrieveConversationSummaryFromChatGPTAsync(List<SocketMessage> messages)
        {
            var response = string.Empty;

            try
            {
                ChatClient client = new(model: _configuration["SUMMARY_MODEL"], apiKey: _configuration["OPEN_AI_API_KEY"]);

                ChatCompletion completionResult = await client.CompleteChatAsync(_configuration["SUMMARY_PROMPT"]);

                response = completionResult.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, "Failed to retrieve conversation summary from ChatGPT");
            }

            // if null or empty, return a default message
            return response ?? "Failed to retrieve conversation summary";
        }

        public async Task<string> RetrieveQuestionAboutConversationFromChatGPTAsync(string question, List<DiscordMessageVM> messages)
        {
            var response = string.Empty;

            try
            {
                ChatClient client = new(model: _configuration["SUMMARY_MODEL"], apiKey: _configuration["OPEN_AI_API_KEY"]);

                var prompt = $"{_configuration["QUESTION_PROMPT"]}\n\n";

                prompt += $"{question}\n\n";

                foreach (var message in messages)
                {
                    var role = message.UserName != null && message.UserName.Equals(_configuration["BACTA_BOT_NAME"])
                        ? "[ASSISTANT]"
                        : "[USER]";
                    prompt +=
                        $" {role} USERNAME: {message.UserName} | " +
                        $"NICKNAME: {message.NickName} | " +
                        $"MESSAGE CONTENT: {message.CleanContent} | " +
                        $"MESSAGE DATETIME: {message.MessageDatetime} | " +
                        $"MESSAGE ID: {message.MessageId} | " +
                        $"REPLIED TO MESSAGE ID (nullable): {message.RepliedToMessageId} \n\n";
                }

                ChatCompletion completionResult = await client.CompleteChatAsync(prompt);

                response = completionResult.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, "Failed to retrieve conversation summary from ChatGPT");
            }

            // if null or empty, return a default message
            return response ?? "Failed to retrieve conversation summary";
        }

    }
}
