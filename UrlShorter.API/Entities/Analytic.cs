namespace UrlShorter.API.Entities
{
    public class Analytic
    {
        public long AnalyticID { get; set; }
        public long UrlID { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? Referer { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Browser { get; set; }
        public string? Os { get; set; }
        public string? Device { get; set; }
        public string? Platform { get; set; }
        public string? Engine { get; set; }


        public Url? Url { get; set; }
    }
}
