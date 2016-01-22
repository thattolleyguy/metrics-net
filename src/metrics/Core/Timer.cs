using System;
using System.Diagnostics;
using System.Runtime.Serialization;


namespace Metrics.Core
{

    /// <summary>
    /// A timer metric which aggregates timing durations and provides duration
    /// statistics, plus throughput statistics via <see cref="Meter" />.
    /// </summary>
    public class Timer : IMetric, IMetered, ISampling
    {
        private readonly Meter meter;
        private readonly Histogram histogram;
        private readonly Clock clock;

        public Timer() : this(new ExponentiallyDecayingReservoir())
        {
        }

        public Timer(Reservoir reservoir) : this(reservoir, Clock.DEFAULT)
        {

        }

        public Timer(Reservoir reservoir, Clock clock)
        {
            this.meter = new Meter(clock);
            this.clock = clock;
            this.histogram = new Histogram(reservoir);
        }

        public void Update(long duration, TimeUnit unit)
        {
            update(unit.ToNanos(duration));
        }
        private void update(long duration)
        {
            if (duration >= 0)
            {
                histogram.Update(duration);
                meter.Mark();

            }
        }

        public void Time(Action action)
        {
            long startTime = clock.getTick();
            try
            {
                action();
            }
            finally
            {
                
                update(this.clock.getTick() - startTime);
            }
        }

        public T Time<T>(Func<T> action)
        {
            long startTime = clock.getTick();
            try
            {
                return action();
            }
            finally
            {
                update(this.clock.getTick() - startTime);
            }
        }

        public Context Time()
        {
            return new Context(this, clock);
        }

        public long Count { get { return histogram.Count; } }
        public double FifteenMinuteRate { get { return meter.FifteenMinuteRate; } }

        public double FiveMinuteRate { get { return meter.FiveMinuteRate; } }

        public double MeanRate { get { return meter.MeanRate; } }

        public double OneMinuteRate { get { return meter.OneMinuteRate; } }

        public Snapshot Snapshot { get { return histogram.Snapshot; } }

        public class Context : IDisposable
        {
            private readonly Timer timer;
            private readonly Clock clock;
            private readonly long startTime;

            internal Context(Timer timer, Clock clock)
            {
                this.timer = timer;
                this.clock = clock;
                this.startTime = clock.getTick();
            }

            public long stop()
            {
                long elapsed = clock.getTick() - startTime;
                timer.Update(elapsed, TimeUnit.Nanoseconds);
                return elapsed;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        stop();
                    }


                    disposedValue = true;
                }
            }

            // ~Context() {
            //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            //   Dispose(false);
            // }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
            #endregion

        }

    }
}