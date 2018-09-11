using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}
