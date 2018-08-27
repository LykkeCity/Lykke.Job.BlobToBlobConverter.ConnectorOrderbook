using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services
{
    public interface IDynamicStructure
    {
        Task UpdateStructureIfRequiredAsync(string assetPair);

        string GetDirectoryName(string assetPair);
    }
}
