using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet
{
    public abstract class BulbBase : IBulb
    {
        Task IBulb.Act()
        {
            throw new System.NotImplementedException();
        }
    }
}
