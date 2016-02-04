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

        private CountingInterceptor interceptor = null;

        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            if (interceptor == null)
            {
                MetricName metricName = Utils.BuildName(request, Name, Absolute);
                MetricRegistry registry = request.Context.Kernel.Get<MetricRegistry>();
                Counter counter = registry.Counter(metricName);
                interceptor = new CountingInterceptor(counter, Monotonic);
            }
            return interceptor;
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
