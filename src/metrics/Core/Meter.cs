using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Stats;
using Metrics.Support;
using System.Text;

namespace Metrics.Core
{
    /// <summary>
    /// A meter metric which measures mean throughput and one-, five-, and fifteen-minute exponentially-weighted moving average throughputs.
    /// </summary>
    /// <see href="http://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average">EMA</see>
    public class Meter : IMetered,IMetric
    {
        private static readonly long TICK_INTERVAL = TimeUnit.Seconds.ToNanos(5);

        private EWMA _m1Rate = EWMA.OneMinuteEWMA();
        private EWMA _m5Rate = EWMA.FiveMinuteEWMA();
        private EWMA _m15Rate = EWMA.FifteenMinuteEWMA();

        private AtomicLong _count = new AtomicLong();
        private AtomicLong _lastTick;
        private long _startTime = DateTime.Now.Ticks;
        


        private readonly CancellationTokenSource _token = new CancellationTokenSource();
        private Clock clock;

        public Meter():
            this(Clock.DEFAULT)
        {
              
        }

        public Meter(Clock clock)
        {
            this.clock = clock;
            this._startTime = this.clock.getTick();
            this._lastTick = new AtomicLong(_startTime);
        }

        /// <summary>
        /// Returns the type of events the meter is measuring
        /// </summary>
        /// <returns></returns>
        public string EventType { get; private set; }


        /// <summary>
        /// Mark the occurrence of an event
        /// </summary>
        public void Mark()
        {
            Mark(1);
        }

        /// <summary>
        /// Mark the occurrence of a given number of events
        /// </summary>
        public void Mark(long n)
        {
            tickIfNecessary();
            _count.AddAndGet(n);
            _m1Rate.Update(n);
            _m5Rate.Update(n);
            _m15Rate.Update(n);
        }

        private void tickIfNecessary()
        {
            long oldTick = _lastTick.Get();
            long newTick = clock.getTick();
            long age = newTick - oldTick;
            if(age>TICK_INTERVAL)
            {
                long newIntervalStartTick = newTick - age % TICK_INTERVAL;
                if(_lastTick.CompareAndSet(oldTick,newIntervalStartTick))
                {
                    long requiredTicks = age / TICK_INTERVAL;
                    for (long i = 0; i < requiredTicks;i++)
                    {
                        _m1Rate.Tick();
                        _m5Rate.Tick();
                        _m15Rate.Tick();
                    }

                }
            }
        }

        /// <summary>
        ///  Returns the number of events which have been marked
        /// </summary>
        /// <returns></returns>
        public long Count
        {
            get { return _count.Get(); }
        }

        /// <summary>
        /// Returns the fifteen-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the fifteen-minute load
        /// average in the top Unix command.
        /// </remarks> 
        /// </summary>
        public double FifteenMinuteRate
        {
            get
            {
                tickIfNecessary();
                return _m15Rate.Rate(TimeUnit.Seconds);
            }
        }

        /// <summary>
        /// Returns the five-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the five-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        public double FiveMinuteRate
        {
            get
            {
                tickIfNecessary();
                return _m5Rate.Rate(TimeUnit.Seconds);
            }
        }



        /// <summary>
        /// Returns the mean rate at which events have occured since the meter was created
        /// </summary>
        public double MeanRate
        {
            get
            {
                if (Count != 0)
                {
                    var elapsed = (clock.getTick() - _startTime); // 1 DateTime Tick == 100ns
                    return Count / (double)elapsed * TimeUnit.Seconds.ToNanos(1);
                }
                return 0.0;
            }
        }

        /// <summary>
        /// Returns the one-minute exponentially-weighted moving average rate at
        /// which events have occured since the meter was created
        /// <remarks>
        /// This rate has the same exponential decay factor as the one-minute load
        /// average in the top Unix command.
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public double OneMinuteRate
        {
            get
            {
                tickIfNecessary();
                return _m1Rate.Rate(TimeUnit.Seconds);
            }
        }




    }

    public abstract class Clock
    {
        public abstract long getTick();

        public static readonly Clock DEFAULT = new UserTimeClock();
        public long getTime()
        {

            // TODO: FIX THIS
            return 0;
        }
    }

    public class UserTimeClock : Clock
    {
        public override long getTick()
        {
            return DateTime.Now.Ticks*100;
        }
    }
}