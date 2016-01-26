using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{

    /// <summary>
    /// A statistical snapshot
    /// </summary>
    public abstract class Snapshot
    {
        /// <summary>
        /// Returns the value at the given quantile
        /// </summary>
        /// <param name="quantile">a given quantile in <c>[0..1]</c></param>
        /// <returns>the value in the distribution at <c>quantile</c></returns>
        public abstract double GetValue(double quantile);

        /// <summary>
        /// The entire set of values in the snapshot
        /// </summary>
        public abstract long[] Values { get; }

        /// <summary>
        /// The number of values in the snapshot
        /// </summary>
        public abstract int Size { get; }

        /// <summary>
        /// The median value in the distribution
        /// </summary>
        public double Median
        {
            get { return GetValue(.5); }
        }

        /// <summary>
        /// The value at the 75th percentile in the distribution
        /// </summary>
        public double Percentile75th
        {
            get { return GetValue(.75); }
        }

        /// <summary>
        /// The value at the 95th percentile in the distribution
        /// </summary>
        public double Percentile95th
        {
            get { return GetValue(.95); }
        }

        /// <summary>
        /// The value at the 98th percentile in the distribution
        /// </summary>
        public double Percentile98th
        {
            get { return GetValue(.98); }
        }

        /// <summary>
        /// The value at the 99th percentile in the distribution
        /// </summary>
        public double Percentile99th
        {
            get { return GetValue(.99); }
        }

        /// <summary>
        /// The value at the 99.9th percentile in the distribution
        /// </summary>
        public double Percentile999th
        {
            get { return GetValue(.999); }
        }

        /// <summary>
        /// The highest value in the snapshot
        /// </summary>
        public abstract long Max { get; }

        /// <summary>
        /// The arithmetic mean of the values in the snapshot
        /// </summary>
        public abstract double Mean { get; }

        /// <summary>
        /// The lowest value in the snapshot
        /// </summary>
        public abstract long Min { get; }

        /// <summary>
        /// The standard deviation of the values in the snapshot
        /// </summary>
        public abstract double StdDev { get; }

        /// <summary>
        /// Writes the values of the snapshot to the given stream.
        /// </summary>
        /// <param name="stream">Stream to which the values are written</param>
        public abstract void dump(Stream stream);
    }
}
