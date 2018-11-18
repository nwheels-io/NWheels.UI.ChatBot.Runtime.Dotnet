using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Internals;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbContext
    {
        Task<IBulbContext> Light(IBulb bulb, int? intensity = null, int? autoDimBy = null);
        Task<IBulbContext> Adjust(IBulb bulb, int? intensity = null, int? autoDimBy = null);
        Task<IBulbContext> EmitEffect(IEffect effect, IEnumerable<IChannel> limitToChannels = null);
        Bot Bot { get; }
    }
}
