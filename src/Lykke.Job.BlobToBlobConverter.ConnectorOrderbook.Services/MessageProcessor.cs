using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    [UsedImplicitly]
    public class MessageProcessor : IMessageProcessor
    {
        private const int _maxBatchCount = 500000;

        private readonly ILog _log;
        private readonly IDynamicStructure _dynamicStructure;

        private Func<string, List<string>, Task> _messagesHandler;
        private Dictionary<string, List<string>> _assetPairsDict;

        public MessageProcessor(ILog log, IDynamicStructure dynamicStructure)
        {
            _log = log;
            _dynamicStructure = dynamicStructure;
        }

        public void StartBlobProcessing(Func<string, List<string>, Task> messagesHandler)
        {
            _assetPairsDict = new Dictionary<string, List<string>>();
            _messagesHandler = messagesHandler;
        }

        public async Task FinishBlobProcessingAsync()
        {
            foreach (var pair in _assetPairsDict)
            {
                await _messagesHandler(pair.Key, pair.Value);
            }
            _assetPairsDict.Clear();
        }

        public async Task<bool> TryProcessMessageAsync(byte[] data)
        {
            bool result = JsonDeserializer.TryDeserialize(data, out InConnectorOrderbook externalOrderbook);
            if (!result)
                return false;

            if (!externalOrderbook.IsValid())
                _log.WriteWarning(nameof(MessageProcessor), nameof(Convert), $"ConnectorOrderbook {externalOrderbook.ToJson()} is invalid!");

            await ProcessAsync(externalOrderbook);

            return true;
        }

        public async Task ProcessAsync(InConnectorOrderbook book)
        {
            var directory = _dynamicStructure.GetDirectoryName(book.Asset);
            if (!_assetPairsDict.ContainsKey(directory))
            {
                _assetPairsDict.Add(directory, new List<string>());
                await _dynamicStructure.UpdateStructureIfRequiredAsync(book.Asset);
            }
            var list = _assetPairsDict[directory];
            var bookId = Guid.NewGuid().ToString();

            if (book.Asks != null)
                foreach (var ask in book.Asks)
                {
                    var askEntry = new OutOrderbookEntry
                    {
                        BookId = bookId,
                        Source = book.Source,
                        Asset = book.Asset,
                        Timestamp = DateTimeConverter.Convert(book.Timestamp),
                        IsAsk = true,
                        Price = (decimal)ask.Price,
                        Volume = (decimal)ask.Volume,
                    };
                    list.Add(askEntry.GetValuesString());
                }
            if (book.Bids != null)
                foreach (var bid in book.Bids)
                {
                    var bidEntry = new OutOrderbookEntry
                    {
                        BookId = bookId,
                        Source = book.Source,
                        Asset = book.Asset,
                        Timestamp = DateTimeConverter.Convert(book.Timestamp),
                        IsAsk = false,
                        Price = (decimal)bid.Price,
                        Volume = (decimal)bid.Volume,
                    };
                    list.Add(bidEntry.GetValuesString());
                }

            if (list.Count >= _maxBatchCount)
            {
                await _messagesHandler(directory, list);
                list.Clear();
            }
        }
    }
}
