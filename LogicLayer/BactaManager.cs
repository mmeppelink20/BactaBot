using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicLayerInterfaces;
using static DataObjects.ButtonIdContainer;

namespace LogicLayer
{
    public class BactaManager(ILogger<IBactaManager> logger) : IBactaManager
    {
        private readonly ILogger<IBactaManager> _logger = logger;

        public string GenerateBactaResponseAsync(ComponentBuilder builder)
        {
            Random rand = new();

            int n = rand.Next(102);

            string result = n switch
            {
                0 or 1 => "bacta max win",
                >= 2 and <= 30 => "bacta",
                100 or 101 => "klytobacter",
                _ => "no bacta"
            };

            builder.WithButton("Respin", "btnRespin", ButtonId.btnRespin.GetStyle());

            return result;
        }

        public static EmbedBuilder BactaEmbedBuilder(string result)
        {
            return new EmbedBuilder
            {
                Description = result,
                Color = Color.DarkBlue
            };
        }

    }
}
