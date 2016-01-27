using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// An abstraction for how time passes. It is passed to <c>Timer</c> to track timing.
    /// </summary>
    public abstract class Clock
    {
        /// <summary>
        /// Returns the current time in nanoseconds.
        /// </summary>
        /// <returns>the current time in nanoseconds</returns>
        public abstract long getTick();


        /// <summary>
        /// Returns the current time in milliseconds.
        /// </summary>
        /// <returns>time in milliseconds</returns>
        public long CurrentTime
        {
            get { return TimeUnit.Ticks.ToMillis(DateTime.Now.Ticks); }
        }

        private static readonly Clock DEFAULT = new UserTimeClock();
        /// <summary>
        /// The default clock to use
        /// </summary>
        /// <returns>the default clock instance</returns>
        public static Clock DefaultClock
        {
            get { return DEFAULT; }

        }
    }

    /// <summary>
    /// A clock implementation that returns the current ticks in nanoseconds.
    /// </summary>
    public class UserTimeClock : Clock
    {
        /// <summary>
        /// This returns the current ticks in nanoseconds. This means that the resolution of this clock is at the tick level, not the nanosecond level
        /// </summary>
        /// <returns>Current time in nanoseconds</returns>
        public override long getTick()
        {
            return DateTime.Now.Ticks * 100;
        }
    }
}
