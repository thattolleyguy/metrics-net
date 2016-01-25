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


    /// <summary>
    /// A set of factory methods for creating centrally registered metric instances
    /// </summary>
    /// <see href="https://github.com/codahale/metrics"/>
    /// <seealso href="http://codahale.com/codeconf-2011-04-09-metrics-metrics-everywhere.pdf" />
    public class MetricRegistry
    {

        private readonly ConcurrentDictionary<MetricName, IMetric> _metrics;
        private readonly MetricRegistryEventHandler handler;


        public MetricRegistry() : this(new ConcurrentDictionary<MetricName, IMetric>())
        {
        }

        /// <summary>
        /// Creates a <see cref="MetricRegistry"/> with a custom <see cref="ConcurrentDictionary{TKey, TValue}"/>
        /// implementation for use inside the registry. Call as the super-constructor to create a <see cref="MetricRegistry"/>
        /// with space- or time-bounded metric lifecycles, for example.
        /// </summary>
        /// <param name="metricsMap"></param>
        protected MetricRegistry(ConcurrentDictionary<MetricName, IMetric> metricsMap)
        {
            this._metrics = metricsMap;
            this.handler = new MetricRegistryEventHandler();
        }

        /// <summary>
        /// Given an <see cref="IMetric"/>, registers it under the given name
        /// </summary>
        /// <typeparam name="T">the type of the metric</typeparam>
        /// <param name="name">the name of the metric</param>
        /// <param name="metric">the metric</param>
        /// <returns><see cref="IMetric"/></returns>
        public T Register<T>(string name, T metric) where T : IMetric
        {
            return Register(MetricName.build(name), metric);
        }

        /// <summary>
        /// Given an <see cref="IMetric"/>, registers it under the given name
        /// </summary>
        /// <typeparam name="T">the type of the metric</typeparam>
        /// <param name="name">the name of the metric</param>
        /// <param name="metric">the metric</param>
        /// <returns><see cref="IMetric"/></returns>
        public T Register<T>(MetricName name, T metric) where T : IMetric
        {
            if (metric is MetricSet)
            {
                RegisterAll(name, (MetricSet)metric);
            }
            else
            {
                bool added = _metrics.TryAdd(name, metric);
                if (added)
                {
                    OnMetricAdded(name, metric);
                }
                else
                {
                    throw new ArgumentException("A metric named " + name + " already exists");

                }
            }

            return metric;
        }

        /// <summary>
        /// Return the <see cref="Core.Counter"/> registered under this name; or create and register
        /// a new <see cref="Core.Counter"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Counter"/></returns>
        public Counter Counter(string name)
        {
            return Counter(MetricName.build(name));
        }

        /// <summary>
        /// Return the <see cref="Core.Counter"/> registered under this name; or create and register
        /// a new <see cref="Core.Counter"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Counter"/></returns>
        public Counter Counter(MetricName name)
        {
            return GetOrAdd(name, new Counter());
        }


        /// <summary>
        /// Return the <see cref="Core.Histogram"/> registered under this name; or create and register
        /// a new <see cref="Core.Histogram"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Histogram"/></returns>
        public Histogram Histogram(string name)
        {
            return Histogram(MetricName.build(name));
        }
        /// <summary>
        /// Return the <see cref="Core.Histogram"/> registered under this name; or create and register
        /// a new <see cref="Core.Histogram"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Histogram"/></returns>
        public Histogram Histogram(MetricName name)
        {
            return GetOrAdd(name, new Histogram(new ExponentiallyDecayingReservoir()));
        }


        /// <summary>
        /// Return the <see cref="Core.Meter"/> registered under this name; or create and register
        /// a new <see cref="Core.Meter"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Meter"/></returns>
        public Meter Meter(string name)
        {
            return Meter(MetricName.build(name));
        }
        /// <summary>
        /// Return the <see cref="Core.Meter"/> registered under this name; or create and register
        /// a new <see cref="Core.Meter"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Meter"/></returns>
        public Meter Meter(MetricName name)
        {
            return GetOrAdd(name, new Meter());
        }

        /// <summary>
        /// Return the <see cref="Core.Timer"/> registered under this name; or create and register
        /// a new <see cref="Core.Timer"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Timer"/></returns>
        public Timer Timer(String name)
        {
            return Timer(MetricName.build(name));
        }
        /// <summary>
        /// Return the <see cref="Core.Timer"/> registered under this name; or create and register
        /// a new <see cref="Core.Timer"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <returns>a new or pre-existing <see cref="Core.Timer"/></returns>
        public Timer Timer(MetricName name)
        {
            return GetOrAdd(name, new Core.Timer());
        }

        /// <summary>
        /// Return the <see cref="Core.Gauge"/> registered under this name; or create and register
        /// a new <see cref="Core.Gauge"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The evaluator for the gauge</param>
        /// <returns>a new or pre-existing <see cref="Core.Gauge"/></returns>
        public Gauge<T> Gauge<T>(string name, Func<T> evaluator)
        {
            return Gauge(new MetricName(name), evaluator);
        }

        /// <summary>
        /// Return the <see cref="Core.Gauge"/> registered under this name; or create and register
        /// a new <see cref="Core.Gauge"/> if none is registered.
        /// </summary>
        /// <param name="name">The metric name</param>
        /// <param name="evaluator">The evaluator for the gauge</param>
        /// <returns>a new or pre-existing <see cref="Core.Gauge"/></returns>
        public Gauge<T> Gauge<T>(MetricName name, Func<T> evaluator)
        {
            return GetOrAdd(name, new Gauge<T>(evaluator));
        }

        /// <summary>
        /// Removes the metric with the given name.
        /// </summary>
        /// <param name="name">the name of the metric</param>
        /// <returns>whether or not the metric was removed</returns>
        public bool Remove(MetricName name)
        {
            IMetric metric = null;
            _metrics.TryRemove(name, out metric);
            if (metric != null)
            {
                OnMetricRemoved(name, metric);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all metrics which match the given filter.
        /// </summary>
        /// <param name="filter">a filter</param>
        public void RemoveMatching(MetricFilter filter)
        {
            foreach (KeyValuePair<MetricName, IMetric> pair in _metrics)
            {
                if (filter(pair.Key, pair.Value))
                {
                    Remove(pair.Key);
                }
            }
        }


        /// <summary>
        /// The set of the names of all the metrics in the registry.
        /// </summary>
        public ImmutableSortedSet<MetricName> Names
        {
            get
            {
                return _metrics.Keys.ToImmutableSortedSet();
            }
        }

        /// <summary>
        /// Returns a map of all the gauges in the registry and their names.
        /// </summary>
        /// <returns>all the gauges in the registry</returns>
        public IDictionary<MetricName, Gauge> getGauges()
        {
            return getGauges(MetricFilters.ALL);
        }

        /// <summary>
        /// Returns a map of all the gauges in the registry and their names which match the given filter.
        /// </summary>
        /// <param name="filter">the metric filter to match</param>
        /// <returns>the matching gauges in the registry</returns>
        public IDictionary<MetricName, Gauge> getGauges(MetricFilter filter)
        {
            return getMetrics<Gauge>(filter);
        }

        /// <summary>
        /// Returns a map of all the counters in the registry and their names.
        /// </summary>
        /// <returns>all the counters in the registry</returns>
        public IDictionary<MetricName, Counter> getCounters()
        {
            return getCounters(MetricFilters.ALL);
        }
        /// <summary>
        /// Returns a map of all the counters in the registry and their names which match the given filter.
        /// </summary>
        /// <param name="filter">the metric filter to match</param>
        /// <returns>the matching counters in the registry</returns>
        public IDictionary<MetricName, Counter> getCounters(MetricFilter filter)
        {
            return getMetrics<Counter>(filter);
        }

        /// <summary>
        /// Returns a map of all the histograms in the registry and their names.
        /// </summary>
        /// <returns>all the histograms in the registry</returns>
        public IDictionary<MetricName, Histogram> getHistograms()
        {
            return getHistograms(MetricFilters.ALL);
        }

        /// <summary>
        /// Returns a map of all the histograms in the registry and their names which match the given filter.
        /// </summary>
        /// <param name="filter">the metric filter to match</param>
        /// <returns>the matching histograms in the registry</returns>
        public IDictionary<MetricName, Histogram> getHistograms(MetricFilter filter)
        {
            return getMetrics<Histogram>(filter);
        }

        /// <summary>
        /// Returns a map of all the meters in the registry and their names.
        /// </summary>
        /// <returns>all the meters in the registry</returns>
        public IDictionary<MetricName, Meter> getMeters()
        {
            return getMeters(MetricFilters.ALL);
        }

        /// <summary>
        /// Returns a map of all the meters in the registry and their names which match the given filter.
        /// </summary>
        /// <param name="filter">the metric filter to match</param>
        /// <returns>the matching meters in the registry</returns>
        public IDictionary<MetricName, Meter> getMeters(MetricFilter filter)
        {
            return getMetrics<Meter>(filter);
        }

        /// <summary>
        /// Returns a map of all the timers in the registry and their names.
        /// </summary>
        /// <returns>all the timers in the registry</returns>
        public IDictionary<MetricName, Timer> getTimers()
        {
            return getTimers(MetricFilters.ALL);
        }

        /// <summary>
        /// Returns a map of all the timers in the registry and their names which match the given filter.
        /// </summary>
        /// <param name="filter">the metric filter to match</param>
        /// <returns>the matching timers in the registry</returns>
        public IDictionary<MetricName, Timer> getTimers(MetricFilter filter)
        {
            return getMetrics<Timer>(filter);
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


        private IDictionary<MetricName, T> getMetrics<T>(MetricFilter filter) where T : IMetric
        {
            MetricFilter finalFilter = filter + ((name, metric) => metric is T);
            IDictionary<MetricName, T> retVal = new Dictionary<MetricName, T>();
            foreach (KeyValuePair<MetricName, IMetric> kv in _metrics)
            {
                if (finalFilter(kv.Key, kv.Value))
                {

                    retVal.Add(kv.Key, (T)kv.Value);
                }
            }
            return retVal.ToImmutableDictionary();
        }

        /// <summary>
        /// Get all Metrics in the registry
        /// </summary>
        public IDictionary<MetricName, IMetric> Metrics
        {
            get { return _metrics.ToImmutableDictionary(); }
        }


        private void OnMetricAdded(MetricName name, IMetric metric)
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

        private void RegisterAll(MetricName prefix, MetricSet metricSet)
        {
            if (prefix == null)
                prefix = MetricName.EMPTY;

            foreach (KeyValuePair<MetricName, IMetric> entry in metricSet.Metrics)
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



        /// <summary>
        /// Adds a <see cref="MetricRegistryListener"/> to a collection of listeners that will be notified on
        /// metric creation.Listeners will be notified in the order in which they are added.
        /// <para />
        /// The listener will be notified of all existing metrics when it first registers.
        /// </summary>
        /// <param name="listener"></param>
        public void AddListener(MetricRegistryListener listener)
        {
            handler.RegisterListener(listener);
            // TODO: Figure out how to notify listener of existing metrics efficiently
            foreach (KeyValuePair<MetricName, IMetric> entry in this._metrics)
            {
                MetricName name = entry.Key;
                IMetric metric = entry.Value;
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
                    continue;
                }
            }
        }


        /// <summary>
        /// Removes a <see cref="MetricRegistryListener"/> from this registry's collection of listeners.
        /// </summary>
        /// <param name="listener">the listener that will be removed</param>
        public void RemoveListener(MetricRegistryListener listener)
        {
            handler.RemoveListener(listener);
        }

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