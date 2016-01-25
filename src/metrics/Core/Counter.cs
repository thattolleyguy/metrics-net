using System.Runtime.Serialization;
using System.Text;
using Metrics.Support;

namespace Metrics.Core
{
    /// <summary>
    /// An incrementing and decrementing counter metric.
    /// </summary>
    public sealed class Counter : IMetric,ICounted
    {
        private readonly AtomicLong _count;

        public Counter()
        {
             _count = new AtomicLong(0);
        }

        /// <summary>
        /// Increment the counter by one
        /// </summary>
        public void Increment()
        {
            Increment(1);
        }
        /// <summary>
        /// Increment the counter by amount
        /// </summary>
        /// <param name="amount">the amount by which the counter will be increased</param>
        public void Increment(long amount)
        {
            _count.AddAndGet(amount);
        }

        /// <summary>
        /// Decrement the counter by one
        /// </summary>
        public void Decrement()
        {
            Decrement(1);
        }

        /// <summary>
        /// Decrement the counter by amount
        /// </summary>
        /// <param name="amount">the amount by which the counter will be decreased</param>
        public void Decrement(long amount)
        {
            _count.AddAndGet(0 - amount);
        }
        /// <summary>
        /// Returns the counter's current value
        /// </summary>
        public long Count
        {
            get { return _count.Get(); }
        }
    }
}