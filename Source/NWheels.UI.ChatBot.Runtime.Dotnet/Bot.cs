using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;
using NWheels.UI.ChatBot.Runtime.Dotnet.Internals;

namespace NWheels.UI.ChatBot.Runtime.Dotnet
{
    public class Bot
    {
        private readonly ImmutableList<IChannel> _channels;
        private readonly ImmutableList<IIntentFilter> _intentFilters;
        private readonly ImmutableList<IBulbTriggerFilter> _bulbTriggers;
        private readonly CancellationToken _cancel;
        private readonly Brain _brain;
        private readonly BotStatus _status;
        private readonly Exception _fault;

        public Bot(
            IEnumerable<IChannel> channels, 
            IEnumerable<IIntentFilter> intentFilters,
            IEnumerable<IBulbTriggerFilter> bulbTriggers)
            : this(
                channels.ToImmutableList(),
                intentFilters.ToImmutableList(),
                bulbTriggers.ToImmutableList(),
                CancellationToken.None,
                brain: null,
                status: BotStatus.Off,
                fault: null)
        {
        }

        private Bot(
            ImmutableList<IChannel> channels, 
            ImmutableList<IIntentFilter> intentFilters,
            ImmutableList<IBulbTriggerFilter> bulbTriggers,
            CancellationToken cancel,
            Brain brain,
            BotStatus status, 
            Exception fault)
        {
            _channels = channels;
            _intentFilters = intentFilters;
            _bulbTriggers = bulbTriggers;
            _cancel = cancel;
            _brain = brain;
            _status = status;
            _fault = fault;
        }

        public async Task<Bot> Start(CancellationToken cancel)
        {
            ValidateStatus(BotStatus.Off, BotStatus.Aborted, BotStatus.Faulted);

            var started = await Init();

            return started;
        }
        
        public async Task<Bot> Run()
        {
            ValidateStatus(BotStatus.Ready);

            var finished = await FinishedOrFaulted(this, b => b.Init());

            return finished;
        }

        private void ValidateStatus(params BotStatus[] expected)
        {
            var actualState = this.Status;
            
            if (!expected.Contains(actualState))
            {
                throw new InvalidOperationException(
                    $"Must be {string.Join(" or ", expected)}, but actually {actualState}.");
            }
        }

        private async Task<Bot> Init()
        {
            var nextBrain = await InvokeBulbTriggerFilters((trigger, context) => trigger.OnInitBot(context));
            return WithBrain(nextBrain);
        }

        public BotStatus Status => _status;
        public Exception Fault => _fault;

        private async Task<Bot> RunMainLoop()
        {
            var bot = this;

            while (bot.Status == BotStatus.Ready)
            {
                if (!_cancel.IsCancellationRequested)
                {
                    bot = await FinishedOrFaulted(bot, b => b.NextMainLoopIteration());
                }
                else
                {
                    bot = this.WithStatus(BotStatus.Aborted);
                }
            }

            return bot;
        }
        
        private async Task<Bot> NextMainLoopIteration()
        {
            var (input, withInput) = await PullNextInput();
            var (intent, withIntent) = await withInput.PullNextIntent(input);
            var dispatched = await withIntent.DispatchIntent(intent);
            var autoDimmed = await dispatched.AutoDimBulbs();
            var nextBulb = await autoDimmed.ScheduleNextBulb();
            var acted = await autoDimmed.ActUponNextBulb(nextBulb);

            return acted;
        }

        private async Task<(IInput input, Bot withInput)> PullNextInput()
        {
            var allChannelPulls = _channels.Select(c => c.WaitForInput()).ToArray();
            var pullInput = (await Task.WhenAny(allChannelPulls)).Result;
            var input = await pullInput();
            
            var nextBrain = await InvokeBulbTriggerFilters((trigger, context) => trigger.OnInput(context, input));
            var nextBot = WithBrain(nextBrain);

            return (input, nextBot);
        }

        private async Task<(IIntent intent, Bot withIntent)> PullNextIntent(IInput input)
        {
            var volunteer = _intentFilters.FirstOrDefault(filter => filter.WillAnalyzeInput(input));

            if (volunteer != null)
            {
                var intent = await volunteer.AnalyzeInput(input);
                
                var nextBrain = await InvokeBulbTriggerFilters((trigger, context) => trigger.OnIntent(context, intent));
                var nextBot = WithBrain(nextBrain);

                return (intent, nextBot);
            }
            
            throw new BotFaultException(this, $"No intent filter volunteered to analyze input '{input}'.");
        }

        private async Task<Bot> AutoDimBulbs()
        {
            var nextBrain = await _brain.AutoDimBulbs();
            return WithBrain(nextBrain);
        }

        private async Task<Bot> DispatchIntent(IIntent intent)
        {
            var nextBrain = await _brain.DispatchIntent(intent);
            return WithBrain(nextBrain);
        }

        private Task<IBulb> ScheduleNextBulb()
        {
            return _brain.ScheduleNextBulb();
        }

        private async Task<Bot> ActUponNextBulb(IBulb nextBulb)
        {
            var nextBrain = await _brain.ActUponBulb(nextBulb);
            return WithBrain(nextBrain);
        }

        private async Task<Brain> InvokeBulbTriggerFilters(BulbTriggerInvocation invocation)
        {
            IBulbTriggerContext triggerContext = new BulbTriggerContext(_brain);

            foreach (var trigger in _bulbTriggers)
            {
                triggerContext = await invocation(trigger, triggerContext);
            }

            return triggerContext.Brain;
        }

        private Bot WithBrain(Brain newBrain)
        {
            return new Bot(_channels, _intentFilters, _bulbTriggers, _cancel, newBrain, _status, _fault);
        }

        private Bot WithStatus(BotStatus newStatus)
        {
            return new Bot(_channels, _intentFilters, _bulbTriggers, _cancel, _brain, newStatus, _fault);
        }

        private Bot WithFault(Exception newFault)
        {
            return new Bot(_channels, _intentFilters, _bulbTriggers, _cancel, _brain, _status, newFault);
        }

        private static async Task<Bot> FinishedOrFaulted(Bot current, Func<Bot, Task<Bot>> action)
        {
            Bot finished;
            
            try
            {
                finished = await action(current);
            }
            catch (OperationCanceledException)
            {
                finished = current.WithStatus(BotStatus.Aborted);
            }
            catch (Exception e)
            {
                finished = current.WithStatus(BotStatus.Faulted).WithFault(e);
            }

            return finished;
        }

        private delegate Task<IBulbTriggerContext> BulbTriggerInvocation(
            IBulbTriggerFilter trigger,
            IBulbTriggerContext context);

        private class BulbContext : IBulbContext
        {
            public BulbContext(Bot bot, IBulb thisBulb)
            {
                this.Bot = bot;
                this.ThisBulb = thisBulb;
            }

            public Task<IBulbContext> Light(IBulb bulb, int? intensity = null, int? autoDimBy = null)
            {
                throw new System.NotImplementedException();
            }

            public Task<IBulbContext> Adjust(IBulb bulb, int? intensity = null, int? autoDimBy = null)
            {
                throw new System.NotImplementedException();
            }

            public Task<IBulbContext> EmitEffect(IEffect effect, IEnumerable<IChannel> limitToChannels = null)
            {
                throw new System.NotImplementedException();
            }

            public Bot Bot { get; }
            public IBulb ThisBulb { get; }
        }
    }
}
