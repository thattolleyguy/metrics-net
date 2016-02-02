using Ninject;
using Ninject.Extensions.Interception.Request;
using Ninject.Extensions.Interception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Metrics.Reporting;
using Metrics.Reporting.Graphite;

namespace Metrics.Ninject
{
    public class Tryout
    {
        public static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel();
            MetricRegistry registry = new MetricRegistry();
            kernel.Bind<MetricRegistry>().ToConstant<MetricRegistry>(registry);


            Tryout t = kernel.Get<Tryout>();
            ConsoleReporter reporter = ConsoleReporter.ForRegistry(registry).build();
            reporter.Start(1, TimeUnit.Seconds);

            Graphite sender = new Graphite("ttolley-lap3", 2003);
            GraphiteReporter greporter = GraphiteReporter.ForRegistry(registry).Build(sender);
            greporter.Start(10, TimeUnit.Seconds);

            int i = 0;
            Random r = new Random();
            for (; i < 10000; i++)
            {
                t.Test(r.Next(101));
            }

            Console.WriteLine("Done counting");
            for (i = 0; i < 10; i++)
            {
                Thread.Sleep(60000);
            }


        }

        [Metered(Name = "Attributes.TestMeterAttribute", Absolute = true)]
        [Counted(Name = "Attributes.TestCountAttribute", Absolute = true)]
        [Timed(Name = "Attributes.TestTimerAttribute", Absolute = true)]
        public virtual void Test(int timeout)
        {
                Thread.Sleep(timeout);

        }
    }
}
