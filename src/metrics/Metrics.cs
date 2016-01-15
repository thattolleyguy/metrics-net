using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using metrics.Core;
using metrics.Reporting;
using metrics.Support;
using System.Collections.Immutable;

namespace metrics
{
    /// <summary>
    /// A set of factory methods for creating centrally registered metric instances
    /// </summary>
    /// <see href="https://github.com/codahale/metrics"/>
    /// <seealso href="http://codahale.com/codeconf-2011-04-09-metrics-metrics-everywhere.pdf" />
    public class MetricRegistry : IDisposable
    {
        private readonly ConcurrentDictionary<MetricName, IMetric> _metrics;

        // TODO: Support registry listeners
        //private readonly List<MetricRegistryListener> listeners;


        public MetricRegistry()
        {
            this._metrics = new ConcurrentDictionary<MetricName, IMetric>();
        }

        public T Register<T>(string name, T metric) where T : IMetric
        {
            return Register(MetricName.build(name), metric);
        }

        public T Register<T>(MetricName name, T metric) where T : IMetric
        {
            //if (metric.GetType().IsAssignableFrom(typeof(MetricSet))) {
            //        registerAll(name, (MetricSet)metric);
            //    } else {
            IMetric existing = metrics.PutIfAbsent(name, metric);
            if (existing == null)
            {
                onMetricAdded(name, metric);
            }
            else {
                throw new ArgumentException("A metric named " + name + " already exists");
            }
            //}

            return metric;
        }

        /**
            * Given a metric set, registers them.
            *
            * @param metrics    a set of metrics
            * @throws IllegalArgumentException if any of the names are already registered
            */
        /* public void registerAll(MetricSet metrics)
         {
             registerAll(null, metrics);
         }*/

        /// <summary>
        /// Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public Counter Counter(string name)
        {
            return Counter(MetricName.build(name));
        }

        /// <summary>
        /// Creates a new counter metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public Counter Counter(MetricName name)
        {
            return GetOrAdd(name, new Counter());
        }


        /// <summary>
        /// Creates a new non-biased histogram metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public Histogram Histogram(string name)
        {
            return Histogram(MetricName.build(name));
        }

        public Histogram Histogram(MetricName name)
        {
            return GetOrAdd(name, new Histogram(new ExponentiallyDecayingReservoir()));
        }


        /// <summary>
        /// Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns></returns>
        public Meter Meter(string name)
        {
            return Meter(MetricName.build(name));
        }

        public Meter Meter(MetricName name)
        {
            return GetOrAdd(name, new Meter());
        }
        /**
            * @see #timer(MetricName)
            */
        public Timer Timer(String name)
        {
            return timer(MetricName.build(name));
        }

        /**
         * Return the {@link Timer} registered under this name; or create and register
         * a new {@link Timer} if none is registered.
         *
         * @param name the name of the metric
         * @return a new or pre-existing {@link Timer}
         */
        public Timer Timer(MetricName name)
        {
            return GetOrAdd(name, new Core.Timer());
        }





        /// <summary>
        /// ( convenience method for installing a gauge that is bound to a <see cref="PerformanceCounter" />
        /// </summary>
        /// <param name="category">The performance counter category</param>
        /// <param name="counter">The performance counter name</param>
        /// <param name="instance">The performance counter instance, if applicable</param>
        /// <param name="label">A label to distinguish the metric in polling reports</param>
        public void InstallPerformanceCounterGauge(string category, string counter, string instance, string label)
        {
            var performanceCounter = new PerformanceCounter(category, counter, instance, true);
            GetOrAdd(new MetricName(Environment.MachineName + label), new Gauge<double>(() => performanceCounter.NextValue()));
        }

        /// <summary>
        /// A convenience method for installing a gauge that is bound to a <see cref="PerformanceCounter" />
        /// </summary>
        /// <param name="category">The performance counter category</param>
        /// <param name="counter">The performance counter name</param>
        /// <param name="label">A label to distinguish the metric in polling reports</param>
        public void InstallPerformanceCounterGauge(string category, string counter, string label)
        {
            var performanceCounter = new PerformanceCounter(category, counter, true);
            GetOrAdd(new MetricName(Environment.MachineName + label), new Gauge<double>(() => performanceCounter.NextValue()));
        }

        /// <summary>
        /// Creates a new gauge metric and registers it under the given type and name
        /// </summary>
        /// <typeparam name="T">The type the gauge measures</typeparam>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The gauge evaluation function</param>
        /// <returns></returns>
        public Gauge<T> Gauge<T>(string name, Func<T> evaluator)
        {
            return GetOrAdd(new MetricName(name), new Gauge<T>(evaluator));
        }

        /**
 * Removes the metric with the given name.
 *
 * @param name the name of the metric
 * @return whether or not the metric was removed
 */
        public bool remove(MetricName name)
        {
            IMetric metric = null;
            _metrics.TryRemove(name, out metric);
            if (metric != null)
            {
                
                onMetricRemoved(name, metric);
                return true;
            }
            return false;
        }

