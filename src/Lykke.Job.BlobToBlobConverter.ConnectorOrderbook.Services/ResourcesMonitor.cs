using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Common;
using Common.Log;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class ResourcesMonitor : TimerPeriod
    {
        private const double _1mb = 1024 * 1024;
        private const string _cpuMetric = "Custom CPU";
        private const string _ramMetric = "Custom RAM";

        private readonly ILog _log;
        private readonly TelemetryClient _telemetryClient = new TelemetryClient();
        private readonly Process _process = Process.GetCurrentProcess();
        private readonly Stopwatch _cpuWatch = new Stopwatch();
        private readonly double? _cpuThreshold;
        private readonly int? _ramMbThreshold;
        private readonly TimeSpan _startCpuTime;

        public ResourcesMonitor(ILog log)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, log)
        {
            _startCpuTime = _process.TotalProcessorTime;
            _cpuWatch.Start();
        }

        public ResourcesMonitor(ILog log, double? cpuThreshold, int? ramMbThreshold)
            : base((int)TimeSpan.FromMinutes(1).TotalMilliseconds, log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _cpuThreshold = cpuThreshold;
            _ramMbThreshold = ramMbThreshold;

            _startCpuTime = _process.TotalProcessorTime;
            _cpuWatch.Start();
        }

        public override Task Execute()
        {
            // A very simple and not that accruate evaluation of how much CPU the process is take out of a core.
            double cpuPercentage = (_process.TotalProcessorTime - _startCpuTime).TotalMilliseconds / _cpuWatch.ElapsedMilliseconds;
            _telemetryClient.TrackMetric(_cpuMetric, cpuPercentage);

            if (_cpuThreshold.HasValue && _cpuThreshold.Value <= cpuPercentage)
                _log.WriteMonitor(nameof(ResourcesMonitor), "", $"CPU usage is {cpuPercentage}");

            double memoryInMBytes = _process.WorkingSet64 / _1mb;
            _telemetryClient.TrackMetric(_ramMetric, memoryInMBytes);

            if (_ramMbThreshold.HasValue && _ramMbThreshold.Value <= memoryInMBytes)
                _log.WriteMonitor(nameof(ResourcesMonitor), "", $"RAM usage is {memoryInMBytes}");

            return Task.CompletedTask;
        }
    }
}
