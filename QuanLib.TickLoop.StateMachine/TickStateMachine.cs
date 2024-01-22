using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop.StateMachine
{
    public class TickStateMachine<TState> : ITickUpdatable where TState : Enum
    {
        public TickStateMachine(TState initialState, IList<StateContext<TState>> stateContexts)
        {
            ArgumentNullException.ThrowIfNull(initialState, nameof(initialState));
            ArgumentNullException.ThrowIfNull(stateContexts, nameof(stateContexts));

            CurrentState = initialState;
            _stateContexts = stateContexts.ToDictionary(item => item.State, item => item);
            _stateQueue = new();
        }

        private readonly Queue<TState> _stateQueue;

        private readonly Dictionary<TState, StateContext<TState>> _stateContexts;

        public TState CurrentState { get; private set; }

        public TState NextState
        {
            get
            {
                if (_stateQueue.TryPeek(out var state))
                    return state;
                else
                    return CurrentState;
            }
        }

        public void Submit(TState state)
        {
            lock (_stateQueue)
            {
                _stateQueue.Enqueue(state);
            }
        }

        public void OnTickUpdate(int tick)
        {
            lock (_stateQueue)
            {
                while (_stateQueue.TryPeek(out var state))
                {
                    if (_stateContexts.TryGetValue(state, out var stateContext) &&
                        stateContext.AllowableStates.Contains(CurrentState) &&
                        stateContext.StateGotoHandler(CurrentState, state))
                    {
                        CurrentState = state;
                    }

                    _stateQueue.Dequeue();
                }

                if (_stateContexts.TryGetValue(CurrentState, out var currentStateContext))
                    currentStateContext.UpdateHandler.Invoke(tick);
            }
        }
    }
}
