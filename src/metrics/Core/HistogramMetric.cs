using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using metrics.Stats;
using metrics.Support;
using System.IO;

namespace metrics.Core
{
    /// <summary>
    /// A metric which calculates the distribution of a value
    /// <see href="http://www.johndcook.com/standard_deviation.html">Accurately computing running variance</see>
    /// </summary>
    public class Histogram : IMetric
    {
        private readonly Reservoir reservoir;
        private readonly AtomicLong count;


        /// <summary>
        /// Creates a new <see cref="Histogram" /> with the given sample type
        /// </summary>
        public Histogram(Reservoir reservoir)
        {
            this.reservoir = reservoir;
            this.count = new AtomicLong(0);
        }

        /// <summary>
        /// Adds a recorded value
        /// </summary>
        public void Update(int value)
        {
            Update((long)value);
        }

        /// <summary>
        /// Adds a recorded value
        /// </summary>
        public void Update(long value)
        {
            count.IncrementAndGet();
            reservoir.Update(value);
        }



        /// <summary>
        /// Returns the number of values recorded
        /// </summary>
        public long Count { get { return count.Get(); } }

        public Snapshot getSnapshot()
        {
            return reservoir.GetSnapshot();
        }
    }
}
