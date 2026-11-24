using Microsoft.EntityFrameworkCore;
using PitStop.API.Data;
using PitStop.API.Dtos;
using PitStop.API.Models;
using System.Diagnostics;

namespace PitStop.API.Services;

public class WashroomService(PitStopDbContext dbContext)
{
    public async Task<List<Washroom>> GetAllWashroomsAsync()
    {

        return await dbContext.Washrooms.Include(w=>w.OperatingHours).ToListAsync();

    }
    public async Task<List<NearbyWashroomDto>> GetNearByAsync(double latitude, double longitude,LocationType? type,CancellationToken cancellationToken)


    {
        TimeZoneInfo torontoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
        DateTime torontoNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, torontoTimeZone);
        DayOfWeek today = torontoNow.DayOfWeek;
        const double boundingBoxRadiusKm = 3.1;
        double latitudeOffset = boundingBoxRadiusKm / 111.0;
        double minLatitude = latitude - latitudeOffset;
        double maxLatitude = latitude + latitudeOffset;

        double longitudeOffset =
            boundingBoxRadiusKm / (111.0 * Math.Cos(latitude * Math.PI / 180.0));

        double minLongitude = longitude - longitudeOffset;
        double maxLongitude = longitude + longitudeOffset;

        var stopwatch = Stopwatch.StartNew();

        var activeWashrooms = await dbContext.Washrooms.AsNoTracking()
     .Where(washroom => washroom.IsActive)
     .Where(washroom =>
         washroom.Latitude >= minLatitude &&
         washroom.Latitude <= maxLatitude)
     .Where(washroom =>
         washroom.Longitude >= minLongitude &&
         washroom.Longitude <= maxLongitude).Include(washroom => washroom.OperatingHours).ToListAsync(cancellationToken);

        stopwatch.Stop();

        Console.WriteLine(
            $"GetNearByAsync database query: {stopwatch.ElapsedMilliseconds} ms");

        var availableWashrooms = activeWashrooms.Where(washroom => IsAvailableNow(washroom));
        if (type.HasValue)
        {
            availableWashrooms = availableWashrooms.Where(washroom => washroom.Type == type.Value);
        }

        var washroomsWithDistance = availableWashrooms.Select(washroom =>
        {
            var distance = DistanceCalculator.CalculateDistanceKm(
             latitude, longitude,
             washroom.Latitude, washroom.Longitude
            );



            return new NearbyWashroomDto(
                washroom.Id,
                washroom.Name,
                washroom.Address,
                washroom.Latitude,
                washroom.Longitude,
                distance,
                washroom.Type.ToString(),
                BuildHoursText(washroom,today)
                
                
            );
        });

        var nearbyWashrooms = washroomsWithDistance
        .Where(dto => dto.DistanceKm <= 3)
        .OrderBy(dto => dto.DistanceKm).Take(10).ToList();

        return nearbyWashrooms;

    }
    private bool IsAvailableNow(Washroom washroom)
    {
        if (washroom.OperatingHours.Count == 0)
        {
            return true;
        }
        TimeZoneInfo torontoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

        DateTime torontoNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, torontoTimeZone);


        DayOfWeek today = torontoNow.DayOfWeek;
        var todayOperatingHour = washroom.OperatingHours.FirstOrDefault(operatingHour => operatingHour.DayOfWeek == today);

        if(todayOperatingHour == null )
        {
            return true;
        }
        if (todayOperatingHour.IsClosed == true)
        {
            return false;
        }

            TimeOnly currentTime = TimeOnly.FromDateTime(torontoNow);

        if (todayOperatingHour.CloseTime > todayOperatingHour.OpenTime)
        {
            if (currentTime >= todayOperatingHour.OpenTime && currentTime < todayOperatingHour.CloseTime)
            {
                return true;
            }
        }
        else
        {
            if (currentTime >= todayOperatingHour.OpenTime || currentTime < todayOperatingHour.CloseTime)
            {
                return true;
            }
        }

        return false; 
    }

    private static string BuildHoursText(Washroom washroom,DayOfWeek today)
    {
        const string unknown = "Hours unknown. Check before travelling.";
        if (washroom.OperatingHours.Count ==0)
        {
            return unknown;
        }

        var todayHour = washroom.OperatingHours.FirstOrDefault(h => h.DayOfWeek == today);

        if(todayHour==null || !todayHour.CloseTime.HasValue)
        {
            return unknown;
        }

        TimeOnly close = todayHour.CloseTime.Value;

        if (todayHour.OpenTime == TimeOnly.MinValue &&
         (close >= new TimeOnly(23, 59) || close == TimeOnly.MinValue))
        {
            return "Open 24 hours";
        }   

        return $"Open until {close.ToString("h:mm tt")}";
    }


    public async Task<int> AddPortableToiletAsync(double latitude,double longitude)
    {
        var washroom = new Washroom
        {
            Name = "Portable toilet",
            Source = "Manual",
            ExternalId = $"manual:{Guid.NewGuid()}",
            Latitude = latitude,
            Longitude = longitude,
            Type = LocationType.PortableToilet,
            Status = FacilityStatus.Unknown,
            IsActive = false,
        };

        dbContext.Washrooms.Add(washroom);
        await dbContext.SaveChangesAsync();

        return washroom.Id;

    }

    public async Task<List<WaitingToiletDto>> GetWaitingToiletsAsync()
    {
        return await dbContext.Washrooms
            .Where(w => w.Source == "Manual" && !w.IsActive)
            .OrderByDescending(w => w.Id)
            .Select(w => new WaitingToiletDto(w.Id, w.Latitude, w.Longitude))
            .ToListAsync();
    }

    public async Task<bool> ApproveToiletAsync(int id)
    {
        var washroom = await dbContext.Washrooms
            .FirstOrDefaultAsync(w => w.Id == id && w.Source == "Manual" && !w.IsActive);

        if (washroom is null)
        {
            return false;
        }

        washroom.IsActive = true;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectToiletAsync(int id)
    {
        var washroom = await dbContext.Washrooms
            .FirstOrDefaultAsync(w => w.Id == id && w.Source == "Manual" && !w.IsActive);

        if (washroom is null)
        {
            return false;
        }

        dbContext.Washrooms.Remove(washroom);
        await dbContext.SaveChangesAsync();
        return true;
    }

}