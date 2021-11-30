using System.Diagnostics;

namespace Prometheus.Experimental
{
    /// <summary>
    /// Publishes metrics that carry the local time, potentially allowing time synchronization problems to be diagnosed.
    /// </summary>
    public sealed class LocalTimeMetrics
    {
        public static void Register(CollectorRegistry registry = null)
        {
            registry = registry ?? Metrics.DefaultRegistry;
            var factory = Metrics.WithCustomRegistry(registry);

            // Unix time can arbitrarily change as the machine's clock is adjusted.
            var unixTime = factory.CreateGauge("unixtime_seconds", "Local time in Unix timestamp format.");

            // Stopwatch time should always tick 1 second per second.
            var swTime = factory.CreateCounter("elapsed_seconds_total", "Seconds elapsed since app startup.");

            var stopwatch = Stopwatch.StartNew();

            registry.AddBeforeCollectCallback(delegate
            {
                unixTime.SetToCurrentTimeUtc();

                // Some CPUs can experience negative time at frequency changes. Our metric stands still at such moments.
                swTime.IncTo(stopwatch.Elapsed.TotalSeconds);
            });
        }
    }
}
