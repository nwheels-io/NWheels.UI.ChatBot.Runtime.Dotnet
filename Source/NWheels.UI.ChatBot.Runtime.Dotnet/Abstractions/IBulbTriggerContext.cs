using System.Collections.Generic;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbTriggerContext
    {
        IBulbTriggerContext Light(IBulb bulb, int? intensity = null, int? autoDimBy = null);
        IBulbTriggerContext Adjust(IBulb bulb, int? intensity = null, int? autoDimBy = null);
        IReadOnlyCollection<IBulb> Bulbs { get; }
    }
}
