namespace UrlShorter.API.Entities
{
    public class Url
    {
        public long UrlID { get; set; }
        public required string OriginalUrl { get; set; }
        public required string ShortUrlCode { get; set; }
        public DateTimeOffset ExpirationDate { get; set; }
        public DateTimeOffset CreatedDate { get; set; }


        public virtual ICollection<Analytic>? Analytics { get; set; }
    }
}
