using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Internals;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbTriggerContext
    {
        Task<IBulbTriggerContext> Light(IBulb bulb, int? intensity = null, int? autoDimBy = null);
        Task<IBulbTriggerContext> Adjust(IBulb bulb, int? intensity = null, int? autoDimBy = null);
        Brain Brain { get; }
    }
}
