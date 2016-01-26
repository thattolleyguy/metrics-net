using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Core
{

    public interface MetricRegistryListener
    {
        /// <summary>
        /// Called when a <see cref="Gauge"/> is added to the registry
        /// </summary>
        /// <param name="name">the gauge's name</param>
        /// <param name="gauge">the gauge</param>
        void onGaugeAdded(MetricName name, Gauge gauge);

        /// <summary>
        /// Called when a <see cref="Gauge"/> is removed from the registry
        /// </summary>
        /// <param name="name">the gauge's name</param>
        void onGaugeRemoved(MetricName name);

        /// <summary>
        /// Called when a <see cref="Counter"/> is added to the registry
        /// </summary>
        /// <param name="name">the counter's name</param>
        /// <param name="counter">the counter</param>
        void onCounterAdded(MetricName name, Counter counter);

        /// <summary>
        /// Called when a <see cref="Counter"/> is removed from the registry
        /// </summary>
        /// <param name="name">the counter's name</param>
        void onCounterRemoved(MetricName name);

        /// <summary>
        /// Called when a <see cref="Histogram"/> is added to the registry
        /// </summary>
        /// <param name="name">the histogram's name</param>
        /// <param name="histogram">the histogram</param>
        void onHistogramAdded(MetricName name, Histogram histogram);

        /// <summary>
        /// Called when a <see cref="Histogram"/> is removed from the registry
        /// </summary>
        /// <param name="name">the histogram's name</param>
        void onHistogramRemoved(MetricName name);

        /// <summary>
        /// Called when a <see cref="Meter"/> is added to the registry
        /// </summary>
        /// <param name="name">the meter's name</param>
        /// <param name="meter">the meter</param>
        void onMeterAdded(MetricName name, Meter meter);

        /// <summary>
        /// Called when a <see cref="Meter"/> is removed from the registry
        /// </summary>
        /// <param name="name">the meter's name</param>
        void onMeterRemoved(MetricName name);

        /// <summary>
        /// Called when a <see cref="Timer"/> is added to the registry
        /// </summary>
        /// <param name="name">the timer's name</param>
        /// <param name="timer">the timer</param>
        void onTimerAdded(MetricName name, Timer timer);

        /// <summary>
        /// Called when a <see cref="Timer"/> is removed from the registry
        /// </summary>
        /// <param name="name">the timer's name</param>
        void onTimerRemoved(MetricName name);


    }


}
