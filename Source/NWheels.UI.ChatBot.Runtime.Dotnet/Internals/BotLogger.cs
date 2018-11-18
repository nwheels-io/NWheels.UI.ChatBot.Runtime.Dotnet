using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Internals
{
    public class BotLogger
    {
        private readonly ILogWriter _writer;

        public BotLogger(ILogWriter writer)
        {
            _writer = writer;
        }

        void BotStatus(BotStatus newStatus)
        {
            _writer.Event("STATE", input: newStatus);
        }

        void BotBrain(Brain brain)
        {
            _writer.Event("BRAIN", input: brain);
        }
    }


    public enum EventTarget
    {
        State,
        Bot,
        Light,
        Shake,
        Dim,
        AutoDim,
        Listen,
        Dispatch,
        Sched,
        Nlu,
        Tts,
        Stt
    }
}