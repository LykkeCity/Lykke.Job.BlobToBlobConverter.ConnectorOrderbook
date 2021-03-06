﻿using Lykke.Job.BlobToBlobConverter.Common;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class StructureBuilder : IStructureBuilder, IDynamicStructure
    {
        private const string _containerPrefix = "connectorderbook";

        private readonly string _instanceTag;
        private readonly IBlobSaver _blobSaver;
        private readonly HashSet<string> _assetPairs = new HashSet<string>();
        private readonly List<ColumnInfo> _columnData;

        public bool IsDynamicStructure => true;

        public StructureBuilder(string instanceTag, IBlobSaver blobSaver)
        {
            _instanceTag = instanceTag;
            _blobSaver = blobSaver;
            _columnData = OutOrderbookEntry.GetStructure()
                .Select(p => new ColumnInfo
                {
                    ColumnName = p.Item1,
                    ColumnType = p.Item2
                })
                .ToList();
        }

        public bool IsAllBlobsReprocessingRequired(TablesStructure currentStructure)
        {
            if (currentStructure?.Tables == null || currentStructure.Tables.Count <= 1)
                return true;

            foreach (var tableStructure in currentStructure.Tables)
            {
                string assetPair = ExtractAssetPairFromDirectory(tableStructure.AzureBlobFolder);
                if (assetPair == null)
                    continue;

                _assetPairs.Add(assetPair);
            }

            return false;
        }

        public TablesStructure GetTablesStructure()
        {
            return new TablesStructure
            {
                Tables = new List<TableStructure>
                {
                    GetTableStructureForAssetPair(null),
                }
            };
        }

        public string GetDirectoryName(string assetPair)
        {
            return string.IsNullOrWhiteSpace(assetPair) ? _containerPrefix : $"{_containerPrefix}-{assetPair.ToLower()}";
        }

        public async Task UpdateStructureIfRequiredAsync(string assetPair)
        {
            assetPair = assetPair.ToLower();

            if (string.IsNullOrWhiteSpace(assetPair) || _assetPairs.Contains(assetPair))
                return;

            _assetPairs.Add(assetPair);

            var tablesStructure = new TablesStructure
            {
                Tables = _assetPairs.Select(GetTableStructureForAssetPair).ToList(),
            };

            await _blobSaver.CreateOrUpdateTablesStructureAsync(tablesStructure);
        }

        private TableStructure GetTableStructureForAssetPair(string assetPair)
        {
            return new TableStructure
            {
                TableName = string.IsNullOrWhiteSpace(_instanceTag)
                    ? $"ConnectOrderbook_{assetPair}"
                    : $"ConnectOrderbook_{_instanceTag}_{assetPair}",
                AzureBlobFolder = GetDirectoryName(assetPair),
                Columns = _columnData,
            };
        }

        private string ExtractAssetPairFromDirectory(string directory)
        {
            int lastInd = directory.LastIndexOf('-');
            if (lastInd == -1)
                return null;

            return directory.Substring(lastInd + 1);
        }
    }
}
