namespace Lykke.Job.BlobToBlobConverter.ConnectorOrderbook.Core.Domain.OutputModels
{
    public class OutOrderbookEntry
    {
        public string BookId { get; set; }

        public string Source { get; set; }

        public string Asset { get; set; }

        public string Timestamp { get; set; }

        public bool IsAsk { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }

        public string GetValuesString()
        {
            return $"{Source},{Asset},{Timestamp},{IsAsk},{Price},{Volume}";
        }

        public static string GetColumnsString()
        {
            return $"{nameof(Source)},{nameof(Asset)},{nameof(Timestamp)},{nameof(IsAsk)},{nameof(Price)},{nameof(Volume)}";
        }
    }
}
