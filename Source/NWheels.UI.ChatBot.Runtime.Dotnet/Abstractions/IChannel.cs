using System;
using System.Threading;
using System.Threading.Tasks;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface IChannel
    {
        Task<PullNextInput> WaitForInput();
    }
    
    public delegate Task<IInput> PullNextInput();
}
