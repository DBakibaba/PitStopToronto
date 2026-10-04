namespace PitStop.API.Models
{
    public class WashroomOperatingHour
    {
        public int Id { get; set; }
        public int WashroomId { get; set; }
        public  Washroom? Washroom { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        public TimeOnly? OpenTime { get; set; }
        public TimeOnly? CloseTime { get; set; }

        public bool IsClosed { get; set; }
    }
}
