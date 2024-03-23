using QuanLib.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop
{
    public abstract class TickLoopSystem : UnmanagedRunnable, ITickUpdatable
    {
        protected TickLoopSystem(TimeSpan tickPerTime, ILoggerGetter? loggerGetter = null) : base(loggerGetter)
        {
            TickPerTime = tickPerTime;
            SystemTick = 0;
            _tickTasks = new();
            _syatemStopwatch = new();
            _tickStopwatch = new();
        }

        private readonly ConcurrentQueue<TickTask> _tickTasks;

        private readonly Stopwatch _syatemStopwatch;

        private readonly Stopwatch _tickStopwatch;

        public TimeSpan SystemRunningTime => _syatemStopwatch.Elapsed;

        public TimeSpan TickRunningTime => _tickStopwatch.Elapsed;

        public TimeSpan TickStartTime { get; private set; }

        public TimeSpan TickEndTime { get; private set; }

        public TimeSpan TickPerTime { get; }

        public int SystemTick { get; private set; }

        protected override void Run()
        {
            while (IsRunning)
            {
                ResetTick();
                OnBeforeTick();
                OnTickUpdate(SystemTick);
                SystemInterrupt();
                OnAfterTick();
            }
        }

        public abstract void OnBeforeTick();

        public abstract void OnTickUpdate(int tick);

        public abstract void OnAfterTick();

        public void Submit(Action action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            TickTask tickTask = new(action);
            _tickTasks.Enqueue(tickTask);
        }

        public async Task<TickTask> SubmitAndWaitAsync(Action action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            TickTask tickTask = new(action);
            _tickTasks.Enqueue(tickTask);
            await tickTask.WaitForCompleteAsync();
            return tickTask;
        }

        private void ResetTick()
        {
            SystemTick++;
            TickStartTime = SystemRunningTime;
            TickEndTime = TickStartTime + TickStartTime;
            _tickStopwatch.Restart();
        }

        private void SystemInterrupt()
        {
            do
            {
                while (_tickTasks.TryDequeue(out var tickTask))
                {
                    tickTask.Start();
                    if (tickTask.State == TickTaskState.Failed && tickTask.Exception is not null)
                        throw new AggregateException(tickTask.Exception);
                }
                Thread.Yield();
            } while (IsRunning && TickRunningTime < TickPerTime);
        }
    }
}
