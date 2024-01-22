using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop
{
    public class TickTask
    {
        public TickTask(Action action)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            _action = action;
            _semaphore = new(0);
            _task = WaitSemaphoreAsync();
            State = TickTaskState.NotStarted;
        }

        private readonly SemaphoreSlim _semaphore;

        private readonly Task _task;

        private readonly Action _action;

        public TickTaskState State { get; private set; }

        public Exception? Exception { get; private set; }

        public void Start()
        {
            try
            {
                State = TickTaskState.Running;
                _action.Invoke();
                State = TickTaskState.Completed;
            }
            catch (Exception ex)
            {
                Exception = ex;
                State = TickTaskState.Failed;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task WaitForCompleteAsync()
        {
            await _task;
        }

        private async Task WaitSemaphoreAsync()
        {
            await _semaphore.WaitAsync();
        }
    }
}
