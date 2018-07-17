using Common;
using JetBrains.Annotations;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    [UsedImplicitly]
    public class ShutdownManager : IShutdownManager
    {
        private readonly List<IStopable> _items = new List<IStopable>();

        public ShutdownManager(IEnumerable<IStopable> stopables)
        {
            _items = stopables.ToList();
        }

        public async Task StopAsync()
        {
            Parallel.ForEach(_items, i => i.Stop());

            await Task.CompletedTask;
        }
    }
}
