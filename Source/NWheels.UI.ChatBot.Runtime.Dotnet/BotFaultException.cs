using System;

namespace NWheels.UI.ChatBot.Runtime.Dotnet
{
    public class BotFaultException : Exception
    {
        public readonly Bot Bot;

        public BotFaultException(Bot bot, string message) : base(message)
        {
            this.Bot = bot;
        }
    }
}