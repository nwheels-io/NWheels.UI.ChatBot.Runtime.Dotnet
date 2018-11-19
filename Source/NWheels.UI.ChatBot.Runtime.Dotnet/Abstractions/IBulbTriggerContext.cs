using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Internals;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbTriggerContext
    {
        Task<IBulbTriggerContext> UseBrain(Func<Brain, Task<Brain>> action);
        Brain Brain { get; }
    }
}
