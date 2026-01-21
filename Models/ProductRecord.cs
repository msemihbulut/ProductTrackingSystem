using System;

namespace ProductTrackingSystem.Models
{
    public enum ProductionStatus
    {
        URETIM,
        DURUS
    }

    public class ProductionRecord
    {
        public int RecordId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public ProductionStatus Status { get; set; } = ProductionStatus.URETIM;
        public string? DowntimeReason { get; set; }
        public string Duration => (EndTime - StartTime).ToString(@"hh\:mm");
        public TimeSpan DurationSpan => EndTime - StartTime;
    }
}