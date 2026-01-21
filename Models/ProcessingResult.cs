namespace SOb.Models
{
    public class ProcessingResult
    {
        public int id { get; set; }
        public string fileName { get; set; }
        public double dTime { get; set; }
        public DateTime startTime { get; set; }
        public double avgExecutionTime { get; set; }
        public double avgValue { get; set; }
        public double maxValue { get; set; }
        public double midValue { get; set; }
        public double minValue { get; set; }
    }
}
