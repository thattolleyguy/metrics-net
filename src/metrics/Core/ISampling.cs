using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// An object which samples values
    /// </summary>
    public interface ISampling
    {
        /// <summary>
        /// Returns a snapshot of the values
        /// </summary>
        Snapshot Snapshot { get; }
    }
}
