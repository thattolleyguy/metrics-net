using System;

namespace Metrics.Stats
{
    public interface IDateTimeSupplier
    {
        DateTime UtcNow { get; }
    }
}