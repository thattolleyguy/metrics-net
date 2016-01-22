using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{
    /// <summary>
    /// An interface for metrics types which have counts
    /// </summary>
    public interface ICounted
    {
        /// <summary>
        /// Returns the current count
        /// </summary>
        long Count { get; }
    }
}
