namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulbTriggerFilter
    {
        IBulbTriggerContext OnInitBot(IBulbTriggerContext context);
        IBulbTriggerContext OnInput(IBulbTriggerContext context, IInput input);
        IBulbTriggerContext OnIntent(IBulbTriggerContext context, IIntent intent);
        IBulbTriggerContext OnEffect(IBulbTriggerContext context, IEffect effect);
    }
}
