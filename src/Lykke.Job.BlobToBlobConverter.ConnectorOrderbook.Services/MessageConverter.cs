using System.Collections.Generic;
using Common;
using Common.Log;
using Lykke.Job.BlobToBlobConverter.Common.Abstractions;
using Lykke.Job.BlobToBlobConverter.Common.Helpers;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels;
using Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Services
{
    public class MessageConverter : IMessageConverter
    {
        private const string _mainContainer = "connectorderbook";

        private readonly ILog _log;

        public MessageConverter(ILog log)
        {
            _log = log;
        }

        public Dictionary<string, List<string>> Convert(IEnumerable<byte[]> messages)
        {
            var result = new Dictionary<string, List<string>>
            {
                { _mainContainer, new List<string>() },
            };

            foreach (var message in messages)
            {
                AddConvertedMessage(message, result);
            }

            return result;
        }

        private void AddConvertedMessage(byte[] message, Dictionary<string, List<string>> result)
        {
            var book = JsonDeserializer.Deserialize<InConnectorOrderbook>(message);
            if (!book.IsValid())
                _log.WriteWarning(nameof(MessageConverter), nameof(Convert), $"ConnectorOrderbook {book.ToJson()} is invalid!");

            if (book.Asks != null)
                foreach (var ask in book.Asks)
                {
                    var askEntry = new OutOrderbookEntry
                    {
                        Source = book.Source,
                        Asset = book.Asset,
                        Timestamp = DateTimeConverter.Convert(book.Timestamp),
                        IsAsk = true,
                        Price = (decimal)ask.Price,
                        Volume = (decimal)ask.Volume,
                    };
                    result[_mainContainer].Add(askEntry.ToString());
                }
            if (book.Bids != null)
                foreach (var bid in book.Bids)
                {
                    var bidEntry = new OutOrderbookEntry
                    {
                        Source = book.Source,
                        Asset = book.Asset,
                        Timestamp = DateTimeConverter.Convert(book.Timestamp),
                        IsAsk = false,
                        Price = (decimal)bid.Price,
                        Volume = (decimal)bid.Volume,
                    };
                    result[_mainContainer].Add(bidEntry.ToString());
                }
        }
    }
}
