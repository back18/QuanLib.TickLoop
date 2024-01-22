using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop.StateMachine
{
    public delegate bool StateGotoHandler<TState>(TState sourceState, TState targetState);
}
