using DataObjects;
using Discord.Commands;
using Discord.WebSocket;
using LogicLayerInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Diagnostics.Contracts;

namespace LogicLayer
{
    public class ChatGPTManager : IChatGPTManager
    {
        private readonly ILogger<IChatGPTManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IGuildMessageManager _messageManager;
        //private readonly ChatTool _discordMessageTool;

        public ChatGPTManager(ILogger<IChatGPTManager> logger, IConfiguration configuration, DiscordSocketClient client, CommandService commands, IGuildMessageManager messageManager)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;
            _commands = commands;
            _messageManager = messageManager;
            //_discordMessageTool = CreateDiscordMessageTool();
        }

        public async Task<string> RetrieveChatBotCompletionFromChatGPTAsync(SocketMessage userMessage, int minutes)
        {
            var response = string.Empty;

            try
            {
                ChatClient client = new(model: _configuration["BACTA_BOT_MODEL"], apiKey: _configuration["OPEN_AI_API_KEY"]);

                var messages = await _messageManager.RetrieveDiscordMessagesByChannelIDAndMinutesAsync(userMessage.Channel.Id, minutes);

                List<ChatMessage> chatMessages = [];

                chatMessages.Add(ChatMessage.CreateSystemMessage($"{_configuration["BACTA_BOT_PROMPT"]}"));

                foreach (var message in messages)
                {
                    // Skip the user's message if it matches
                    if (message.MessageId == userMessage.Id)
                        continue;

                    string content = message.IsDeleted
                        ? message.ToStringForDeletedMessage()
                        : message.ToStringForCompletion();

                    if (message.UserName == _configuration["BACTA_BOT_NAME"])
                    {
                        chatMessages.Add(ChatMessage.CreateAssistantMessage(content));
                    }
                    else
                    {
                        chatMessages.Add(ChatMessage.CreateUserMessage(content));
                    }
                }

                // log the entire chatMessages
                if (Int32.Parse(_configuration["LOG_BACTA_PROMPT"] ?? "0") == 1)
                {
                    _logger.LogInformation("Chat Messages: \n\n{ChatMessages}\n\n", string.Join("\n", chatMessages.Select(m => m.Content[0].Text)));
                }

                ChatCompletion completionResult = await client.CompleteChatAsync([.. chatMessages]);

                response = completionResult.Content[0].Text;
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, "Failed to retrieve Bacta bot mention response from ChatGPT");
            }

            // if null or empty, return a default message  
            return response ?? "Failed to retrieve Bacta bot mention response";
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

                // log the prompt
                _logger.LogInformation("Prompt: \n\n{Prompt}\n\n", prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError((int)BactaLogging.LogEvent.ChatGPT, ex, "Failed to retrieve conversation from ChatGPT");
            }

            // if null or empty, return a default message
            return response ?? "Failed to retrieve conversation summary";
        }

        private static ChatTool CreateDiscordMessageTool()
        {
            return ChatTool.CreateFunctionTool(
                functionName: "GetDiscordMessages",
                functionDescription: "Contextual Discord messages used to inform Bacta Bot's response",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "array",
                    "items": {
                        "type": "object",
                        "properties": {
                            "userName": { "type": "string" },
                            "nickName": { "type": "string" },
                            "messageId": { "type": "string" },
                            "content": { "type": "string" },
                            "timestamp": { "type": "string", "format": "date-time" },
                            "isDeleted": { "type": "boolean" },
                            "repliedToMessageId": { "type": "string" }
                        },
                        "required": ["userName", "content", "timestamp"]
                    }
                }
                """)
            );

        }


    }
}
