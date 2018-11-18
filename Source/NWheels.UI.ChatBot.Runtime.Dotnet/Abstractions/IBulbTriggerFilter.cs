using System;
using System.Threading.Tasks;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbTriggerFilter
    {
        Task<IBulbTriggerContext> OnInitBot(IBulbTriggerContext context);
        Task<IBulbTriggerContext> OnInput(IBulbTriggerContext context, IInput input);
        Task<IBulbTriggerContext> OnIntent(IBulbTriggerContext context, IIntent intent);
        Task<IBulbTriggerContext> OnEffect(IBulbTriggerContext context, IEffect effect);
    }
}
