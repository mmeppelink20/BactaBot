namespace DataObjects
{
    public class BactaLogging
    {
        public enum LogEvent
        {
            StartUpShutDown = 1,
            AddGuildMessage = 1000,
            AddChannel = 1001,
            AddGuild = 1002,
            LogCurrentGuildMessages = 1003,
            RemoveGuildMessage = 1004,
            RemoveAllGuildMessages = 1005,
            GetGuildMessage = 1006,
            GetAllGuildMessages = 1007,
            MessageRelated = 1008,
            MessageNotInGuild = 2000,
            RegisterCommand = 3000,
            ChatGPT = 3001,
            Configuration = 3002
        }
    }
}
