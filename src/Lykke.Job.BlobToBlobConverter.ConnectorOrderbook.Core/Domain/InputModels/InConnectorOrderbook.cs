using System;
using System.Linq;
using System.Collections.Generic;

namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.InputModels
{
    public class InConnectorOrderbook
    {
        private static int _maxLength = 255;

        public string Source { get; set; }

        public string Asset { get; set; }

        public DateTime Timestamp { get; set; }

        public List<InBookValue> Asks { get; set; }

        public List<InBookValue> Bids { get; set; }

        public bool IsValid()
        {
            bool isValid = !string.IsNullOrWhiteSpace(Source) && Source.Length <= _maxLength
                && !string.IsNullOrWhiteSpace(Asset) && Asset.Length <= _maxLength
                && Timestamp != default(DateTime)
                && (Asks.Count > 0 || Bids.Count > 0);
            if (!isValid)
                return false;

            if (Asks.Count > 0 && Bids.Count > 0)
            {
                double maxBid = Bids.Max(i => i.Price);
                double minAsk = Asks.Min(i => i.Price);
                return maxBid <= minAsk;
            }

            return true;
        }
    }
}
