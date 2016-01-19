using System;
using System.Text;
using System.Threading;
using Metrics.Core;
using Metrics.Reporting;

namespace Metrics.Tests
{
    public class Tryout
    {
        static void Main(string[] args)
        {
            var db1Metrics = new MetricRegistry();
            var reporter = ConsoleReporter.ForRegistry(db1Metrics).build();
            var meter = db1Metrics.Meter("testMeter");
            var randomHist = db1Metrics.Histogram("testHist");
            
            reporter.start(1, TimeUnit.Seconds);




            //var docsTimedCounterPerSec = db1Metrics.TimedCounter("db1", "docs new indexed/sec", "new Indexed Documents");
            int i = 0;
            db1Metrics.Gauge<int>("testGauge",()=>i);
            Random r = new Random();
            var counter = db1Metrics.Counter("testCounter");
            for (; i < 1000; i++)
            {
                meter.Mark();
                counter.Increment(i);
                randomHist.Update(r.Next(100));
                Thread.Sleep(100);
            }
            //Console.WriteLine(docsTimedCounterPerSec.CurrentValue);

            /*var RequestsPerSecondHistogram = db1Metrics.Histogram("db1.Request Per Second Histogram");
            for (int i = 0; i < 100; i++)
            {
                RequestsPerSecondCounter.Mark();
                RequestsPerSecondHistogram.Update((long)RequestsPerSecondCounter.CurrentValue);
                Thread.Sleep(10);
            }
            StringBuilder sb = new StringBuilder();
            double[] res;
            var perc = RequestsPerSecondHistogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999);
            res = perc;
            RequestsPerSecondHistogram.LogJson(sb,perc);
            Console.WriteLine(sb);
            Console.WriteLine(RequestsPerSecondHistogram.Percentiles(0.5, 0.75, 0.95, 0.98, 0.99, 0.999));
           // RequestsPerSecondHistogram.Update((long)documentDatabase.WorkContext.MetricsCounters.RequestsPerSecondCounter.CurrentValue); //??
           */
        }
    }
}