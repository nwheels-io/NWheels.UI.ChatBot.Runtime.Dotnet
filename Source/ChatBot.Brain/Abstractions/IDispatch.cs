using System.Threading.Tasks;

namespace ChatBot.Brain.Abstractions
{
    public interface IDispatch<TState>
    {
        void Dispatch(ISyncAction<TState> action);
        Task Dispatch(IAsyncAction<TState> action);
        TResult Dispatch<TResult>(ISyncAction<TState, TResult> action);
        Task<TResult> Dispatch<TResult>(IAsyncAction<TState, TResult> action);
    }
}
