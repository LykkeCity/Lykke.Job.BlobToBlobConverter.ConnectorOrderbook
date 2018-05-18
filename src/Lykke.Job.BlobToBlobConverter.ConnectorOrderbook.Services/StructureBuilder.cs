using System.Linq;
using System.Collections.Generic;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;
using Lykke.Job.BlobToBlobConverter.Common;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class StructureBuilder : IStructureBuilder
    {
        private readonly string _instanceTag;

        internal static string MainContainer => "connectorderbook";

        public StructureBuilder(string instanceTag)
        {
            _instanceTag = instanceTag;
        }

        public Dictionary<string, string> GetMappingStructure()
        {
            var result = new Dictionary<string, string>
            {
                { MainContainer, OutOrderbookEntry.GetColumnsString() },
            };
            return result;
        }

        public TablesStructure GetTablesStructure()
        {
            return new TablesStructure
            {
                Tables = new List<TableStructure>
                {
                    new TableStructure
                    {
                        TableName = string.IsNullOrWhiteSpace(_instanceTag) ? "ConnectOrderbook" : $"ConnectOrderbook_{_instanceTag}",
                        AzureBlobFolder = MainContainer,
                        Colums = OutOrderbookEntry.GetStructure()
                            .Select(p => new ColumnInfo { ColumnName = p.Item1, ColumnType = p.Item2 })
                            .ToList(),
                    }
                }
            };
        }
    }
}
