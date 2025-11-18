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
    public abstract class TickLoopSystem : MultitaskRunnable, ITickUpdatable
    {
        protected TickLoopSystem(TimeSpan tickMaxTime, ILoggerGetter? loggerGetter = null) : base(loggerGetter)
        {
            TickMaxTime = tickMaxTime;
            SystemTick = 0;
            _syatemStopwatch = new();
            _tickStopwatch = new();

            _busyLoop = new(1, loggerGetter);
            _busyLoop.Pause();
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

        public LoopTask Submit(Action action)
        {
            return _busyLoop.Submit(action);
        }

        public void SubmitAndWait(Action action)
        {
            _busyLoop.SubmitAndWait(action);
        }

        public Task SubmitAndWaitAsync(Action action)
        {
            return _busyLoop.SubmitAndWaitAsync(action);
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

            if (TickRunningTime >= tickMaxTime)
            {
                _busyLoop.HandleSingleLoop();
            }
            else
            {
                _busyLoop.Resume();
                _busyLoop.SubmitAndWait(() => !IsRunning || TickRunningTime >= tickMaxTime);
                _busyLoop.Pause();
            }

            while (IsRunning && TickRunningTime < TickMaxTime) { }
        }
    }
}
