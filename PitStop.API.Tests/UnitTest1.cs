using PitStop.API.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PitStop.API.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void OperatingHoursParserTests()
        {
            var parser = new OperatingHoursParser();
            string hours= "Mo-Su 07:00-22:00";
            var result = parser.ParseOsmHours(hours);

            Assert.Equal(7, result.Count);


            foreach(var operatingHour in result)
            {
                Assert.Equal(new TimeOnly(7, 0), operatingHour.OpenTime);
                Assert.Equal(new TimeOnly(22, 0), operatingHour.CloseTime);
                Assert.False(operatingHour.IsClosed);
            }

        }
        [Fact]
        public void ParseOsmHours_NullInput_ReturnsFallbackSchedule() 
        { 
            var parser = new OperatingHoursParser();
            string hours = " ";
            var result = parser.ParseOsmHours(hours);

            Assert.Equal(0, result.Count);


            foreach (var operatingHour in result)
            {
                
                Assert.False(operatingHour.IsClosed);
            }

        }
        [Fact]
        public void ParseOsmHours_SundayHasDifferentHours_ReturnCallBackSchedule()
        {
            var parser = new OperatingHoursParser();
            string hours = "Mo-Sa 08:00-21:00; Su 09:00-18:00";
            var result = parser.ParseOsmHours(hours);

            Assert.Equal(7, result.Count);
            Assert.Single(result.Where(operatingHour => operatingHour.DayOfWeek == DayOfWeek.Sunday));

            var uniqueDays = result.Select(operatingHour => operatingHour.DayOfWeek).Distinct().ToList();

            Assert.Equal(7, uniqueDays.Count);


            foreach (var operatingHour in result)
            {
                if (operatingHour.DayOfWeek == DayOfWeek.Sunday)
                {
                    Assert.Equal(new TimeOnly(9, 0), operatingHour.OpenTime);
                    Assert.Equal(new TimeOnly(18, 0), operatingHour.CloseTime);
                }
                else
                {
                    Assert.Equal(new TimeOnly(21, 0), operatingHour.CloseTime);
                    Assert.Equal(new TimeOnly(8, 0), operatingHour.OpenTime);
                }
               
               
                Assert.False(operatingHour.IsClosed);
            }
        }

        [Fact]
        public void ParseOsmHours_AllTimeOpen()
        {
            var parser = new OperatingHoursParser();
            string hours = "24/7";
            var result = parser.ParseOsmHours(hours);

            Assert.Equal(7, result.Count);


            foreach (var operatingHour in result)
            {
                Assert.Equal(TimeOnly.MinValue, operatingHour.OpenTime);
                Assert.Equal(TimeOnly.MaxValue, operatingHour.CloseTime);
                Assert.False(operatingHour.IsClosed);
            }
        }

        [Fact]
        public void ParseOsmHours_MissingClosingTime_ReturnsEmptyList()
        {
            var parser = new OperatingHoursParser();
            string hours = "Mo-Su 07:00";
            var result = parser.ParseOsmHours(hours);

            Assert.Empty(result);

        }

    }
}
