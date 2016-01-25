using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// A gauge whose value is derived from the value of another gauge
    /// </summary>
    /// <typeparam name="F">The base gauge's value type</typeparam>
    /// <typeparam name="T">The derivative type</typeparam>
    public class DerivativeGauge<F, T> : Gauge<T>
    {
        /// <summary>
        /// Creates a new derivative with the given base gauge
        /// </summary>
        /// <param name="baseGauge">the gague from which to derive this gauge's value</param>
        /// <param name="transform">transform function</param>
        public DerivativeGauge(Gauge<F> baseGauge, Func<F, T> transform) : base(() => transform(baseGauge.Value))
        {

        }

    }
}
