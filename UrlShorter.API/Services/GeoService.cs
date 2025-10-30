using MaxMind.GeoIP2;
using System.Net;

namespace UrlShorter.API.Services
{
    public static class GeoService
    {
        private static string? _dbPath;

        public static void Initialize(string databasePath)
        {
            _dbPath = databasePath;
        }

        public static (string? Country, string? City, string? Region) GetLocation(string ipAddress)
        {
            if (!IPAddress.TryParse(ipAddress, out var ip))
                return (null, null, null);

            // Local veya özel IP kontrolü
            if (IPAddress.IsLoopback(ip) || ipAddress.StartsWith("10.") || ipAddress.StartsWith("192.168."))
                return ("Local", "Local", "Local");

            try
            {
                if (_dbPath is null)
                    return (null, null, null);

                using var reader = new DatabaseReader(_dbPath);
                var city = reader.City(ip);

                var country = city.Country.Name;
                var cityName = city.City.Name;
                var region = city.Subdivisions.FirstOrDefault()?.Name;

                return (country, cityName, region);
            }
            catch
            {
                return (null, null, null);
            }
        }
    }
}
