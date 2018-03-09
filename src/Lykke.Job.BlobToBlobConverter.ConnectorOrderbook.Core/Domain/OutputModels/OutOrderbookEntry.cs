namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels
{
    public class OutOrderbookEntry
    {
        public string Source { get; set; }

        public string Asset { get; set; }

        public string Timestamp { get; set; }

        public bool IsAsk { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }
    }
}
