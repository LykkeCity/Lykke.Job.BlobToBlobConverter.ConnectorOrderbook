using System;
using System.Collections.Generic;

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
            return $"{BookId},{Source},{Asset},{Timestamp},{IsAsk},{Price},{Volume}";
        }

        public static string GetColumnsString()
        {
            return $"{nameof(BookId)},{nameof(Source)},{nameof(Asset)},{nameof(Timestamp)},{nameof(IsAsk)},{nameof(Price)},{nameof(Volume)}";
        }

        public static List<(string, string)> GetStructure()
        {
            return new List<(string, string)>
            {
                (nameof(BookId), typeof(string).Name),
                (nameof(Source), typeof(string).Name),
                (nameof(Asset), typeof(string).Name),
                (nameof(Timestamp), typeof(DateTime).Name),
                (nameof(IsAsk), typeof(bool).Name),
                (nameof(Price), typeof(decimal).Name),
                (nameof(Volume), typeof(decimal).Name),
            };
        }
    }
}
