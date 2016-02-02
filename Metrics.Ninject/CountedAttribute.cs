using Ninject.Extensions.Interception.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Request;
using Metrics.Core;
using Ninject;

namespace Metrics.Ninject
{
    public class CountedAttribute : InterceptAttribute
    {
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public bool Absolute { get; set; }
        public bool Monotonic { get; set; }


        private Counter counter = null;



        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            if (counter == null)
            {
                MetricName metricName = null;
                if (Absolute)
                    metricName = new MetricName(Name);
                else
                    metricName = new MetricName(request.Target.GetType().FullName + "." + Name);

                MetricRegistry registry = request.Context.Kernel.Get<MetricRegistry>();
                counter = registry.Counter(metricName);
            }
            return new CountingInterceptor(counter, Monotonic);
        }
    }

    public class CountingInterceptor : IInterceptor
    {
        private readonly Counter counter;
        private readonly bool monotonic;

        public CountingInterceptor(Counter counter, bool monotonic)
        {
            this.counter = counter;
            this.monotonic = monotonic;
        }

        public void Intercept(IInvocation invocation)
        {
            this.counter.Increment();
            try
            {
                invocation.Proceed();
            }
            finally
            {
                if (monotonic)
                    this.counter.Decrement();
            }
        }
    }
}
