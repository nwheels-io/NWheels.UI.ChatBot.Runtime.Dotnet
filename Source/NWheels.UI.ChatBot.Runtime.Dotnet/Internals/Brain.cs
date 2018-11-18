using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Internals
{
    public class Brain
    {
        public readonly Memory Memory;
        public readonly ImmutableList<IBulb> Bulbs;

        public Task<Brain> Light(IBulb bulb, int? intensity, int? autoDimBy)
        {
            
        }

        public Task<Brain> Adjust(IBulb bulb, int? intensity, int? autoDimBy)
        {
            
        }

        public Task<Brain> AutoDimBulbs()
        {
            
        }

        public Task<Brain> DispatchIntent(IIntent intent)
        {
            
        }

        public Task<IBulb> ScheduleNextBulb()
        {
            
        }

        public Task<Brain> ActUponBulb(IBulbContext bulbContext)
        {
            
        }
    }
}