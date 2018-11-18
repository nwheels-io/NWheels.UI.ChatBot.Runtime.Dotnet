using System;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.LogWriters
{
    public class LogSpan : IDisposable
    {
        private readonly Action _onDispose;

        public LogSpan(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }
    }
}