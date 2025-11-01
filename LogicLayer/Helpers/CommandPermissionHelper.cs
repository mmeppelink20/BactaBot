using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DataObjects;
using LogicLayerInterfaces.Helpers;

namespace LogicLayer.Helpers
{
    public class CommandPermissionHelper : ICommandPermissionHelper
    {
        private readonly ILogger<ICommandPermissionHelper> _logger;
        private readonly IConfiguration _configuration;

        public CommandPermissionHelper(
            ILogger<ICommandPermissionHelper> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public bool IsUserDeveloper(ulong userId)
        {
            try
            {
                var developerUserIdList = _configuration[ConfigurationKeys.DeveloperUserIdList];
                if (string.IsNullOrEmpty(developerUserIdList))
                {
                    _logger.LogWarning("Developer user ID list is not configured");
                    return false;
                }

                var developerIds = developerUserIdList.Split(',').Select(ulong.Parse).ToHashSet();
                return developerIds.Contains(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is a developer", userId);
                return false;
            }
        }
    }
}