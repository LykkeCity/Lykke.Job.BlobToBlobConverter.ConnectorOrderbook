using Autofac;
using JetBrains.Annotations;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private List<IStartable> _startables = new List<IStartable>();

        public async Task StartAsync()
        {
            foreach (var startable in _startables)
            {
                startable.Start();
            }

            await Task.CompletedTask;
        }

        public void Register(IStartable startable)
        {
            _startables.Add(startable);
        }
    }
}
