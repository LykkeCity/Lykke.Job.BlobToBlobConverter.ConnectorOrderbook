using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    [UsedImplicitly]
    public class MessageProcessor : IMessageProcessor
    {
        private const string _mainContainer = "connectorderbook";
        private const int _maxBatchCount = 500000;

        private readonly ILog _log;

        private Func<string, List<string>, Task> _messagesHandler;
        private HashSet<string> _items;

        public MessageProcessor(ILog log)
        {
            _log = log;
        }

        public Dictionary<string, string> GetMappingStructure()
        {
            var result = new Dictionary<string, string>
            {
                { _mainContainer, OutOrderbookEntry.GetColumnsString() },
            };
            return result;
        }

        public void StartBlobProcessing(Func<string, List<string>, Task> messagesHandler)
        {
            _items = new HashSet<string>();
            _messagesHandler = messagesHandler;
        }

        public async Task FinishBlobProcessingAsync()
        {
            await _messagesHandler(_mainContainer, _items.ToList());
            _items.Clear();
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

        public async Task ProcessAsync(InConnectorOrderbook message)
        {
            AddConvertedMessage(message, _items);

            if (_items.Count >= _maxBatchCount)
            {
                await _messagesHandler(_mainContainer, _items.ToList());
                _items.Clear();
            }
        }

        private void AddConvertedMessage(InConnectorOrderbook book, ICollection<string> list)
        {
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
        }
    }
}
