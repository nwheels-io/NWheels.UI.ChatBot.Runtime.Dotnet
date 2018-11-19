using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet
{
    public abstract class BulbBase : IBulb
    {
        public Task<IBulbContext> Act(IBulbContext context)
        {
            throw new System.NotImplementedException();
        }

        public Task<IBulb> Adjust(int? intensity = null, int? autoDimBy = null)
        {
            throw new System.NotImplementedException();
        }

        public int Intensity { get; }
        public int AutoDimBy { get; }
    }
}
