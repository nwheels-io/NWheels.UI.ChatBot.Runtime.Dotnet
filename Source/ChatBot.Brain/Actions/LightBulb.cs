using ChatBot.Brain.Abstractions;
using ChatBot.Brain.State;

namespace ChatBot.Brain.Actions
{
    public class LightBulb : ISyncAction<Bot>
    {
        public readonly Bulb Bulb;
        public readonly int? Intensity;
        public readonly int? AutoDimBy;

        public LightBulb(Bulb bulb, int? intensity = null, int? autoDimBy = null)
        {
            Bulb = bulb;
            Intensity = intensity;
            AutoDimBy = autoDimBy;
        }

        Bot ISyncAction<Bot>.Apply(Bot state)
        {
            throw new System.NotImplementedException();
        }
    }
}