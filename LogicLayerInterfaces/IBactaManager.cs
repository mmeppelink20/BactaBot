using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicLayerInterfaces
{
    public interface IBactaManager
    {
        string GenerateBactaResponseAsync(ComponentBuilder builder);
    }
}
