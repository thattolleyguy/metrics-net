using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metrics.Core
{
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

        /**
         * A single sample item with value and its weights for {@link WeightedSnapshot}.
         */


        //private static readonly Charset UTF_8 = Charset.forName("UTF-8");

        private readonly long[] values;
        private readonly double[] normWeights;
        private readonly double[] quantiles;

        /**
         * Create a new {@link Snapshot} with the given values.
         *
         * @param values    an unordered set of values in the reservoir
         */
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

        /**
         * Returns the value at the given quantile.
         *
         * @param quantile    a given quantile, in {@code [0..1]}
         * @return the value in the distribution at {@code quantile}
         */
        public double getValue(double quantile)
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

        /**
         * Returns the number of values in the snapshot.
         *
         * @return the number of values
         */
        public int size()
        {
            return values.Length;
        }

        /**
         * Returns the entire set of values in the snapshot.
         *
         * @return the entire set of values
         */

        public long[] getValues()
        {
            long[] dest = new long[values.Length];
            Array.Copy(values, dest, values.Length);
            return dest;
        }

        /**
         * Returns the highest value in the snapshot.
         *
         * @return the highest value
         */

        public long getMax()
        {
            if (values.Length == 0)
            {
                return 0;
            }
            return values[values.Length - 1];
        }

        /**
         * Returns the lowest value in the snapshot.
         *
         * @return the lowest value
         */

        public long getMin()
        {
            if (values.Length == 0)
            {
                return 0;
            }
            return values[0];
        }

        /**
         * Returns the weighted arithmetic mean of the values in the snapshot.
         *
         * @return the weighted arithmetic mean
         */

        public double getMean()
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

        /**
         * Returns the weighted standard deviation of the values in the snapshot.
         *
         * @return the weighted standard deviation value
         */

        public double getStdDev()
        {
            // two-pass algorithm for variance, avoids numeric overflow

            if (values.Length <= 1)
            {
                return 0;
            }

            double mean = getMean();
            double variance = 0;

            for (int i = 0; i < values.Length; i++)
            {
                double diff = values[i] - mean;
                variance += normWeights[i] * diff * diff;
            }

            return Math.Sqrt(variance);
        }

        /**
         * Writes the values of the snapshot to the given stream.
         *
         * @param output an output stream
         */

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
