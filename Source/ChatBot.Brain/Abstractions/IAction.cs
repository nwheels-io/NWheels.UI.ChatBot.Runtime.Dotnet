using System.Threading.Tasks;

namespace ChatBot.Brain.Abstractions
{
    public interface IAction<TState>
    {
    }

    public interface ISyncAction<TState> : IAction<TState>
    {
        TState Apply(TState state);
    }
    
    public interface IAsyncAction<TState> : IAction<TState>
    {
        Task<TState> Apply(TState state, IDispatch<TState> dispatch);
    }

    public interface ISyncAction<TState, TResult> : IAction<TState>
    {
        (TState state, TResult result) Apply(TState state);
    }
    
    public interface IAsyncAction<TState, TResult> : IAction<TState>
    {
        Task<(TState state, TResult result)> Apply(TState state, IDispatch<TState> dispatch);
    }
}
