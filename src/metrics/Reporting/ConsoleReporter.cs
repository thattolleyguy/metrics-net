using System;
using System.IO;

namespace Metrics.Reporting
{
    
    public class ConsoleReporter : ReporterBase
    {
        public ConsoleReporter(MetricRegistry metrics) : base(Console.Out, metrics)
        {
            
        }

        public ConsoleReporter(IReportFormatter formatter) : base(Console.Out, formatter)
        {

        }
    }
}
