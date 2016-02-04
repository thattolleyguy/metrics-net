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
    public class ExceptionMeteredAttribute : InterceptAttribute
    {
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public bool Absolute { get; set; }

        public Type ExceptionType { get; set; }

        private ExceptionMeteringInterceptor meterInterceptor = null;

        public override IInterceptor CreateInterceptor(IProxyRequest request)
        {
            if (meterInterceptor == null)
            {
                MetricName metricName = Utils.BuildName(request, Name, Absolute);
                MetricRegistry registry = request.Context.Kernel.Get<MetricRegistry>();
                Meter meter = registry.Meter(metricName);
                Type t = typeof(Exception);
                if (ExceptionType!=null && ExceptionType.IsSubclassOf(t))
                    t = ExceptionType;
                meterInterceptor = new ExceptionMeteringInterceptor(meter, t);

            }


            return meterInterceptor;
        }
    }

    public class ExceptionMeteringInterceptor : IInterceptor
    {
        private readonly Meter meter;
        private readonly Type exceptionType;

        public ExceptionMeteringInterceptor(Meter meter, Type exceptionType)
        {
            this.meter = meter;
            this.exceptionType = exceptionType;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception e)
            {
                if (exceptionType.IsAssignableFrom(e.GetType()))
                    meter.Mark();
                throw e;
            }
        }
    }
}
