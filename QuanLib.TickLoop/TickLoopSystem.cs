using QuanLib.BusyWaiting;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop
{
    public abstract class TickLoopSystem : UnmanagedRunnable, ITickUpdatable
    {
        protected TickLoopSystem(TimeSpan tickMaxTime, ILoggerGetter? loggerGetter = null) : base(loggerGetter)
        {
            TickMaxTime = tickMaxTime;
            SystemTick = 0;
            _syatemStopwatch = new();
            _tickStopwatch = new();

            _busyLoop = new(1, loggerGetter);
            _busyLoop.SetDefaultThreadName("BusyLoop Thread");
            AddSubtask(_busyLoop);

            TickStart += OnTickStart;
            TickUpdate += OnTickUpdate;
            TickEnd += OnTickEnd;
        }

        private readonly Stopwatch _syatemStopwatch;

        private readonly Stopwatch _tickStopwatch;

        private readonly BusyLoop _busyLoop;

        public TimeSpan SystemRunningTime => _syatemStopwatch.Elapsed;

        public TimeSpan TickRunningTime => _tickStopwatch.Elapsed;

        public TimeSpan TickStartTime { get; private set; }

        public TimeSpan TickEndTime { get; private set; }

        public TimeSpan TickMaxTime { get; }

        public int SystemTick { get; private set; }

        public event TickUpdateHandler TickStart;

        public event TickUpdateHandler TickUpdate;

        public event TickUpdateHandler TickEnd;

        protected override void Run()
        {
            _syatemStopwatch.Start();

            while (IsRunning)
            {
                ResetTick();
                TickStart.Invoke(SystemTick);
                TickUpdate.Invoke(SystemTick);
                TickEnd.Invoke(SystemTick);
                SystemInterrupt();
            }

            _syatemStopwatch.Stop();
        }

        protected abstract void OnTickStart(int tick);

        public abstract void OnTickUpdate(int tick);

        protected abstract void OnTickEnd(int tick);

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
            TickEndTime = TickStartTime + TickMaxTime;
            _tickStopwatch.Restart();
        }

        private void SystemInterrupt()
        {
            if (!IsRunning)
                return;

            TimeSpan tickMaxTime = TickMaxTime - TimeSpan.FromMilliseconds(_busyLoop.DelayMilliseconds * 2);

            _busyLoop.Resume();
            _busyLoop.SubmitAndWaitAsync(() => !IsRunning || TickRunningTime >= tickMaxTime).Wait();
            _busyLoop.Pause();

            while (IsRunning && TickRunningTime < TickMaxTime) { }
        }
    }
}
