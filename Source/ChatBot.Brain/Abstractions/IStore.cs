using System.Threading.Tasks;
using ChatBot.Brain.State;

namespace ChatBot.Brain.Abstractions
{
    public interface IStore<TState> : IDispatch<TState>
    {
        TState State { get; }
        void AddObserver(IObserver<TState> observer);
        void RemoveObserver(IObserver<TState> observer);
    }
}
