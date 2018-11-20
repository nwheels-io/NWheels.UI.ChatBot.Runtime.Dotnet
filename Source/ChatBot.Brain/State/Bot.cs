using System.Collections.Generic;
using System.Collections.Immutable;
using ChatBot.Brain.Abstractions;

namespace ChatBot.Brain.State
{
    public class Bot
    {
        public readonly Memory Memory;
        public readonly Mind Mind;
        public readonly ImmutableDictionary<string, Bulb> Bulbs;
        public readonly ImmutableList<Listener> Listeners;
        public readonly ImmutableList<Listener> OrderedListeners;
        public readonly IInput ReceivedInput;
        public readonly IIntent ReceivedIntent;
        public readonly IEffect EmittedEffect;

        public Bot(Memory memory, IEnumerable<Bulb> bulbs)
        {
            
        }
    }
}
