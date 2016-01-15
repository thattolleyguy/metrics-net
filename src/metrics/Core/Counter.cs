using System.Runtime.Serialization;
using System.Text;
using metrics.Support;

namespace metrics.Core
{
    /// <summary>
    /// An atomic counter metric
    /// </summary>
    public sealed class Counter : IMetric
    {
        private readonly AtomicLong _count;

        public Counter()
        {
             _count = new AtomicLong(0);
        }

        public void Increment()
        {
            Increment(1);
        }

        public void Increment(long amount)
        {
            _count.AddAndGet(amount);
        }

        public void Decrement()
        {
            Decrement(1);
        }

        public void Decrement(long amount)
        {
            _count.AddAndGet(0 - amount);
        }

        public long Count
        {
            get { return _count.Get(); }
        }
    }
}