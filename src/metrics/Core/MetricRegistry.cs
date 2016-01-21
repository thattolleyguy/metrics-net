using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Metrics.Core;
using Metrics.Reporting;
using Metrics.Support;
using System.Collections.Immutable;

namespace Metrics
{

    public delegate bool MetricFilter(MetricName name, IMetric metric);

    /// <summary>
    /// A set of factory methods for creating centrally registered metric instances
    /// </summary>
    /// <see href="https://github.com/codahale/metrics"/>
    /// <seealso href="http://codahale.com/codeconf-2011-04-09-metrics-metrics-everywhere.pdf" />
    public class MetricRegistry
    {

        public static MetricFilter ALL = AllMetrics;
        private static bool AllMetrics(MetricName name, IMetric metric)
        {
            return true;
        }

        private readonly ConcurrentDictionary<MetricName, IMetric> metrics;
        private readonly MetricRegistryEventHandler handler;


        public MetricRegistry()
        {
            this.metrics = new ConcurrentDictionary<MetricName, IMetric>();
            this.handler = new MetricRegistryEventHandler();
        }

        public T Register<T>(string name, T metric) where T : IMetric
        {
            return Register(MetricName.build(name), metric);
        }

        public T Register<T>(MetricName name, T metric) where T : IMetric
        {
            if (metric is MetricSet)
            {
                RegisterAll(name, (MetricSet)metric);
            }
            else
            {
                bool exists = metrics.TryAdd(name, metric);
                if (exists)
                {
                    throw new ArgumentException("A metric named " + name + " already exists");

                }
                else
                {
                    OnMetricAdded(name, metric);

                }
            }

            return metric;
        }

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
        
        /// <summary>
        /// Creates a new timer metric and registers it under the given type and name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Timer Timer(String name)
        {
            return Timer(MetricName.build(name));
        }

