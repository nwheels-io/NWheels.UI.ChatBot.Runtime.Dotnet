using System.Threading.Tasks;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IBulb
    {
        Task<IBulbContext> Act(IBulbContext context);
        Task<IBulb> Adjust(int? intensity = null, int? autoDimBy = null);
        int Intensity { get; }
        int AutoDimBy { get; }
    }
}
