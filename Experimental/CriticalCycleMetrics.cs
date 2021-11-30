using System;
using System.Diagnostics;

namespace Prometheus.Experimental
{
    /// <summary>
    /// Exposes timing metrics about a critical data processing cycle.
    /// It is expected that the duration of such a cycle is in the micro/millisecond range.
    /// 
    /// We also include some helpers methods for easy and accurate measurement.
    /// </summary>
    public sealed class CriticalCycleMetrics : IDisposable
    {
        public Histogram.Child IterationDuration { get; }
        public Histogram.Child IntervalDuration { get; }

        private long _iterationStartTimestamp;
        private long _intervalStartTimestamp;

        /// <summary>
        /// Marks the start of an iteration and the end of the interval.
        /// </summary>
        public void IterationStarted()
        {
            // Some CPUs can experience negative time at frequency changes. Use Max to ignore that situation.
            _iterationStartTimestamp = Math.Max(_intervalStartTimestamp, Stopwatch.GetTimestamp());

            if (_intervalStartTimestamp != 0)
                IntervalDuration.Observe(1.0 * (_iterationStartTimestamp - _intervalStartTimestamp) / Stopwatch.Frequency);
        }

        /// <summary>
        /// Marks the end of an iteration and start of the interval.
        /// </summary>
        public void IterationEnded()
        {
            // Some CPUs can experience negative time at frequency changes. Use Max to ignore that situation.
            _intervalStartTimestamp = Math.Max(_iterationStartTimestamp, Stopwatch.GetTimestamp());

            IterationDuration.Observe(1.0 * (_intervalStartTimestamp - _iterationStartTimestamp) / Stopwatch.Frequency);
        }

        public CriticalCycleMetrics(string name)
        {
            IterationDuration = BaseIterationDuration.WithLabels(name);
            IntervalDuration = BaseIntervalDuration.WithLabels(name);
        }

        public void Dispose()
        {
            IterationDuration.Dispose();
            IntervalDuration.Dispose();
        }

        private static readonly Histogram BaseIterationDuration = Metrics.CreateHistogram(
            "critical_cycle_iteration_duration_seconds",
            "Histogram of iteration durations.",
            new HistogramConfiguration
            {
                LabelNames = new[] { "name" },
                Buckets = new[]
                {
                    0.000001,
                    0.000005,
                    0.00001,
                    0.00005,
                    0.0001,
                    0.0002,
                    0.0003,
                    0.0004,
                    0.0005,
                    0.0006,
                    0.0007,
                    0.0008,
                    0.0009,
                    0.001,
                    0.01,
                    0.02,
                    0.03,
                    0.04,
                    0.05
                }
            });

        private static readonly Histogram BaseIntervalDuration = Metrics.CreateHistogram(
            "critical_cycle_interval_duration_seconds",
            "Histogram of durations for the interval between iterations.",
            new HistogramConfiguration
            {
                LabelNames = new[] { "name" },
                Buckets = new[]
                {
                    0.0001,
                    0.0005,
                    0.001,
                    0.005,
                    0.01,
                    0.02,
                    0.03,
                    0.04,
                    0.05,
                    0.06,
                    0.07,
                    0.08,
                    0.09,
                    0.1,
                    1.0
                }
            });
    }
}
