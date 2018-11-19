using System;
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

        public async Task<IBulbTriggerContext> UseBrain(Func<Brain, Task<Brain>> action)
        {
            var nextBrain = await action(Brain);
            return new BulbTriggerContext(nextBrain);
        }

        public Brain Brain { get; }
    }
}