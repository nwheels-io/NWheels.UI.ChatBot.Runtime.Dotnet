using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ChatBot.Brain.Abstractions;

namespace ChatBot.Brain
{
    public class Store<TState> : IStore<TState>
    {
        private readonly LinkedList<Abstractions.IObserver<TState>> _observers;
        private readonly Queue<Func<Task>> _dispatchQueue;
        private TState _state;
        private int _dispatchDepth;

        public Store(TState initialState)
        {
            _observers = new LinkedList<Abstractions.IObserver<TState>>();
            _dispatchQueue = new Queue<Func<Task>>();
            _state = initialState;
            _dispatchDepth = 0;
        }
        
        public void Dispatch(ISyncAction<TState> action)
        {
            if (_dispatchDepth <= 0)
            {
                DoDispatch();
            }
            else
            {
                _dispatchQueue.Enqueue(DoDispatch);
            }

            Task DoDispatch()
            {
                var nextState = action.Apply(_state); 
                SetState(nextState, action);
                return Task.CompletedTask;
            }
        }

        public async Task Dispatch(IAsyncAction<TState> action)
        {
            if (_dispatchDepth <= 0)
            {
                await DoDispatch();
            }
            else
            {
                _dispatchQueue.Enqueue(DoDispatch);
            }

            async Task DoDispatch()
            {
                var nextState = await action.Apply(_state, this); 
                SetState(nextState, action);
            }
        }

        public TResult Dispatch<TResult>(ISyncAction<TState, TResult> action)
        {
            if (_dispatchDepth <= 0)
            {
                var (nextState, result)  = action.Apply(_state); 
                SetState(nextState, action);
                return result;
            }
            
            throw new InvalidOperationException(
                $"Reentrant sync actions with result are not allowed ({action}).");
        }

        public Task<TResult> Dispatch<TResult>(IAsyncAction<TState, TResult> action)
        {
            if (_dispatchDepth <= 0)
            {
                return DoDispatch();
            }
            else
            {
                var promise = new TaskCompletionSource<TResult>();
                _dispatchQueue.Enqueue(() => DoDispatch(promise));
                return promise.Task;
            }

            async Task<TResult> DoDispatch(TaskCompletionSource<TResult> promise = null)
            {
                var (nextState, result) = await action.Apply(_state, this); 
                SetState(nextState, action);
                promise?.SetResult(result);
                return result;
            }
        }

        public void AddObserver(Abstractions.IObserver<TState> observer)
        {
            if (_observers.Contains(observer))
            {
                throw new ArgumentException(
                    $"Specified instance of '{observer}' is already added.");
            }

            _observers.AddLast(observer);
        }

        public void RemoveObserver(Abstractions.IObserver<TState> observer)
        {
            _observers.Remove(observer);
        }

        public TState State => _state;

        private async Task InvokeDispatchQueue()
        {
            while (_dispatchQueue.Count > 0)
            {
                var operation = _dispatchQueue.Dequeue();
                _dispatchDepth++;

                try
                {
                    await operation();
                }
                catch (Exception e)
                {
                    //TODO: log error
                }
                finally
                {
                    _dispatchDepth--;
                }
            }
        }
        
        private void SetState(TState nextState, IAction<TState> action)
        {
            var prevState = _state;

            if (!nextState.Equals(_state))
            {
                InvokeObservers();
                _state = nextState;
            }
            
            void InvokeObservers()
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(nextState, prevState, action, dispatch: this);
                }
            }
        }
    }
}
