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
        private readonly ImmutableList<IBulbTriggerFilter> _bulbTriggers;
        private readonly CancellationToken _cancel;
        private readonly Brain _brain;
        private readonly BotStatus _status;
        private readonly Exception _fault;

        public Bot(
            IEnumerable<IChannel> channels, 
            IEnumerable<IBulbTriggerFilter> bulbTriggers)
        {
            _channels = channels.ToImmutableList();
            _bulbTriggers = bulbTriggers.ToImmutableList();
            _brain = null;
            _fault = null;
            _status = BotStatus.Off;
        }

        private Bot(
            ImmutableList<IChannel> channels, 
            ImmutableList<IBulbTriggerFilter> bulbTriggers,
            CancellationToken cancel,
            Brain brain,
            BotStatus status, 
            Exception fault)
        {
            _channels = channels;
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
            return this
                .WithBrain(new Brain())
                .WithStatus(BotStatus.Ready);
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
            var (nextBulb, scheduled) = await autoDimmed.ScheduleNextBulb();
            var acted = await scheduled.ActUponNextBulb(nextBulb);

            return acted;
        }

        private async Task<(IInput input, Bot withInput)> PullNextInput()
        {
            throw new NotImplementedException();
        }

        private async Task<(IIntent intent, Bot withIntent)> PullNextIntent(IInput input)
        {
            throw new NotImplementedException();
        }

        private async Task<Bot> AutoDimBulbs()
        {
            throw new NotImplementedException();
        }

        private async Task<Bot> DispatchIntent(IIntent intent)
        {
            throw new NotImplementedException();
        }

        private async Task<(IBulb bulb, Bot withSchedule)> ScheduleNextBulb()
        {
            throw new NotImplementedException();
        }

        private async Task<Bot> ActUponNextBulb(IBulb nextBulb)
        {
            throw new NotImplementedException();
        }

        private Bot WithBrain(Brain newBrain)
        {
            return new Bot(_channels, _bulbTriggers, _cancel, newBrain, _status, _fault);
        }

        private Bot WithStatus(BotStatus newStatus)
        {
            return new Bot(_channels, _bulbTriggers, _cancel, _brain, newStatus, _fault);
        }

        private Bot WithFault(Exception newFault)
        {
            return new Bot(_channels, _bulbTriggers, _cancel, _brain, _status, newFault);
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
    }
}