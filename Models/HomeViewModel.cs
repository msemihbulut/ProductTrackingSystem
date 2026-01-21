using System.Collections.Generic;

namespace ProductTrackingSystem.Models
{
    public class HomeViewModel
    {
        public List<ProductionRecord> InputTable { get; set; }
        public List<ProductionRecord> OutputTable { get; set; }

        public HomeViewModel()
        {
            InputTable = new List<ProductionRecord>();
            OutputTable = new List<ProductionRecord>();
        }
    }
}