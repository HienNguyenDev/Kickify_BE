using System;

namespace Kickify.Application.Common.Utilities;

public static class GeoCalculator
{
    private const double EarthRadiusKm = 6371.0;

    public static double GetDistanceInMeters(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1)); 
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c * 1000; // Convert to meters
    }

    private static double ToRadians(double angleIn10thOfADegree) => angleIn10thOfADegree * Math.PI / 180.0;
}
