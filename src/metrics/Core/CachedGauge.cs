using Metrics.Core;
using Metrics.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    public class CachedGauge<T> : Gauge<T>
    {

        private readonly Clock clock;
        private readonly AtomicLong reloadAt;
        private readonly long timeoutNS;
        private T value;

        /// <summary>
        /// Creates a new cached gauge with the given timeout period.
        /// </summary>
        /// <param name="timeout"> the timeout</param>
        /// <param name="timeoutUnit">the unit of { @code timeout }</param>
        /// <param name="evaluator"></param>


        protected CachedGauge(long timeout, TimeUnit timeoutUnit, Func<T> evaluator) : this(Clock.DEFAULT, timeout, timeoutUnit, evaluator)
        {

        }

        /**
         * Creates a new cached gauge with the given clock and timeout period.
         *
         * @param clock          the clock used to calculate the timeout
         * @param timeout        the timeout
         * @param timeoutUnit    the unit of {@code timeout}
         */
        protected CachedGauge(Clock clock, long timeout, TimeUnit timeoutUnit, Func<T> evaluator) : base(evaluator)
        {
            this.clock = clock;
            this.reloadAt = new AtomicLong(0);
            this.timeoutNS = timeoutUnit.ToNanos(timeout);
        }

        /**
         * Loads the value and returns it.
         *
         * @return the new value
         */


        public T getValue()
        {
            if (shouldLoad())
            {
                this.value = this.Value;
            }
            return value;
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
