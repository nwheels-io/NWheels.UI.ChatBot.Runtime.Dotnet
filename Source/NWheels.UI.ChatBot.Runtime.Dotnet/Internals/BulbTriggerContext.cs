using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Internals
{
    public class BulbTriggerContext : IBulbTriggerContext
    {
        public BulbTriggerContext(Brain brain)
        {
            this.Brain = brain;
        }

        public async Task<IBulbTriggerContext> Light(IBulb bulb, int? intensity = null, int? autoDimBy = null)
        {
            var nextBrain = await Brain.Light(bulb, intensity, autoDimBy);
            return new BulbTriggerContext(nextBrain);
        }

        public async Task<IBulbTriggerContext> Adjust(IBulb bulb, int? intensity = null, int? autoDimBy = null)
        {
            var nextBrain = await Brain.Adjust(bulb, intensity, autoDimBy);
            return new BulbTriggerContext(nextBrain);
        }

        public Brain Brain { get; }
    }
}