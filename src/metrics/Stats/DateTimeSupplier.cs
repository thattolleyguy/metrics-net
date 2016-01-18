using System;

namespace Metrics.Stats
{
    internal class DateTimeSupplier : IDateTimeSupplier
    {
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }
}