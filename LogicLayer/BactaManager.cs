using Discord;
using Microsoft.Extensions.Logging;
using LogicLayerInterfaces;
using DataObjects;
using Microsoft.Extensions.Configuration;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer
{
    public class BactaManager(ILogger<IBactaManager> logger, IConfiguration configuration) : IBactaManager
    {
        private readonly ILogger<IBactaManager> _logger = logger;
        private readonly IConfiguration _configuration = configuration;

        public OperationResult<string> GenerateBactaResponse(ComponentBuilder builder)
        {
            if (builder == null)
            {
                var errorMessage = "ComponentBuilder cannot be null";
                _logger.LogWarning(errorMessage);
                return OperationResult<string>.Failure(errorMessage);
            }

            try
            {
                _logger.LogDebug("Generating bacta response");

                // Get odds from configuration with validation
                var noBactaOddsResult = ConfigurationValidator.GetRequiredValue(_configuration, 
                    ConfigurationKeys.BactaCommandNoBactaOdds, int.Parse);
                var bactaOddsResult = ConfigurationValidator.GetRequiredValue(_configuration, 
                    ConfigurationKeys.BactaCommandBactaOdds, int.Parse);
                var klytobacterOddsResult = ConfigurationValidator.GetRequiredValue(_configuration, 
                    ConfigurationKeys.BactaCommandKlytobacterOdds, int.Parse);
                var bactaMaxWinOddsResult = ConfigurationValidator.GetRequiredValue(_configuration, 
                    ConfigurationKeys.BactaCommandBactaMaxWinOdds, int.Parse);

                // Use defaults if configuration is missing
                var noBactaOdds = noBactaOddsResult.IsSuccess ? noBactaOddsResult.Data : 70;
                var bactaOdds = bactaOddsResult.IsSuccess ? bactaOddsResult.Data : 28;
                var klytobacterOdds = klytobacterOddsResult.IsSuccess ? klytobacterOddsResult.Data : 2;
                var bactaMaxWinOdds = bactaMaxWinOddsResult.IsSuccess ? bactaMaxWinOddsResult.Data : 2;

                // Log configuration issues
                if (!noBactaOddsResult.IsSuccess) _logger.LogWarning("Using default no bacta odds: {Error}", noBactaOddsResult.ErrorMessage);
                if (!bactaOddsResult.IsSuccess) _logger.LogWarning("Using default bacta odds: {Error}", bactaOddsResult.ErrorMessage);
                if (!klytobacterOddsResult.IsSuccess) _logger.LogWarning("Using default klytobacter odds: {Error}", klytobacterOddsResult.ErrorMessage);
                if (!bactaMaxWinOddsResult.IsSuccess) _logger.LogWarning("Using default bacta max win odds: {Error}", bactaMaxWinOddsResult.ErrorMessage);

                // Generate random number with proper range validation
                var random = new Random();
                var totalRange = noBactaOdds + bactaOdds + klytobacterOdds + bactaMaxWinOdds;
                if (totalRange <= 0)
                {
                    _logger.LogError("Invalid odds configuration - total range is {TotalRange}", totalRange);
                    return OperationResult<string>.Failure("Invalid bacta odds configuration");
                }

                var roll = random.Next(0, totalRange);
                _logger.LogDebug("Bacta roll: {Roll} out of {TotalRange}", roll, totalRange);

                string result;
                if (roll < bactaMaxWinOdds)
                {
                    result = _configuration[ConfigurationKeys.BactaCommandBactaMaxWinMessage] ?? "bacta max win";
                }
                else if (roll < bactaMaxWinOdds + klytobacterOdds)
                {
                    result = _configuration[ConfigurationKeys.BactaCommandKlytobacterWinMessage] ?? "klytobacter";
                }
                else if (roll < bactaMaxWinOdds + klytobacterOdds + bactaOdds)
                {
                    result = _configuration[ConfigurationKeys.BactaCommandBactaWinMessage] ?? "bacta";
                }
                else
                {
                    result = _configuration[ConfigurationKeys.BactaCommandNoBactaWinMessage] ?? "no bacta";
                }

                // Add button with error handling
                try
                {
                    builder.WithButton("Respin", "btnRespin", ButtonId.btnRespin.GetStyle());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to add respin button to component builder");
                    // Continue without the button rather than failing completely
                }

                _logger.LogDebug("Generated bacta response: {Result}", result);
                return OperationResult<string>.Success(result);
            }
            catch (Exception ex)
            {
                var errorMessage = "Unexpected error occurred while generating bacta response";
                _logger.LogError(ex, errorMessage);
                return OperationResult<string>.Failure(errorMessage, ex);
            }
        }

        // Legacy method for backward compatibility
        public string GenerateBactaResponseAsync(ComponentBuilder builder)
        {
            var result = GenerateBactaResponse(builder);
            if (result.IsFailure)
            {
                _logger.LogError("Legacy GenerateBactaResponseAsync failed: {Error}", result.ErrorMessage);
                return "Error generating bacta response";
            }
            return result.Data!;
        }

        public static OperationResult<EmbedBuilder> CreateBactaEmbed(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return OperationResult<EmbedBuilder>.Failure("Result cannot be null or empty");
            }

            try
            {
                var embed = new EmbedBuilder
                {
                    Description = result,
                    Color = Color.DarkBlue
                };

                return OperationResult<EmbedBuilder>.Success(embed);
            }
            catch (Exception ex)
            {
                return OperationResult<EmbedBuilder>.Failure("Failed to create bacta embed", ex);
            }
        }

        // Legacy method for backward compatibility
        public static EmbedBuilder BactaEmbedBuilder(string result)
        {
            var embedResult = CreateBactaEmbed(result);
            return embedResult.IsSuccess ? embedResult.Data! : new EmbedBuilder { Description = "Error", Color = Color.Red };
        }
    }
}
