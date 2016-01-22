using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    public abstract class Clock
    {
        public abstract long getTick();

        public static readonly Clock DEFAULT = new UserTimeClock();
        public long getTime()
        {
            return DateTime.Now.Ticks / 10;
        }
    }

    public class UserTimeClock : Clock
    {
        public override long getTick()
        {
            return DateTime.Now.Ticks * 100;
        }
    }
}
