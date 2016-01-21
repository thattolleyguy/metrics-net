using System;
using System.Runtime.Serialization;
using System.Text;

namespace Metrics.Core
{
    /// <summary>
    /// An untyped version of a gauge for reporting purposes
    /// </summary>
    public abstract class Gauge : IMetric
    {
        private Type type;
        public Gauge(Type type)
        {
            this.type = type;
        }
        public abstract string ValueAsString { get; }
    }

    /// <summary>
    /// A gauge metric is an instantaneous reading of a particular value. To
    /// instrument a queue's depth, for example:
    /// <example>
    /// <code> 
    /// var queue = new Queue{int}();
    /// var gauge = new GaugeMetric{int}(() => queue.Count);
    /// </code>
    /// </example>
    /// </summary>
    public class Gauge<T> : Gauge
    {
        private readonly Func<T> _evaluator;

        public Gauge(Func<T> evaluator) : base(typeof(T))
        {
            _evaluator = evaluator;
        }

        public T Value
        {
            get { return _evaluator.Invoke(); }
        }

        public override string ValueAsString
        {
            get { return Value.ToString(); }
        }

        [IgnoreDataMember]
        public IMetric Copy
        {
            get { return new Gauge<T>(_evaluator); }
        }

        public void LogJson(StringBuilder sb)
        {
            sb.Append("{\"value\":").Append(Value).Append("}");

        }
    }
}