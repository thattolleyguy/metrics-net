using System;
using Metrics.Support;

namespace Metrics.Stats
{
    /// <summary>
    ///  An exponentially-weighted moving average
    /// </summary>
    /// <see href="http://www.teamquest.com/pdfs/whitepaper/ldavg1.pdf">UNIX Load Average Part 1: How It Works</see>
    /// <see href="http://www.teamquest.com/pdfs/whitepaper/ldavg2.pdf">UNIX Load Average Part 2: Not Your Average Average</see>
    public class EWMA
    {
        private static readonly int INTERVAL = 5;
        private static readonly double SECONDS_PER_MINUTE = 60.0;
        private static readonly int ONE_MINUTE = 1;
        private static readonly int FIVE_MINUTES = 5;
        private static readonly int FIFTEEN_MINUTES = 15;

        private static readonly double M1Alpha = 1 - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / ONE_MINUTE);
        private static readonly double M5Alpha = 1 - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / FIVE_MINUTES);
        private static readonly double M15Alpha = 1 - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / FIFTEEN_MINUTES);

        private volatile bool _initialized;
        private VolatileDouble _rate;

        private readonly AtomicLong _uncounted = new AtomicLong(0);
        private readonly double _alpha, _interval;

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX one minute load average and which expects to be ticked every 5 seconds.
        /// </summary>
        public static EWMA OneMinuteEWMA()
        {
            return new EWMA(M1Alpha, INTERVAL, TimeUnit.Seconds);
        }

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX five minute load average and which expects to be ticked every 5 seconds.
        /// </summary>
        /// <returns></returns>
        public static EWMA FiveMinuteEWMA()
        {
            return new EWMA(M5Alpha, INTERVAL, TimeUnit.Seconds);
        }

        /// <summary>
        ///  Creates a new EWMA which is equivalent to the UNIX fifteen minute load average and which expects to be ticked every 5 seconds.
        /// </summary>
        /// <returns></returns>
        public static EWMA FifteenMinuteEWMA()
        {
            return new EWMA(M15Alpha, INTERVAL, TimeUnit.Seconds);
        }

        /// <summary>
        /// Create a new EWMA with a specific smoothing constant.
        /// </summary>
        /// <param name="alpha">The smoothing constant</param>
        /// <param name="interval">The expected tick interval</param>
        /// <param name="intervalUnit">The time unit of the tick interval</param>
        public EWMA(double alpha, long interval, TimeUnit intervalUnit)
        {
            _interval = intervalUnit.ToNanos(interval);
            _alpha = alpha;
        }

        /// <summary>
        ///  Update the moving average with a new value.
        /// </summary>
        /// <param name="n"></param>
        public void Update(long n)
        {
            _uncounted.AddAndGet(n);
        }

        /// <summary>
        /// Mark the passage of time and decay the current rate accordingly.
        /// </summary>
        public void Tick()
        {
            var count = _uncounted.GetAndSet(0);
            var instantRate = count / _interval;
            if (_initialized)
            {
                _rate += _alpha * (instantRate - _rate);
            }
            else
            {
                _rate.Set(instantRate);
                _initialized = true;
            }
        }

        /// <summary>
        /// Returns the rate in the given units of time.
        /// </summary>
        public double Rate(TimeUnit rateUnit)
        {
            var nanos = rateUnit.ToNanos(1);
            return _rate * nanos;
        }
    }
}
