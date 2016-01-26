using System;
using System.Diagnostics;
using System.Runtime.Serialization;


namespace Metrics.Core
{

    /// <summary>
    /// A timer metric which aggregates timing durations and provides duration
    /// statistics, plus throughput statistics via <see cref="Meter" />.
    /// </summary>
    public class Timer : IMetric, IMetered, ISampling, ICounted
    {

        /// <summary>
        /// A timing context
        /// </summary>
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

            /// <summary>
            /// Updates the timer with the difference between current and start time. Call to this method will
            /// not reset the start time. Multiple calls result in multiple updates.
            /// </summary>
            /// <returns>the elapsed time in nanoseconds</returns>
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

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
            #endregion

        }


        private readonly Meter meter;
        private readonly Histogram histogram;
        private readonly Clock clock;

        /// <summary>
        /// Creates a new <see cref="Timer"/> using an <see cref="ExponentiallyDecayingReservoir"/> and the default <see cref="Clock"/>
        /// </summary>
        public Timer() : this(new ExponentiallyDecayingReservoir())
        {
        }

        /// <summary>
        /// Creats a new <see cref="Timer"/> that uses the given <see cref="Reservoir"/> and the default <see cref="Clock"/>
        /// </summary>
        /// <param name="reservoir">the <see cref="Reservoir"/> implementation the timer should use</param>
        public Timer(Reservoir reservoir) : this(reservoir, Clock.DefaultClock)
        {

        }

        /// <summary>
        /// Creates a new <see cref="Timer"/> that uses the given <see cref="Reservoir"/> and <see cref="Clock"/>
        /// </summary>
        /// <param name="reservoir">the <see cref="Reservoir"/> implementation the timer should use</param>
        /// <param name="clock">the <see cref="Clock"/> implementation the timer should use</param>
        public Timer(Reservoir reservoir, Clock clock)
        {
            this.meter = new Meter(clock);
            this.clock = clock;
            this.histogram = new Histogram(reservoir);
        }

        /// <summary>
        /// Adds a recorded duration
        /// </summary>
        /// <param name="duration">the length of the duration</param>
        /// <param name="unit">the scale unit of the duration</param>
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

        /// <summary>
        /// Times and records the duration of an event
        /// </summary>
        /// <param name="action">a <see cref="Action"/> whose <see cref="Action.Invoke"/> is timed</param>
        public void Time(Action action)
        {
            long startTime = clock.getTick();
            try
            {
                action.Invoke();
            }
            finally
            {

                update(this.clock.getTick() - startTime);
            }
        }

        /// <summary>
        /// Times and records the duration of an event
        /// </summary>
        /// <typeparam name="T">The type of the value returned by <c>action</c></typeparam>
        /// <param name="action">a <see cref="Func{TResult}"/> whose <see cref="Func{TResult}.Invoke"/> is timed </param>
        /// <returns>the value returned by <c>action</c></returns>
        public T Time<T>(Func<T> action)
        {
            long startTime = clock.getTick();
            try
            {
                return action.Invoke();
            }
            finally
            {
                update(this.clock.getTick() - startTime);
            }
        }

        /// <summary>
        /// Returns a new <see cref="Context"/>
        /// </summary>
        /// <returns></returns>
        public Context Time()
        {
            return new Context(this, clock);
        }
        
        /// <summary>
        ///  Returns the number of events which have been marked
        /// </summary>
        /// <returns></returns>
        public long Count { get { return histogram.Count; } }
        
        /// <summary>
        /// Returns the fifteen-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the fifteen-minute load
        /// average in the top Unix command.
        /// </remarks> 
        /// </summary>
        public double FifteenMinuteRate { get { return meter.FifteenMinuteRate; } }
        
        /// <summary>
        /// Returns the five-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the five-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        public double FiveMinuteRate { get { return meter.FiveMinuteRate; } }
        
        /// <summary>
        /// Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        public double MeanRate { get { return meter.MeanRate; } }
       
        /// <summary>
        /// Returns the one-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the one-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        public double OneMinuteRate { get { return meter.OneMinuteRate; } }

        /// <summary>
        /// Returns a snapshot of the reservoir's value
        /// </summary>
        public Snapshot Snapshot { get { return histogram.Snapshot; } }



    }
}