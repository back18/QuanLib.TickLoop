using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop
{
    public enum TickTaskState
    {
        NotStarted,

        Running,

        Completed,

        Failed
    }
}
