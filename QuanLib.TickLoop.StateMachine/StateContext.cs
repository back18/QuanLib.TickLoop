using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop.StateMachine
{
    public class StateContext<TState> where TState : Enum
    {
        public StateContext(TState state, IList<TState> allowableStates, StateGotoHandler<TState> stateGotoHandler, TickUpdateHandler? updateHandler = null)
        {
            ArgumentNullException.ThrowIfNull(state, nameof(state));
            ArgumentNullException.ThrowIfNull(allowableStates, nameof(allowableStates));
            ArgumentNullException.ThrowIfNull(stateGotoHandler, nameof(stateGotoHandler));

            State = state;
            AllowableStates = allowableStates.AsReadOnly();
            StateGotoHandler = stateGotoHandler;
            UpdateHandler = updateHandler ?? ((tick) => { });
        }

        public TState State { get; }

        public ReadOnlyCollection<TState> AllowableStates { get; }

        internal StateGotoHandler<TState> StateGotoHandler { get; }

        internal TickUpdateHandler UpdateHandler { get; }
    }
}
