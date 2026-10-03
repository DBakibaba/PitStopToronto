namespace PitStop.API.Services
{
    public class OperatingHoursParser
    {
        public DayOfWeek ParseDay(string day)
        {
            switch (day)
            {
                case "Mon":
                    return DayOfWeek.Monday;
                    break;

                case "Tue":
                    return DayOfWeek.Tuesday;
                    break;

                case "Wed":
                    return DayOfWeek.Wednesday;
                    break;

                case "Thu":
                    return DayOfWeek.Thursday;
                    break;

                case "Fri":
                    return DayOfWeek.Friday;
                    break;

                case "Sat":
                    return DayOfWeek.Saturday;
                    break;

                case "Sun":
                    return DayOfWeek.Sunday;
                    break;

            }
            throw new ArgumentException($"Unknown day:{day}");

        }
    }
}
