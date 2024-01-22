using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLib.TickLoop
{
    public interface ITickUpdatable
    {
        public void OnTickUpdate(int tick);
    }
}