        /**
         * Removes all metrics which match the given filter.
         *
         * @param filter a filter
         */
        // TODO: Support for metric filter
        /* public void removeMatching(MetricFilter filter)
         {
             for (Map.Entry<MetricName, Metric> entry : metrics.entrySet())
             {
                 if (filter.matches(entry.getKey(), entry.getValue()))
                 {
                     remove(entry.getKey());
                 }
             }
         }*/

        /**
         * Adds a {@link MetricRegistryListener} to a collection of listeners that will be notified on
         * metric creation.  Listeners will be notified in the order in which they are added.
         * <p/>
         * <b>N.B.:</b> The listener will be notified of all existing metrics when it first registers.
         *
         * @param listener the listener that will be notified
         */
        // TODO: Metric registry listener
        /* public void addListener(MetricRegistryListener listener)
         {
             listeners.add(listener);

             for (Map.Entry<MetricName, Metric> entry : metrics.entrySet())
             {
                 notifyListenerOfAddedMetric(listener, entry.getValue(), entry.getKey());
             }
         }*/


        /**
         * Removes a {@link MetricRegistryListener} from this registry's collection of listeners.
         *
         * @param listener the listener that will be removed
         */
        /*public void removeListener(MetricRegistryListener listener)
        {
            listeners.remove(listener);
        }*/

        /**
         * Returns a set of the names of all the metrics in the registry.
         *
         * @return the names of all the metrics
         */
        public IEnumerable<MetricName> getNames()
        {
            return new SortedSet<MetricName>(_metrics.Keys).ToImmutableSortedSet();
        }

        /**
         * Returns a map of all the gauges in the registry and their names.
         *
         * @return all the gauges in the registry
         */
        public SortedDictionary<MetricName, Gauge> getGauges()
        {
            return getGauges(MetricFilter.ALL);
        }







        /// <summary>
        /// Creates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public CallbackTimerMetric CallbackTimer(String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            var metricName = new MetricName(name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (CallbackTimerMetric)existingMetric;
            }

            var metric = new CallbackTimerMetric(durationUnit, rateUnit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (CallbackTimerMetric)justAddedMetric;
        }

        /// <summary>
        /// Creates a new metric that can be used to add manual timings into the system. A manual timing
        /// is a timing that is measured not by the metrics system but by the client site and must be added
        /// into metrics as an additional measurement.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="durationUnit">The duration scale unit of the new timer</param>
        /// <param name="rateUnit">The rate unit of the new timer</param>
        /// <returns></returns>
        public ManualTimerMetric ManualTimer(String name, TimeUnit durationUnit, TimeUnit rateUnit)
        {
            var metricName = new MetricName(name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (ManualTimerMetric)existingMetric;
            }

            var metric = new ManualTimerMetric(durationUnit, rateUnit);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (ManualTimerMetric)justAddedMetric;
        }


        /// <summary>
        /// Creates a new meter metric and registers it under the given type and name
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="eventType">The plural name of the type of events the meter is measuring (e.g., <code>"requests"</code>)</param>
        /// <param name="unit">The rate unit of the new meter</param>
        /// <param name="rate">The rate  of the new meter</param>
        /// <returns></returns>
        public PerSecondCounterMetric TimedCounter(string name, string eventType)
        {
            var metricName = new MetricName(name);
            IMetric existingMetric;
            if (_metrics.TryGetValue(metricName, out existingMetric))
            {
                return (PerSecondCounterMetric)existingMetric;
            }

            var metric = PerSecondCounterMetric.New(eventType);
            var justAddedMetric = _metrics.GetOrAdd(metricName, metric);
            return justAddedMetric == null ? metric : (PerSecondCounterMetric)justAddedMetric;
        }
        /// <summary>
        /// Enables the console reporter and causes it to print to STDOUT with the specified period
        /// </summary>
        /// <param name="period">The period between successive outputs</param>
        /// <param name="unit">The time unit of the period</param>
        public void EnableConsoleReporting(long period, TimeUnit unit)
        {
            var reporter = new ConsoleReporter(this);
            EnableReporting(reporter, period, unit);
        }

        /// <summary>
        ///  Enables a reporter to run with the specified interval between outputs
        /// </summary>
        /// <param name="reporter"></param>
        /// <param name="period">The period between successive outputs</param>
        /// <param name="unit">The time unit of the period</param>
        public void EnableReporting(ReporterBase reporter, long period, TimeUnit unit)
        {
            reporter.Start(period, unit);
        }

        /// <summary>
        /// Returns a copy of all currently registered metrics in an immutable collection
        /// </summary>
        public IDictionary<MetricName, IMetric> All
        {
            get { return _metrics.ToImmutableDictionary(); }
        }

        /// <summary>
        /// Clears all previously registered metrics
        /// </summary>
        public void Clear()
        {
            _metrics.Clear();
            PerformanceCounter.CloseSharedResources();
        }

        private T GetOrAdd<T>(MetricName name, T metric) where T : IMetric
        {
            if (_metrics.ContainsKey(name))
            {
                return (T)_metrics[name];
            }

            var added = _metrics.AddOrUpdate(name, metric, (n, m) => m);

            return added == null ? metric : (T)added;
        }

        public void Dispose()
        {
            foreach (var metric in _metrics)
            {
                using (metric.Value as IDisposable)
                {

                }
            }
        }
    }
}