        public Timer Timer(MetricName name)
        {
            return GetOrAdd(name, new Core.Timer());
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

        public bool Remove(MetricName name)
        {
            IMetric metric = null;
            metrics.TryRemove(name, out metric);
            if (metric != null)
            {
                OnMetricRemoved(name, metric);
                return true;
            }
            return false;
        }

        public void RemoveMatching(MetricFilter filter)
        {
            foreach (KeyValuePair<MetricName, IMetric> pair in metrics)
            {
                if (filter(pair.Key, pair.Value))
                {
                    Remove(pair.Key);
                }
            }
        }

        public ImmutableSortedSet<MetricName> Names
        {
            get
            {
                return metrics.Keys.ToImmutableSortedSet();
            }
        }

        public IDictionary<MetricName, Gauge> getGauges()
        {
            return getGauges(ALL);
        }

        public IDictionary<MetricName, Gauge> getGauges(MetricFilter filter)
        {
            return getMetrics<Gauge>(filter);
        }

        /**
    * Returns a map of all the counters in the registry and their names.
    *
    * @return all the counters in the registry
    */
        public IDictionary<MetricName, Counter> getCounters()
        {
            return getCounters(ALL);
        }

        /**
         * Returns a map of all the counters in the registry and their names which match the given
         * filter.
         *
         * @param filter    the metric filter to match
         * @return all the counters in the registry
         */
        public IDictionary<MetricName, Counter> getCounters(MetricFilter filter)
        {
            return getMetrics<Counter>(filter);
        }

        /**
         * Returns a map of all the histograms in the registry and their names.
         *
         * @return all the histograms in the registry
         */
        public IDictionary<MetricName, Histogram> getHistograms()
        {
            return getHistograms(ALL);
        }

        /**
         * Returns a map of all the histograms in the registry and their names which match the given
         * filter.
         *
         * @param filter    the metric filter to match
         * @return all the histograms in the registry
         */
        public IDictionary<MetricName, Histogram> getHistograms(MetricFilter filter)
        {
            return getMetrics<Histogram>(filter);
        }

        /**
         * Returns a map of all the meters in the registry and their names.
         *
         * @return all the meters in the registry
         */
        public IDictionary<MetricName, Meter> getMeters()
        {
            return getMeters(ALL);
        }

        /**
         * Returns a map of all the meters in the registry and their names which match the given filter.
         *
         * @param filter    the metric filter to match
         * @return all the meters in the registry
         */
        public IDictionary<MetricName, Meter> getMeters(MetricFilter filter)
        {
            return getMetrics<Meter>(filter);
        }

        /**
         * Returns a map of all the timers in the registry and their names.
         *
         * @return all the timers in the registry
         */
        public IDictionary<MetricName, Timer> getTimers()
        {
            return getTimers(ALL);
        }

        /**
         * Returns a map of all the timers in the registry and their names which match the given filter.
         *
         * @param filter    the metric filter to match
         * @return all the timers in the registry
         */
        public IDictionary<MetricName, Timer> getTimers(MetricFilter filter)
        {
            return getMetrics<Timer>(filter);
        }

        private T GetOrAdd<T>(MetricName name, T metric) where T : IMetric
        {
            if (metrics.ContainsKey(name))
            {
                return (T)metrics[name];
            }

            var added = metrics.AddOrUpdate(name, metric, (n, m) => m);

            return added == null ? metric : (T)added;
        }


        private IDictionary<MetricName, T> getMetrics<T>(MetricFilter filter) where T : IMetric
        {
            MetricFilter finalFilter = filter + ((name, metric) => metric is T);
            IDictionary<MetricName, T> retVal = new Dictionary<MetricName, T>();
            foreach (KeyValuePair<MetricName, IMetric> kv in metrics)
            {
                if (finalFilter(kv.Key, kv.Value))
                {

                    retVal.Add(kv.Key, (T)kv.Value);
                }
            }
            return retVal.ToImmutableDictionary();
        }

        public IDictionary<MetricName, IMetric> Metrics
        {
            get { return metrics.ToImmutableDictionary(); }
        }


        public void OnMetricAdded(MetricName name, IMetric metric)
        {
            Type metricType = metric.GetType();
            if (metricType.IsSubclassOf(typeof(Gauge)))
            {
                handler.onGaugeAdded(name, (Gauge)metric);
            }
            else if (metric is Counter)
            {
                handler.onCounterAdded(name, (Counter)metric);
            }
            else if (metric is Histogram)
            {
                handler.onHistogramAdded(name, (Histogram)metric);
            }
            else if (metric is Meter)
            {
                handler.onMeterAdded(name, (Meter)metric);
            }
            else if (metric is Timer)
            {
                handler.onTimerAdded(name, (Timer)metric);
            }
            else
            {
                throw new ArgumentException("Unknown metric type: " + metricType);
            }
        }

        private void OnMetricRemoved(MetricName name, IMetric metric)
        {
            Type metricType = metric.GetType();
            if (metricType.IsSubclassOf(typeof(Gauge)))
            {
                handler.onGaugeRemoved(name);
            }
            else if (metric is Counter)
            {
                handler.onCounterRemoved(name);
            }
            else if (metric is Histogram)
            {
                handler.onHistogramRemoved(name);
            }
            else if (metric is Meter)
            {
                handler.onMeterRemoved(name);
            }
            else if (metric is Timer)
            {
                handler.onTimerRemoved(name);
            }
            else
            {
                throw new ArgumentException("Unknown metric type: " + metricType);
            }
        }

        private void RegisterAll(MetricName prefix, MetricSet metrics)
        {
            if (prefix == null)
                prefix = MetricName.EMPTY;

            foreach (KeyValuePair<MetricName, IMetric> entry in metrics.Metrics)
            {
                if (entry.Value is MetricSet)
                {
                    RegisterAll(MetricName.join(prefix, entry.Key), (MetricSet)entry.Value);
                }
                else
                {
                    Register(MetricName.join(prefix, entry.Key), entry.Value);
                }
            }
        }



        /**
         * Adds a {@link MetricRegistryListener} to a collection of listeners that will be notified on
         * metric creation.  Listeners will be notified in the order in which they are added.
         * <p/>
         * <b>N.B.:</b> The listener will be notified of all existing metrics when it first registers.
         *
         * @param listener the listener that will be notified
         */
        public void AddListener(MetricRegistryListener listener)
        {
            handler.RegisterListener(listener);
            // TODO: Figure out how to notify listener of existing metrics efficiently
            /*for (Map.Entry<MetricName, Metric> entry : metrics.entrySet())
            {
                notifyListenerOfAddedMetric(listener, entry.getValue(), entry.getKey());
            }*/
        }


        /**
         * Removes a {@link MetricRegistryListener} from this registry's collection of listeners.
         *
         * @param listener the listener that will be removed
         */
        public void RemoveListener(MetricRegistryListener listener)
        {
            handler.RemoveListener(listener);
        }

    }

