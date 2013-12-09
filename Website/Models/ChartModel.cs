namespace Website.Models
{
    public class ChartModel
    {
        public string[] labels { get; set; }
        public Dataset[] datasets { get; set; }
    }

    public class Dataset
    {
        public string fillColor { get; set; }
        public string strokeColor { get; set; }
        public double[] data { get; set; }
    }
}