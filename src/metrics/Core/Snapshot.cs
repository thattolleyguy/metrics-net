using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace metrics.Core
{
    public abstract class Snapshot
    {
        public abstract double GetValue(double quantile);
        public abstract long[] Values { get; }
        public abstract int Size { get; }
        public double Median
        {
            get { return GetValue(.5); }
        }
        public double Percentile75th
        {
            get { return GetValue(.75); }
        }

        public double Percentile95th
        {
            get { return GetValue(.95); }
        }
        public double Percentile98th
        {
            get { return GetValue(.98); }
        }
        public double Percentile99th
        {
            get { return GetValue(.99); }
        }
        public double Percentile999th
        {
            get { return GetValue(.999); }
        }

        public abstract long Max { get; }
        public abstract long Mean { get; }
        public abstract long Min { get; }
        public abstract long StdDev { get; }
        public abstract void dump(Stream stream);
    }
}
