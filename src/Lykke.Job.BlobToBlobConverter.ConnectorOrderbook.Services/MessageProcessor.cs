using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Services;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    [UsedImplicitly]
    public class MessageProcessor : IMessageProcessor
    {
        private const int _maxBatchCount = 1000;

        private readonly ILog _log;
        private readonly IDynamicStructure _dynamicStructure;

        private Func<string, List<string>, Task> _messagesHandler;
        private Dictionary<string, Dictionary<int, List<string>>> _assetPairsDict;

        public MessageProcessor(ILog log, IDynamicStructure dynamicStructure)
        {
            _log = log;
            _dynamicStructure = dynamicStructure;
        }

        public void StartBlobProcessing(Func<string, List<string>, Task> messagesHandler)
        {
            _assetPairsDict = new Dictionary<string, Dictionary<int, List<string>>>();
            _messagesHandler = messagesHandler;
        }

        public async Task FinishBlobProcessingAsync()
        {
            foreach (var pair in _assetPairsDict)
            {
                await _messagesHandler(pair.Key, pair.Value.SelectMany(i => i.Value).ToList());
            }
            _assetPairsDict.Clear();
        }

        public async Task ProcessMessageAsync(object obj)
        {
            var externalOrderbook = obj as InConnectorOrderbook;
            if (!externalOrderbook.IsValid())
                _log.WriteWarning(nameof(MessageProcessor), nameof(Convert), $"ConnectorOrderbook {externalOrderbook.ToJson()} is invalid!");

            await ProcessAsync(externalOrderbook);
        }

        public async Task ProcessAsync(InConnectorOrderbook book)
        {
            var directory = _dynamicStructure.GetDirectoryName(book.Asset);
            if (!_assetPairsDict.ContainsKey(directory))
            {
                _assetPairsDict.Add(directory, new Dictionary<int, List<string>>());
                await _dynamicStructure.UpdateStructureIfRequiredAsync(book.Asset);
            }

            var bookId = Guid.NewGuid().ToString();
            var items = new List<string>();
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
                    items.Add(askEntry.GetValuesString());
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
                    items.Add(bidEntry.GetValuesString());
                }

            var minutesDict = _assetPairsDict[directory];
            int minuteKey = GetMinuteKey(book.Timestamp);

            int pairItemsCount = minutesDict.Sum(i => i.Value.Count);
            if (pairItemsCount >= _maxBatchCount)
            {
                var allItemsFromOtherMinutes = minutesDict.Keys.Where(k => k != minuteKey).SelectMany(i => minutesDict[i]).ToList();
                await _messagesHandler(directory, allItemsFromOtherMinutes);
                minutesDict.Clear();
            }

            minutesDict[minuteKey] = items;
        }

        private int GetMinuteKey(DateTime time)
        {
            return (((time.Year * 13 + time.Month) * 32 + time.Day) * 25 + time.Hour) * 61 + time.Minute;
        }
    }
}
