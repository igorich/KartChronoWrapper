namespace KartChronoWrapper.Models
{
    public class PilotProfile
    {
        public string Id { set; get; }
        public string Name { set; get; }
        public string KartNo { set; get; }
        public string BestLap { set; get; }

        public List<string> Laps { set; get; }
    }
}
