using Microsoft.EntityFrameworkCore;
using PitStop.API.Models;

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

                case "Tue":
                    return DayOfWeek.Tuesday;

                case "Wed":
                    return DayOfWeek.Wednesday;

                case "Thu":
                    return DayOfWeek.Thursday;

                case "Fri":
                    return DayOfWeek.Friday;

                case "Sat":
                    return DayOfWeek.Saturday;

                case "Sun":
                    return DayOfWeek.Sunday;
     
            }
            throw new ArgumentException($"Unknown day:{day}");

        }
        public TimeOnly ParseTime(string time)
        {
            var normalizedTime = time.Replace("a.m", "AM").Replace("p.m", "PM");
            return TimeOnly.Parse(normalizedTime);
        }


        public List<WashroomOperatingHour>ParseDailyHours(string? hours)
        {

            List<WashroomOperatingHour> operatingHours = new List<WashroomOperatingHour>();

            if (string.IsNullOrWhiteSpace(hours))
            {
                return operatingHours;
            }


            var parts = hours.Split(" to ");

            if (parts.Length != 2)
            {
                return operatingHours;
            }

            string openingText = parts[0].Replace("a.m.", "AM").Replace("p.m.", "PM");
            string closingText = parts[1].Replace("a.m.", "AM").Replace("p.m.", "PM");

            if (!TimeOnly.TryParse(openingText, out TimeOnly openTime) ||
                !TimeOnly.TryParse(closingText, out TimeOnly closeTime))
            {
                return operatingHours;
            }

            foreach(DayOfWeek day in Enum.GetValues<DayOfWeek>())
            {
                var PublicWashroomOperatingHours = new WashroomOperatingHour()
                {

                    DayOfWeek = day,
                    OpenTime = openTime,
                    CloseTime = closeTime,
                    IsClosed = false
                };
                operatingHours.Add(PublicWashroomOperatingHours);

            }
            return operatingHours;
        }


        public List<WashroomOperatingHour> Parse(string? hours)
        {
            List<WashroomOperatingHour> operatingHours = new List<WashroomOperatingHour>();
            if(hours == null || hours== "Closed")
            {
                return operatingHours;
            }

            var parts = hours.Split(';');
            foreach(var part in parts)
            {
                var dayHour = part.Trim();
                string[] splitHours = dayHour.Split(' ');
                DayOfWeek day = ParseDay(splitHours[0]);
                if (splitHours[1]=="Closed")
                {
                    var exceptionOperatingHour = new WashroomOperatingHour()
                    {
                        DayOfWeek = day,
                        OpenTime = null,
                        CloseTime = null,
                        IsClosed = true

                    };
                    operatingHours.Add(exceptionOperatingHour);

                    continue;
                }
                
                

                string openTimeText = $"{splitHours[1] + " " + splitHours[2]}" ;
                string closeTimeText = $"{splitHours[4] + " " + splitHours[5]}";

                TimeOnly openTime = ParseTime(openTimeText);
                TimeOnly closeTime = ParseTime(closeTimeText);

                var operatingHour = new WashroomOperatingHour()
                {

                    DayOfWeek = day,
                    OpenTime = openTime,
                    CloseTime = closeTime,
                    IsClosed = false

                };
                operatingHours.Add(operatingHour);
            }
            return operatingHours;
        }
    }
}
