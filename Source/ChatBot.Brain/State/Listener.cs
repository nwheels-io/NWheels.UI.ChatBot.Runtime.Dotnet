using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace ChatBot.Brain.State
{
    public class Listener
    {
        public readonly string BulbId;
        public readonly ListenerMode Mode;
        public readonly ImmutableList<string> FilterPath;
        public readonly Func<Task> Callback;
    }

    public enum ListenerMode
    {
        Foreground,
        Background
    }
}
