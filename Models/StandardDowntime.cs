namespace ProductTrackingSystem.Models
{
    public class StandardDowntime
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Reason { get; set; }
    }
}
