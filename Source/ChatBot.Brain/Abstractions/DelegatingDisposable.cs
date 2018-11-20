using System;

namespace ChatBot.Brain.Abstractions
{
    public class DelegatingDisposable : IDisposable
    {
        public readonly Action OnCreate;
        public readonly Action OnDispose;

        public DelegatingDisposable(Action onCreate = null, Action onDispose = null)
        {
            this.OnCreate = onCreate;
            this.OnDispose = onDispose;
            
            OnCreate?.Invoke();
        }

        public void Dispose()
        {
            OnDispose?.Invoke();
        }
    }
}