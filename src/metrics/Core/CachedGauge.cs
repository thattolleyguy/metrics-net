using Metrics.Core;
using Metrics.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// A <see cref="Gauge">Gauge</see> implementation which caches its value for a period of time.
    /// </summary>
    /// <typeparam name="T">the type of the gauge's value</typeparam>
    public class CachedGauge<T> : Gauge<T>
    {

        private readonly Clock clock;
        private readonly AtomicLong reloadAt;
        private readonly long timeoutNS;
        private T value;

        /// <summary>
        /// Creates a new cached gauge with the given timeout period.
        /// </summary>
        /// <param name="timeout">the timeout</param>
        /// <param name="timeoutUnit">the unit of timeout</param>
        /// <param name="evaluator">Method of loading the value</param>
        public CachedGauge(long timeout, TimeUnit timeoutUnit, Func<T> evaluator) : this(Clock.DefaultClock, timeout, timeoutUnit, evaluator)
        {

        }

        /// <summary>
        /// Creates a new cached gauge with the given clock and timeout period.
        /// </summary>
        /// <param name="clock">the clock used to calculate the timeout</param>
        /// <param name="timeout">the timeout</param>
        /// <param name="timeoutUnit">the unit of timeout</param>
        /// <param name="evaluator">Method of loading the value</param>
        public CachedGauge(Clock clock, long timeout, TimeUnit timeoutUnit, Func<T> evaluator) : base(evaluator)
        {
            this.clock = clock;
            this.reloadAt = new AtomicLong(0);
            this.timeoutNS = timeoutUnit.ToNanos(timeout);
        }


        /// <summary>
        /// Loads the value and returns it.
        /// </summary>
        /// <returns>the new value</returns>
        public override T Value
        {
            get
            {
                if (shouldLoad())
                {
                    this.value = this.Value;
                }
                return value;
            }
        }

        private bool shouldLoad()
        {
            for (;;)
            {
                long time = clock.getTick();
                long current = reloadAt.Get();
                if (current > time)
                {
                    return false;
                }
                if (reloadAt.CompareAndSet(current, time + timeoutNS))
                {
                    return true;
                }
            }
        }

    }
}
