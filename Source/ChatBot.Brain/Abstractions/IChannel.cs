using System.Threading;
using System.Threading.Tasks;

namespace ChatBot.Brain.Abstractions
{
    public interface IChannel
    {
        Task<PullNextInput> WaitForInput(CancellationToken cancel);
        Task EmitEffect(IEffect effect, CancellationToken cancel);
    }
    
    public delegate Task<IInput> PullNextInput();
}
