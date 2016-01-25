using Metrics.Support;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Core
{
    public interface Reservoir
    {
        /// <summary>
        /// Returns the number of values recorded
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Adds a new recorded value to the reservoir
        /// </summary>
        /// <param name="value">A new recorded value</param>
        void Update(long value);

        /// <summary>
        /// Returns a snapshot of the reservoir's value
        /// </summary>
        Snapshot Snapshot { get; }
    }
}
