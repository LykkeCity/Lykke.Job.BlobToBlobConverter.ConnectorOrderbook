using System.Threading.Tasks;
using Common;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();

        void Register(IStopable stopable);
    }
}
