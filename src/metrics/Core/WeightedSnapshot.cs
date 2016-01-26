using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// A single sample item with value and its weights for <see cref="WeightedSnapshot"/>
    /// </summary>
    public class WeightedSample
    {
        public readonly long value;
        public readonly double weight;

        public WeightedSample(long value, double weight)
        {
            this.value = value;
            this.weight = weight;
        }
    }



    class WeightedSampleComparer : IComparer<WeightedSample>
    {
        public int Compare(WeightedSample o1, WeightedSample o2)
        {
            if (o1.value > o2.value)
                return 1;
            if (o1.value < o2.value)
                return -1;
            return 0;
        }

    }

    public class WeightedSnapshot : Snapshot
    {

        private readonly long[] values;
        private readonly double[] normWeights;
        private readonly double[] quantiles;

        /// <summary>
        /// Creates a new <see cref="Snapshot"/> with the given values
        /// </summary>
        /// <param name="values">an unordered set of values in the reservoir</param>
        public WeightedSnapshot(ICollection<WeightedSample> values)
        {
            WeightedSample[] copy = values.ToArray();

            Array.Sort<WeightedSample>(copy, new WeightedSampleComparer());

            this.values = new long[copy.Length];
            this.normWeights = new double[copy.Length];
            this.quantiles = new double[copy.Length];

            double sumWeight = 0;
            foreach (WeightedSample sample in copy)
            {
                sumWeight += sample.weight;
            }

            for (int i = 0; i < copy.Length; i++)
            {
                this.values[i] = copy[i].value;
                this.normWeights[i] = copy[i].weight / sumWeight;
            }

            for (int i = 1; i < copy.Length; i++)
            {
                this.quantiles[i] = this.quantiles[i - 1] + this.normWeights[i - 1];
            }
        }

        /// <summary>
        /// Returns the value at the given quantile
        /// </summary>
        /// <param name="quantile">a given quantile in <c>[0..1]</c></param>
        /// <returns>the value in the distribution at <c>quantile</c></returns>
        public override double GetValue(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0 || Double.IsNaN(quantile))
            {
                throw new ArgumentException(quantile + " is not in [0..1]");
            }

            if (values.Length == 0)
            {
                return 0.0;
            }

            int posx = Array.BinarySearch(quantiles, quantile);
            if (posx < 0)
                posx = ((-posx) - 1) - 1;

            if (posx < 1)
            {
                return values[0];
            }

            if (posx >= values.Length)
            {
                return values[values.Length - 1];
            }

            return values[posx];
        }

        /// <summary>
        /// The number of values in the snapshot
        /// </summary>
        public override int Size
        {
            get { return values.Length; }
        }

        /// <summary>
        /// The entire set of values in the snapshot
        /// </summary>
        public override long[] Values
        {
            get
            {
                long[] dest = new long[values.Length];
                Array.Copy(values, dest, values.Length);
                return dest;
            }
        }

        /// <summary>
        /// The highest value in the snapshot
        /// </summary>
        public override long Max
        {
            get
            {
                if (values.Length == 0)
                {
                    return 0;
                }
                return values[values.Length - 1];
            }
        }

        /// <summary>
        /// The lowest value in the snapshot
        /// </summary>
        public override long Min
        {
            get
            {
                if (values.Length == 0)
                {
                    return 0;
                }
                return values[0];
            }
        }

        /// <summary>
        /// The arithmetic mean of the values in the snapshot
        /// </summary>
        public override double Mean
        {
            get
            {
                if (values.Length == 0)
                {
                    return 0;
                }

                double sum = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    sum += values[i] * normWeights[i];
                }
                return sum;
            }
        }

        /// <summary>
        /// The standard deviation of the values in the snapshot
        /// </summary>
        public override double StdDev
        {
            // two-pass algorithm for variance, avoids numeric overflow
            get
            {
                if (values.Length <= 1)
                {
                    return 0;
                }

                double mean = Mean;
                double variance = 0;

                for (int i = 0; i < values.Length; i++)
                {
                    double diff = values[i] - mean;
                    variance += normWeights[i] * diff * diff;
                }

                return Math.Sqrt(variance);
            }
        }

        /// <summary>
        /// Writes the values of the snapshot to the given stream.
        /// </summary>
        /// <param name="stream">Stream to which the values are written</param>
        public override void dump(Stream output)
        {
            using (StreamWriter writer = new StreamWriter(output))
            {
                foreach (long value in values)
                {
                    writer.WriteLine(value);
                }
            }
        }
    }
}
