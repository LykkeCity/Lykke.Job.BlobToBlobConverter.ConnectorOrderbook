using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class MessageProcessor : IMessageProcessor<InConnectorOrderbook>
    {
        private const string _mainContainer = "connectorderbook";
        private const int _maxBatchCount = 500000;

        private readonly ILog _log;

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

        public bool TryDeserialize(byte[] data, out InConnectorOrderbook result)
        {
            try
            {
                result = JsonDeserializer.Deserialize<InConnectorOrderbook>(data);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public async Task ProcessAsync(IEnumerable<InConnectorOrderbook> messages, Func<string, IEnumerable<string>, Task> processTask)
        {
            var list = new HashSet<string>();

            foreach (var message in messages)
            {
                AddConvertedMessage(message, list);

                if (list.Count >= _maxBatchCount)
                {
                    await processTask(_mainContainer, list);
                    list.Clear();
                }
            }

            if (list.Count >= 0)
                await processTask(_mainContainer, list);
        }

        private void AddConvertedMessage(InConnectorOrderbook book, ICollection<string> list)
        {
            if (!book.IsValid())
                _log.WriteWarning(nameof(MessageProcessor), nameof(Convert), $"ConnectorOrderbook {book.ToJson()} is invalid!");

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
