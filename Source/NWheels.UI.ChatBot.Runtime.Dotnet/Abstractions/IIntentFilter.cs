using System.Threading.Tasks;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IIntentFilter
    {
        bool WillAnalyzeInput(IInput input);
        Task<IIntent> AnalyzeInput(IInput input);
    }
}