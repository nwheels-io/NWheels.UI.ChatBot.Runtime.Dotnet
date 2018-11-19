using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NWheels.UI.ChatBot.Runtime.Dotnet.Abstractions;
using NWheels.UI.ChatBot.Runtime.Dotnet.Internals;

namespace NWheels.UI.ChatBot.Runtime.Dotnet
{
    public class Bot
    {
        public readonly ImmutableList<IChannel> Channels;
        public readonly ImmutableList<IIntentFilter> IntentFilters;
        public readonly ImmutableList<IBulbTriggerFilter> BulbTriggers;
        public readonly CancellationToken Cancel;
        public readonly Brain Brain;
        public readonly BotStatus Status;
        public readonly Exception Fault;
        
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
            this.Channels = channels;
            this.IntentFilters = intentFilters;
            this.BulbTriggers = bulbTriggers;
            this.Cancel = cancel;
            this.Brain = brain;
            this.Status = status;
            this.Fault = fault;
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

        private async Task<Bot> RunMainLoop()
        {
            var bot = this;

            while (bot.Status == BotStatus.Ready)
            {
                if (!Cancel.IsCancellationRequested)
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
            var allChannelPulls = Channels.Select(c => c.WaitForInput(Cancel)).ToArray();
            var pullInput = (await Task.WhenAny(allChannelPulls)).Result;
            var input = await pullInput();
            
            var nextBrain = await InvokeBulbTriggerFilters((trigger, context) => trigger.OnInput(context, input));
            var nextBot = WithBrain(nextBrain);

            return (input, nextBot);
        }

        private async Task<(IIntent intent, Bot withIntent)> PullNextIntent(IInput input)
        {
            var volunteer = IntentFilters.FirstOrDefault(filter => filter.WillAnalyzeInput(input));

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
            var nextBrain = await Brain.AutoDimBulbs();
            return WithBrain(nextBrain);
        }

        private async Task<Bot> DispatchIntent(IIntent intent)
        {
            var nextBrain = await Brain.DispatchIntent(intent);
            return WithBrain(nextBrain);
        }

        private Task<IBulb> ScheduleNextBulb()
        {
            return Brain.ScheduleNextBulb();
        }

        private async Task<Bot> ActUponNextBulb(IBulb nextBulb)
        {
            var context = new BulbContext(this, nextBulb);
            var nextContext = await nextBulb.Act(context);
            return ((BulbContext)nextContext).Bot;
        }

        private async Task<Brain> InvokeBulbTriggerFilters(BulbTriggerInvocation invocation)
        {
            IBulbTriggerContext triggerContext = new BulbTriggerContext(Brain);

            foreach (var trigger in BulbTriggers)
            {
                triggerContext = await invocation(trigger, triggerContext);
            }

            return triggerContext.Brain;
        }

        private Bot WithBrain(Brain newBrain)
        {
            return new Bot(Channels, IntentFilters, BulbTriggers, Cancel, newBrain, Status, Fault);
        }

        private Bot WithStatus(BotStatus newStatus)
        {
            return new Bot(Channels, IntentFilters, BulbTriggers, Cancel, Brain, newStatus, Fault);
        }

        private Bot WithFault(Exception newFault)
        {
            return new Bot(Channels, IntentFilters, BulbTriggers, Cancel, Brain, Status, newFault);
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
            catch (BotFaultException e)
            {
                finished = e.Bot.WithStatus(BotStatus.Faulted).WithFault(e);
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

            public async Task<IBulbContext> UseBrain(Func<Brain, Task<Brain>> action)
            {
                var nextBrain = await action(Bot.Brain);
                var nextBot = (nextBrain != Bot.Brain ? Bot.WithBrain(nextBrain) : Bot);
                var nextContext = (nextBot != Bot ? new BulbContext(nextBot, ThisBulb) : this);

                return nextContext;
            }

            public Task<IBulbContext> ListenFor<TIntent>(IntentListenMode mode) where TIntent : IIntent
            {
                throw new NotImplementedException();
                /*
                var nextBrain = await action(Bot.Brain);
                var nextBot = (nextBrain != Bot.Brain ? Bot.WithBrain(nextBrain) : Bot);
                var nextContext = (nextBot != Bot ? new BulbContext(nextBot, ThisBulb) : this);

                return nextContext;
                */
            }

            public async Task EmitEffect(IEffect effect, IEnumerable<IChannel> limitToChannels = null)
            {
                var targetChannelSet = (limitToChannels ?? Bot.Channels).ToImmutableList();
                
                foreach (var channel in targetChannelSet)
                {
                    await channel.EmitEffect(effect, Bot.Cancel);
                }
            }

            public Bot Bot { get; }
            public IBulb ThisBulb { get; }
        }
    }
}
