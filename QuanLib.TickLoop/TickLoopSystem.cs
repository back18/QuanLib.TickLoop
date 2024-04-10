using QuanLib.BusyWaiting;
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
            _syatemStopwatch = new();
            _tickStopwatch = new();

            _busyLoop = new(loggerGetter);
            _busyLoop.SetDefaultThreadName("BusyLoop Thread");
            AddSubtask(_busyLoop);
        }

        private readonly Stopwatch _syatemStopwatch;

        private readonly Stopwatch _tickStopwatch;

        private readonly BusyLoop _busyLoop;

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
            _busyLoop.Submit(action);
        }

        public async Task<LoopTask> SubmitAndWaitAsync(Action action)
        {
            return await _busyLoop.SubmitAndWaitAsync(action);
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
            if (!IsRunning)
                return;

            _busyLoop.Resume();
            _busyLoop.SubmitAndWaitAsync(() => IsRunning && TickRunningTime >= TickPerTime).Wait();
            _busyLoop.Pause();
        }
    }
}
