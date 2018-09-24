using System;
using System.Threading.Tasks;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class TypeResolver : IMessageTypeResolver
    {
        public Task<Type> ResolveMessageTypeAsync()
        {
            return Task.FromResult(typeof(InConnectorOrderbook));
        }
    }
}
