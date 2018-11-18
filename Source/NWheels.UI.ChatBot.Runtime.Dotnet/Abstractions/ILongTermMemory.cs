using System;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions
{
    public interface ILongTermMemory
    {
        bool Have<T>();
        T Get<T>(Func<T> init);
        void Set<T>(T obj);
        void Remove<T>();
    }
}
