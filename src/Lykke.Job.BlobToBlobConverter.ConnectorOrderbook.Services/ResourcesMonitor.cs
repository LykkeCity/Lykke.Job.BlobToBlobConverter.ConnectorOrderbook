using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Common;
using Common.Log;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class ResourcesMonitor : TimerPeriod
    {
        private const double _1mb = 1024 * 1024;
        private const string _cpuMetric = "Custom CPU";
        private const string _ramMetric = "Custom RAM";

        private readonly TelemetryClient _telemetryClient = new TelemetryClient();
        private readonly Process _process = Process.GetCurrentProcess();
        private readonly int _processorCount = Environment.ProcessorCount;
        private readonly Stopwatch _cpuWatch = new Stopwatch();

        private TimeSpan? _startCpuTime;

        public ResourcesMonitor(ILog log)
            : base((int)TimeSpan.FromSeconds(1).TotalMilliseconds, log)
        {
        }

        public override Task Execute()
        {
            TimeSpan endCpuTime = _process.TotalProcessorTime;
            _cpuWatch.Stop();

            if (_startCpuTime.HasValue)
            {
                // A very simple but not that accruate evaluation of how much CPU the process is take out of a core.
                double cpuPercentage = (endCpuTime - _startCpuTime.Value).TotalMilliseconds / _cpuWatch.ElapsedMilliseconds;
                var cpuMetric = new MetricTelemetry(_cpuMetric, cpuPercentage);
                _telemetryClient.TrackMetric(cpuMetric);
            }

            double memoryInMBytes = _process.WorkingSet64 / _1mb;
            var ramMetric = new MetricTelemetry(_ramMetric, memoryInMBytes);
            _telemetryClient.TrackMetric(ramMetric);

            _startCpuTime = _process.TotalProcessorTime;
            _cpuWatch.Restart();

            return Task.CompletedTask;
        }
    }
}
