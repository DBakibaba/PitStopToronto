using Microsoft.EntityFrameworkCore;
using PitStop.API.Models;
using System.Diagnostics.Eventing.Reader;

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


        public List<WashroomOperatingHour> ParseDailyHours(string? hours)
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

            foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
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
            if (hours == null || hours == "Closed")
            {
                return operatingHours;
            }

            var parts = hours.Split(';');
            foreach (var part in parts)
            {
                var dayHour = part.Trim();
                string[] splitHours = dayHour.Split(' ');
                DayOfWeek day = ParseDay(splitHours[0]);
                if (splitHours[1] == "Closed")
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



                string openTimeText = $"{splitHours[1] + " " + splitHours[2]}";
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


        public List<WashroomOperatingHour> ParseOsmHours(string? hours)
        {
            List<WashroomOperatingHour> operatingHours = new List<WashroomOperatingHour>();

            if (string.IsNullOrWhiteSpace(hours))
            {
                return operatingHours;
            }

            // For McDonald's and Tim Hortons
            if (hours == "24/7")
            {
                foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
                {
                    var operatingHour = new WashroomOperatingHour
                    {
                        DayOfWeek = day,
                        OpenTime = TimeOnly.MinValue,
                        CloseTime = TimeOnly.MaxValue,
                        IsClosed = false
                    };
                    operatingHours.Add(operatingHour);
                }
                return operatingHours;
            }

            var groups = hours.Split(';');
            foreach (var group in groups)
            {   // ["08:00-23:00"] and ["Mo-We", "08:00-23:00"]
                string dayHours = group.Trim();
                var parts = dayHours.Split(' ');
                string dayRange;
                string timeRange;
                if (parts.Length == 1)
                {
                    dayRange = "Mo-Su";
                    timeRange = parts[0];
                }
                else if(parts.Length == 2)
                {
                    dayRange = parts[0];
                    timeRange = parts[1];
                }
                else
                {
                    return operatingHours;
                }

                
                string[] splitHours = timeRange.Split("-");

                if (splitHours.Length != 2)
                {
                    return new List<WashroomOperatingHour>();
                }

                string openTimeText = splitHours[0];
                string closeTimeText = splitHours[1];
               if (closeTimeText == "24:00")
                {
                    closeTimeText = "00:00";
                }

                if (!TimeOnly.TryParse(openTimeText, out TimeOnly openTime) ||
                    !TimeOnly.TryParse(closeTimeText, out TimeOnly closeTime))
                {
                    return operatingHours;
                }

                List<DayOfWeek> days = ExpandDayRange(dayRange);

                if (days.Count == 0)
                {
                    return operatingHours;
                }

                foreach (DayOfWeek day in days)
                {
                    var operatingHour = new WashroomOperatingHour()
                    {
                        DayOfWeek = day,
                        OpenTime = openTime,
                        CloseTime = closeTime,
                        IsClosed = false
                    };
                    operatingHours.Add(operatingHour);
                }
            }

            return operatingHours;
        }

        public DayOfWeek? ParseDayCode(string day)
        {
            switch (day)
            {
                case "Mo": return DayOfWeek.Monday;
                case "Tu": return DayOfWeek.Tuesday;
                case "We": return DayOfWeek.Wednesday;
                case "Th": return DayOfWeek.Thursday;
                case "Fr": return DayOfWeek.Friday;
                case "Sa": return DayOfWeek.Saturday;
                case "Su": return DayOfWeek.Sunday;
            }
            return null;
        }

        public List<DayOfWeek> ExpandDayRange(string dayRange)
        {
            List<DayOfWeek> days = new List<DayOfWeek>();

            string[] pieces = dayRange.Split("-");
            DayOfWeek? start;
            DayOfWeek? end;

            if (pieces.Length == 1)
            {
                start = ParseDayCode(pieces[0]);
                end = start;
            }
            else if (pieces.Length == 2)
            {
                start = ParseDayCode(pieces[0]);
                end = ParseDayCode(pieces[1]);
            }
            else
            {
                return days;
            }

            if (!start.HasValue || !end.HasValue)
            {
                return days;
            }

            DayOfWeek current = start.Value;

            while (true)
            {
                days.Add(current);
                if (current == end.Value)
                {
                    break;
                }
                current = (DayOfWeek)(((int)current + 1) % 7);
            }

            return days;
        }
    }
}

