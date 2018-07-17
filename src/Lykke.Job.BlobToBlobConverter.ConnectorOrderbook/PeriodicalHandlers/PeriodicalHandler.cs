using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.PeriodicalHandlers
{
    [UsedImplicitly]
    public class PeriodicalHandler : TimerPeriod
    {
        private readonly IBlobProcessor _blobProcessor;

        public PeriodicalHandler(
            IBlobProcessor blobProcessor,
            IStartupManager startupManager,
            ILog log,
            TimeSpan processTimeout)
            : base((int)processTimeout.TotalMilliseconds, log)
        {
            _blobProcessor = blobProcessor;

            startupManager.Register(this);
        }

        public override async Task Execute()
        {
            await _blobProcessor.ProcessAsync();
        }
    }
}
