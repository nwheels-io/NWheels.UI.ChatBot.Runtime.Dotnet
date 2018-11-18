using System;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface ILogWriter
    {
        void Event(string name, object input = null, object output = null);
        IDisposable EventSpan(string name, object input = null, object output = null);
    }
}
