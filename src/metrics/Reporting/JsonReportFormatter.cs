using metrics.Util;

namespace metrics.Reporting
{
    public class JsonReportFormatter : IReportFormatter
    {
        private readonly MetricRegistry _metrics;

        public JsonReportFormatter(MetricRegistry metrics)
        {
            _metrics = metrics;
        }

        public string GetSample()
        {
            return Serializer.Serialize(_metrics.AllSorted);
        }
    }
}