    public interface MetricRegistryListener
    {
        /**
     * Called when a {@link Gauge} is added to the registry.
     *
     * @param name  the gauge's name
     * @param gauge the gauge
     */
        void onGaugeAdded(MetricName name, Gauge gauge);

        /**
         * Called when a {@link Gauge} is removed from the registry.
         *
         * @param name the gauge's name
         */
        void onGaugeRemoved(MetricName name);

        /**
         * Called when a {@link Counter} is added to the registry.
         *
         * @param name    the counter's name
         * @param counter the counter
         */
        void onCounterAdded(MetricName name, Counter counter);

        /**
         * Called when a {@link Counter} is removed from the registry.
         *
         * @param name the counter's name
         */
        void onCounterRemoved(MetricName name);

        /**
         * Called when a {@link Histogram} is added to the registry.
         *
         * @param name      the histogram's name
         * @param histogram the histogram
         */
        void onHistogramAdded(MetricName name, Histogram histogram);

        /**
         * Called when a {@link Histogram} is removed from the registry.
         *
         * @param name the histogram's name
         */
        void onHistogramRemoved(MetricName name);

        /**
         * Called when a {@link Meter} is added to the registry.
         *
         * @param name  the meter's name
         * @param meter the meter
         */
        void onMeterAdded(MetricName name, Meter meter);

        /**
         * Called when a {@link Meter} is removed from the registry.
         *
         * @param name the meter's name
         */
        void onMeterRemoved(MetricName name);

        /**
         * Called when a {@link Timer} is added to the registry.
         *
         * @param name  the timer's name
         * @param timer the timer
         */
        void onTimerAdded(MetricName name, Timer timer);

        /**
         * Called when a {@link Timer} is removed from the registry.
         *
         * @param name the timer's name
         */
        void onTimerRemoved(MetricName name);


    }

    public class MetricRegistryEventHandler
    {
        internal Action<MetricName, Gauge> onGaugeAdded = (name, metric) => { };
        internal Action<MetricName> onGaugeRemoved = (name) => { };
        internal Action<MetricName, Counter> onCounterAdded = (name, metric) => { };
        internal Action<MetricName> onCounterRemoved = (name) => { };
        internal Action<MetricName, Histogram> onHistogramAdded = (name, metric) => { };
        internal Action<MetricName> onHistogramRemoved = (name) => { };
        internal Action<MetricName, Meter> onMeterAdded = (name, metric) => { };
        internal Action<MetricName> onMeterRemoved = (name) => { };
        internal Action<MetricName, Timer> onTimerAdded = (name, metric) => { };
        internal Action<MetricName> onTimerRemoved = (name) => { };

        internal void RegisterListener(MetricRegistryListener listener)
        {
            onGaugeAdded += listener.onGaugeAdded;
            onGaugeRemoved += listener.onGaugeRemoved;
            onCounterAdded += listener.onCounterAdded;
            onCounterRemoved += listener.onCounterRemoved;
            onHistogramAdded += listener.onHistogramAdded;
            onHistogramRemoved += listener.onHistogramRemoved;
            onMeterAdded += listener.onMeterAdded;
            onMeterRemoved += listener.onMeterRemoved;
            onTimerAdded += listener.onTimerAdded;
            onTimerRemoved += listener.onTimerRemoved;
        }
        internal void RemoveListener(MetricRegistryListener listener)
        {
            onGaugeAdded -= listener.onGaugeAdded;
            onGaugeRemoved -= listener.onGaugeRemoved;
            onCounterAdded -= listener.onCounterAdded;
            onCounterRemoved -= listener.onCounterRemoved;
            onHistogramAdded -= listener.onHistogramAdded;
            onHistogramRemoved -= listener.onHistogramRemoved;
            onMeterAdded -= listener.onMeterAdded;
            onMeterRemoved -= listener.onMeterRemoved;
            onTimerAdded -= listener.onTimerAdded;
            onTimerRemoved -= listener.onTimerRemoved;
        }

    }
}
