using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Internals;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbContext
    {
        Task<IBulbContext> UseBrain(Func<Brain, Task<Brain>> action);
        Task<IBulbContext> ListenFor<TIntent>(IntentListenMode mode) where TIntent : IIntent;
        Task EmitEffect(IEffect effect, IEnumerable<IChannel> limitToChannels = null);
    }
}
