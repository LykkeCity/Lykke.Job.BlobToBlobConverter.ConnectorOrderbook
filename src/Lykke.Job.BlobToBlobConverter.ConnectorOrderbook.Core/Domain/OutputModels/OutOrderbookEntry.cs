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

        public override string ToString()
        {
            return $"{nameof(Source)},{Source},{nameof(Asset)},{Asset},{nameof(Timestamp)},{Timestamp},{nameof(IsAsk)},{IsAsk},{nameof(Price)},{Price},{nameof(Volume)},{Volume}";
        }

        public static string GetColumns()
        {
            return $"{nameof(Source)},{nameof(Asset)},{nameof(Timestamp)},{nameof(IsAsk)},{nameof(Price)},{nameof(Volume)}";
        }
    }
}
