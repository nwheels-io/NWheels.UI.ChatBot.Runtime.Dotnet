using System.Threading.Tasks;

namespace ChatBot.Brain.Abstractions
{
    public interface IObserver<TState>
    {
        void OnNext(
            TState nextState, 
            TState prevState, 
            IAction<TState> action, 
            IDispatch<TState> dispatch);
    }
}
