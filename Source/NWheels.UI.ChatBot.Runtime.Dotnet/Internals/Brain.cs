using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;

namespace NWheels.UI.ChatBot.Runtime.Dotnet.Internals
{
    public class Brain
    {
        public readonly Memory Memory;
        public readonly BulbListener OnBulbChanged;
        public readonly ImmutableList<IBulb> Bulbs;
        public readonly ImmutableDictionary<Type, ImmutableList<Delegate>> ListenersByIntentType;

        public Brain(Memory memory, BulbListener onBulbChanged)
            : this(memory, ImmutableList<IBulb>.Empty, onBulbChanged)
        {
        }

        private Brain(Memory memory, ImmutableList<IBulb> bulbs, BulbListener onBulbChanged)
        {
            this.Memory = memory;
            this.Bulbs = bulbs;
            this.OnBulbChanged = onBulbChanged;
        }

        public async Task<Brain> Light(IBulb bulb, int? intensity = null, int? autoDimBy = null)
        {
            if (Bulbs.Contains(bulb))
            {
                throw new ArgumentException(
                    $"Bulb '{bulb}' is already lighted.", nameof(bulb));
            }

            var adjustedBulb = await bulb.Adjust(intensity, autoDimBy);
            var nextBrain = await WithBulb(adjustedBulb);

            return nextBrain;
        }

        public async Task<Brain> Adjust(IBulb bulb, int? intensity = null, int? autoDimBy = null)
        {
            if (!Bulbs.Contains(bulb))
            {
                throw new ArgumentException(
                    $"Bulb '{bulb}' is not lighted.", nameof(bulb));
            }

            var adjustedBulb = await bulb.Adjust(intensity, autoDimBy);
            var shouldRemove = (adjustedBulb.Intensity <= 0);
            var nextBrain =
                shouldRemove
                    ? await WithoutBulb(adjustedBulb)
                    : await WithBulbEvent(adjustedBulb);

            return nextBrain;
        }

        public Task<Brain> AutoDimBulbs()
        {
            return AutoDimBulbsRange(this.Bulbs);
        }

        public Brain AddListener<TIntent>(IntentListenMode mode, IntentListener<TIntent> listener)
            where TIntent : IIntent
        {
            throw new NotImplementedException();
        }

        public Task<Brain> DispatchIntent(IIntent intent)
        {
            throw new NotImplementedException();
        }

        public Task<IBulb> ScheduleNextBulb()
        {
            throw new NotImplementedException();
        }

        private async Task<Brain> AutoDimBulbsRange(ImmutableList<IBulb> range)
        {
            if (range.IsEmpty)
            {
                return this;
            }

            var firstBulb = range[0];
            var withFirst = await Adjust(
                bulb: firstBulb,
                intensity: firstBulb.Intensity - firstBulb.AutoDimBy);

            return await withFirst.AutoDimBulbsRange(range.GetRange(1, range.Count - 1));
        }

        private async Task<Brain> WithBulb(IBulb bulb)
        {
            var withBulb = new Brain(Memory, Bulbs.Add(bulb), OnBulbChanged);
            var withEvent = await withBulb.WithBulbEvent(bulb);
            return withEvent;
        }

        private async Task<Brain> WithoutBulb(IBulb bulb)
        {
            var withoutBulb = new Brain(Memory, Bulbs.Remove(bulb), OnBulbChanged);
            var withEvent = await withoutBulb.WithBulbEvent(bulb);
            return withEvent;
        }

        private async Task<Brain> WithBulbEvent(IBulb bulb)
        {
            return (
                OnBulbChanged != null 
                    ? await OnBulbChanged(this, bulb)
                    : this);
        }
        
        public delegate Task<Brain> BulbListener(Brain brain, IBulb bulb);
        
        public delegate Task<Brain> IntentListener<TIntent>(Brain brain, TIntent intent)
            where TIntent : IIntent;
    }
}
