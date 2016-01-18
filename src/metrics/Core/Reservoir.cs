using Metrics.Support;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Core
{
    public interface Reservoir
    {
        int Size();
        void Update(long value);
        Snapshot GetSnapshot();
    }

    /// <summary>
    /// An exponentially-decaying random sample of {@code long}s. Uses Cormode et
    /// al's forward-decaying priority reservoir sampling method to produce a
    /// statistically representative sample, exponentially biased towards newer
    /// entries.
    /// </summary>
    /// <see href="http://www.research.att.com/people/Cormode_Graham/library/publications/CormodeShkapenyukSrivastavaXu09.pdf">
    /// Cormode et al. Forward Decay: A Practical Time Decay Model for Streaming
    /// Systems. ICDE '09: Proceedings of the 2009 IEEE International Conference on
    /// Data Engineering (2009)
    /// </see>
    public class ExponentiallyDecayingReservoir : Reservoir
    {
        private static readonly int DEFAULT_SIZE = 1028;
        private static readonly double DEFAULT_ALPHA = 0.015;
        private static readonly long RESCALE_THRESHOLD = TimeUnit.Hours.ToNanos(1);

        //TODO: Find skip list
        private readonly ConcurrentDictionary<double, WeightedSample> _values;
        private readonly ReaderWriterLockSlim _lock;
        private readonly double _alpha;
        private readonly int _size;
        private readonly AtomicLong _count;
        private VolatileLong _startTime;
        private readonly AtomicLong _nextScaleTime;
        private readonly Clock clock;

        public ExponentiallyDecayingReservoir() : this(DEFAULT_SIZE, DEFAULT_ALPHA)
        {
        }


        /// <param name="reservoirSize">The number of samples to keep in the sampling reservoir</param>
        /// <param name="alpha">The exponential decay factor; the higher this is, the more biased the sample will be towards newer values</param>
        public ExponentiallyDecayingReservoir(int reservoirSize, double alpha) :
            this(reservoirSize, alpha, Clock.DEFAULT)
        {
        }

        public ExponentiallyDecayingReservoir(int size, double alpha, Clock clock)
        {
            _values = new ConcurrentDictionary<double, WeightedSample>();
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _alpha = alpha;
            _size = size;
            this.clock = clock;
            this._count = new AtomicLong(0);
            this._startTime = CurrentTimeInSeconds();
            this._nextScaleTime = new AtomicLong(clock.getTick() + RESCALE_THRESHOLD);

        }

        public int Size()
        {
            return _size;
        }

        /// <summary>
        /// Adds a new recorded value to the sample
        /// </summary>
        public void Update(long value)
        {
            Update(value, CurrentTimeInSeconds());
        }



        private void Update(long value, long timestamp)
        {
            rescaleIfNeeded();
            lockForRegularUsage();
            _lock.EnterReadLock();
            try
            {
                var itemWeight = Weight(timestamp - _startTime);
                WeightedSample sample = new WeightedSample(value, itemWeight);
                var random = ThreadLocalRandom.NextNonzeroDouble();
                var priority = itemWeight / random;

                var newCount = _count.IncrementAndGet();

                if (newCount <= _size)
                {
                    _values.AddOrUpdate(priority, sample, (p, v) => v);
                }
                else
                {
                    var first = _values.Keys.Min();
                    if (first < priority)
                    {
                        _values.AddOrUpdate(priority, sample, (p, v) => v);

                        WeightedSample removed;
                        while (!_values.TryRemove(first, out removed))
                        {
                            first = _values.Keys.First();
                        }
                    }
                }
            }
            finally
            {
                unlockForRegularUsage();
                _lock.ExitReadLock();
            }
        }
        private void rescaleIfNeeded()
        {

            var now = clock.getTick();
            var next = _nextScaleTime.Get();
            if (now >= next)
            {
                Rescale(now, next);
            }
        }

        public Snapshot GetSnapshot()
        {
            lockForRegularUsage();
            try
            {
                return new WeightedSnapshot(_values.Values);
            }
            finally
            {
                unlockForRegularUsage();
            }
        }



        private long CurrentTimeInSeconds()
        {
            return TimeUnit.Milliseconds.ToSeconds(clock.getTime());
        }

        private double Weight(long t)
        {
            return Math.Exp(_alpha * t);
        }


        /// <summary>
        /// "A common feature of the above techniques—indeed, the key technique that
        /// allows us to track the decayed weights efficiently—is that they maintain
        /// counts and other quantities based on g(ti − L), and only scale by g(t − L)
        /// at query time. But while g(ti −L)/g(t−L) is guaranteed to lie between zero
        /// and one, the intermediate values of g(ti − L) could become very large. For
        /// polynomial functions, these values should not grow too large, and should be
        /// effectively represented in practice by floating point values without loss of
        /// precision. For exponential functions, these values could grow quite large as
        /// new values of (ti − L) become large, and potentially exceed the capacity of
        /// common floating point types. However, since the values stored by the
        /// algorithms are linear combinations of g values (scaled sums), they can be
        /// rescaled relative to a new landmark. That is, by the analysis of exponential
        /// decay in Section III-A, the choice of L does not affect the final result. We
        /// can therefore multiply each value based on L by a factor of exp(−α(L′ − L)),
        /// and obtain the correct value as if we had instead computed relative to a new
        /// landmark L′ (and then use this new L′ at query time). This can be done with
        /// a linear pass over whatever data structure is being used."
        /// </summary>
        /// <param name="now"></param>
        /// <param name="next"></param>
        private void Rescale(long now, long next)
        {
            if (_nextScaleTime.CompareAndSet(next, now + RESCALE_THRESHOLD))
            {
                lockForRescale();
                try
                {
                    var oldStartTime = _startTime;
                    _startTime = CurrentTimeInSeconds();
                    double scalingFactor = Math.Exp(-_alpha * (_startTime - oldStartTime));

                    var keys = new List<double>(_values.Keys);
                    foreach (double key in keys)
                    {
                        WeightedSample sample = null;
                        if (_values.TryRemove(key, out sample))
                        {
                            WeightedSample newSample = new WeightedSample(sample.value, sample.weight * scalingFactor);
                            _values.AddOrUpdate(key * scalingFactor, newSample, (k, v) => v);
                        }
                    }
                }
                finally
                {
                    unlockForRescale();
                }
            }
        }

        private void unlockForRescale()
        {
            _lock.EnterWriteLock();
        }

        private void lockForRescale()
        {
            _lock.ExitWriteLock();
        }

        private void lockForRegularUsage()
        {
            _lock.EnterReadLock();
        }

        private void unlockForRegularUsage()
        {
            _lock.ExitReadLock();
        }
    }
}
