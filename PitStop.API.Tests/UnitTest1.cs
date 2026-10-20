using PitStop.API.Services;
using Xunit;

namespace PitStop.API.Tests
{
    public class OperatingHoursParserTests
    {
        private readonly OperatingHoursParser parser = new OperatingHoursParser();

        // ---------- ParseDayCode ----------

        [Theory]
        [InlineData("Mo", DayOfWeek.Monday)]
        [InlineData("Th", DayOfWeek.Thursday)]
        [InlineData("Su", DayOfWeek.Sunday)]
        public void ParseDayCode_KnownCode_ReturnsDay(string code, DayOfWeek expected)
        {
            DayOfWeek? result = parser.ParseDayCode(code);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("PH")]
        [InlineData("Mon")]
        [InlineData("")]
        public void ParseDayCode_UnknownCode_ReturnsNull(string code)
        {
            DayOfWeek? result = parser.ParseDayCode(code);

            Assert.Null(result);
        }

        // ---------- ExpandDayRange ----------

        [Fact]
        public void ExpandDayRange_NormalRange_ReturnsDaysInOrder()
        {
            List<DayOfWeek> result = parser.ExpandDayRange("Mo-We");

            Assert.Equal(
                new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday },
                result);
        }

        [Fact]
        public void ExpandDayRange_RangeThatWrapsPastSaturday_ReturnsDaysAcrossTheWeekEnd()
        {
            // Bug this protects: mod 7 wrap. Sa(6) must go to Su(0).
            List<DayOfWeek> result = parser.ExpandDayRange("Sa-Th");

            Assert.Equal(
                new List<DayOfWeek>
                {
                    DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday,
                    DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday
                },
                result);
        }

        [Fact]
        public void ExpandDayRange_SingleDay_ReturnsOneDay()
        {
            // Bug this protects: "add first, then check". Otherwise a single day returns nothing.
            List<DayOfWeek> result = parser.ExpandDayRange("Mo");

            Assert.Equal(new List<DayOfWeek> { DayOfWeek.Monday }, result);
        }

        [Fact]
        public void ExpandDayRange_MondayToSunday_ReturnsAllSevenDays()
        {
            List<DayOfWeek> result = parser.ExpandDayRange("Mo-Su");

            Assert.Equal(7, result.Count);
        }

        [Theory]
        [InlineData("XY")]
        [InlineData("Mo-We-Fr")]
        [InlineData("Su,PH")]
        public void ExpandDayRange_UnknownFormat_ReturnsEmptyList(string dayRange)
        {
            List<DayOfWeek> result = parser.ExpandDayRange(dayRange);

            Assert.Empty(result);
        }

        // ---------- ExpandDayList ----------

        [Fact]
        public void ExpandDayList_CommaList_ReturnsAllListedDays()
        {
            List<DayOfWeek> result = parser.ExpandDayList("Sa,Su");

            Assert.Equal(new List<DayOfWeek> { DayOfWeek.Saturday, DayOfWeek.Sunday }, result);
        }

        [Fact]
        public void ExpandDayList_RangeAndSingleDay_ReturnsBoth()
        {
            List<DayOfWeek> result = parser.ExpandDayList("Mo-We,Fr");

            Assert.Equal(
                new List<DayOfWeek>
                {
                    DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday
                },
                result);
        }

        [Fact]
        public void ExpandDayList_PublicHoliday_IsSkipped()
        {
            List<DayOfWeek> result = parser.ExpandDayList("Mo-Su,PH");

            Assert.Equal(7, result.Count);
        }

        [Fact]
        public void ExpandDayList_OneUnknownPiece_ReturnsEmptyList()
        {
            List<DayOfWeek> result = parser.ExpandDayList("Mo,XY");

            Assert.Empty(result);
        }

        // ---------- ParseOsmHours ----------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseOsmHours_NoHours_ReturnsEmptyList(string? hours)
        {
            var result = parser.ParseOsmHours(hours);

            Assert.Empty(result);
        }

      

        [Fact]
        public void ParseOsmHours_DayRangeAndTime_ReturnsCorrectRows()
        {
            var result = parser.ParseOsmHours("Mo-We 05:00-22:00");

            Assert.Equal(3, result.Count);
            Assert.All(result, row =>
            {
                Assert.Equal(new TimeOnly(5, 0), row.OpenTime);
                Assert.Equal(new TimeOnly(22, 0), row.CloseTime);
                Assert.False(row.IsClosed);
            });
        }

        [Fact]
        public void ParseOsmHours_TimeWithoutDays_MeansEveryDay()
        {
            // Bug this protects: "08:00-23:00" used to fail because there was only 1 piece.
            var result = parser.ParseOsmHours("08:00-23:00");

            Assert.Equal(7, result.Count);
        }

        [Fact]
        public void ParseOsmHours_CloseAt2400_BecomesMidnight()
        {
            // Bug this protects: TimeOnly cannot parse "24:00".
            var result = parser.ParseOsmHours("Mo-Su 06:00-24:00");

            Assert.Equal(7, result.Count);
            Assert.All(result, row => Assert.Equal(new TimeOnly(0, 0), row.CloseTime));
        }

       

        [Fact]
        public void ParseOsmHours_CommaListOfDays_ReturnsThoseDays()
        {
            var result = parser.ParseOsmHours("Sa,Su 08:00-20:00");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ParseOsmHours_UnknownDayCode_ReturnsEmptyList()
        {
            var result = parser.ParseOsmHours("XY 08:00-20:00");

            Assert.Empty(result);
        }

        [Fact]
        public void ParseOsmHours_MondayToSunday_ReturnsSameHoursEveryDay()
        {
            var result = parser.ParseOsmHours("Mo-Su 07:00-22:00");

            Assert.Equal(7, result.Count);
            foreach (var operatingHour in result)
            {
                Assert.Equal(new TimeOnly(7, 0), operatingHour.OpenTime);
                Assert.Equal(new TimeOnly(22, 0), operatingHour.CloseTime);
                Assert.False(operatingHour.IsClosed);
            }
        }

        [Fact]
        public void ParseOsmHours_SundayHasDifferentHours_ReturnsSundayHours()
        {
            var result = parser.ParseOsmHours("Mo-Sa 08:00-21:00; Su 09:00-18:00");

            Assert.Equal(7, result.Count);

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
                    Assert.Equal(new TimeOnly(8, 0), operatingHour.OpenTime);
                    Assert.Equal(new TimeOnly(21, 0), operatingHour.CloseTime);
                }
                Assert.False(operatingHour.IsClosed);
            }
        }

        [Fact]
        public void ParseOsmHours_TwentyFourSeven_ReturnsOpenAllDay()
        {
            var result = parser.ParseOsmHours("24/7");

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
            var result = parser.ParseOsmHours("Mo-Su 07:00");

            Assert.Empty(result);
        }
    }
}