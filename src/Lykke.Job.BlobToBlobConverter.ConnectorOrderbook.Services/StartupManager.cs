using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        public async Task StartAsync()
        {
            // TODO: Implement your startup logic here. Good idea is to log every step

            await Task.CompletedTask;
        }
    }
}
