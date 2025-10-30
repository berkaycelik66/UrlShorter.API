namespace UrlShorter.API.Options
{
    public class ShortenerOption
    {
        public required string BaseUrl { get; set; }
        public short ShortUrlLength { get; set; }
        public required string ShortUrlCharacters { get; set; }
        public short ShortUrlExpiration { get; set; }
    }
}
