using MaxMind.GeoIP2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Xml;
using UAParser;
using UrlShorter.API.Context;
using UrlShorter.API.Entities;
using UrlShorter.API.Helpers;
using UrlShorter.API.Options;
using UrlShorter.API.Services;
using UrlShorter.API.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>();

var shortenerOption = builder.Configuration.GetSection("Shortener").Get<ShortenerOption>();

builder.Services.AddSingleton(shortenerOption!);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//ShortLink Oluþturma
app.MapPost("/link/add", async ([FromBody] string urlParameter, [FromServices] ApplicationDbContext _context) =>
{
    if (urlParameter == null)
    {
        return Results.BadRequest(new
        {
            StatusCode = "400",
            Message = "Url cannot be empty"
        });
    }

    if (!UrlValidator.IsValidUrl(urlParameter))
    {
        return Results.BadRequest(new
        {
            StatusCode = "400",
            Message = "Invalid Url"
        });
    }

    var options = app.Services.GetRequiredService<ShortenerOption>();

    //Unique code control
    string code;
    do
    {
        code = CodeGenerator.GenerateCode(options.ShortUrlLength, options.ShortUrlCharacters);
    } while (_context.Urls.Any(x => x.ShortUrlCode == code));


    Url url = new()
    {
        OriginalUrl = urlParameter,
        ShortUrlCode = code,
        CreatedDate = DateTimeOffset.Now,
        ExpirationDate = DateTimeOffset.Now.AddMinutes(options.ShortUrlExpiration)
    };

    await _context.Urls.AddAsync(url);
    await _context.SaveChangesAsync();

    return Results.Ok(new
    {
        ShortUrl = $"{options.BaseUrl}/{url.ShortUrlCode}",
        url.ExpirationDate
    });
});

//ShortLink'ten OrijinalURL'e yönlendirme
app.MapGet("{code}", async (string code, [FromServices] ApplicationDbContext _context) =>
{
    var options = app.Services.GetRequiredService<ShortenerOption>();

    var urlEntity = await _context.Urls
                            .AsQueryable()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.ShortUrlCode == code);

    if (urlEntity is null)
    {
        return Results.NotFound(new
        {
            StatusCode = "404",
            Message = "Link not found"
        });
    }

    //HTTP context (request, headers, connection vs.) bilgilerine eriþmek için
    var contextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

    // Kullanýcýnýn tarayýcý bilgisini içeren User-Agent header'ý.
    var uaString = contextAccessor?.HttpContext?.Request.Headers.UserAgent.ToString();

    // UAParser kütüphanesi ile User-Agent string'ini parse ederek tarayýcý, iþletim sistemi ve cihaz bilgilerini çözümler.
    var clientInfo = Parser.GetDefault().Parse(uaString);

    // Ýstemci IP adresini alýr (RemoteIpAddress)
    var ipAddress = contextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString();

    // MaxMind GeoIP2 veritabanýnýn yolunu belirleyip GeoService'i initialize ediyoruz.(IP’den ülke, þehir, bölge bilgilerini sorgulamak için.)
    GeoService.Initialize("GeoLite2-City.mmdb");

    // Alýnan IP adresini kullanarak ülke, þehir ve bölge bilgilerini getirir.
    var (country, city, region) = GeoService.GetLocation(ipAddress!);

    Analytic analytic = new()
    {
        UrlID = urlEntity.UrlID,
        CreatedDate = DateTimeOffset.Now,
        UserAgent = contextAccessor?.HttpContext?.Request.Headers.UserAgent,
        IpAddress = ipAddress,
        Referer = contextAccessor?.HttpContext?.Request.Headers.Referer,

        //Need MaxMind.GeoIP2
        Country = country,
        City = city,
        Region = region,

        // Need UAParser 
        Browser = clientInfo.UA.Family,
        Os = clientInfo.OS.Family,
        Device = clientInfo.Device.Family,
        Platform = clientInfo.OS.ToString(),
        Engine = clientInfo.UA.ToString(),
    };

    await _context.Analytics.AddAsync(analytic);
    await _context.SaveChangesAsync();

    return Results.Redirect(urlEntity.OriginalUrl);
});

//ShortLink Analizleri Görme
app.MapGet("link/{code}/analytic", async (string code, [FromServices] ApplicationDbContext _context) =>
{
    var urlEntity = await _context.Urls
                            .AsQueryable()
                            .AsNoTracking()
                            .Include(x => x.Analytics)
                            .FirstOrDefaultAsync(x => x.ShortUrlCode == code);
    if (urlEntity is null)
    {
        return Results.NotFound(new
        {
            StatusCode = "404",
            Message = "Link not found"
        });
    }

    return Results.Ok(new
    {
        urlEntity.OriginalUrl,
        urlEntity.CreatedDate,
        urlEntity.ExpirationDate,
        Analytics = urlEntity.Analytics?.Select(x => new
        {
            x.CreatedDate,
            x.UserAgent,
            x.IpAddress,
            x.Referer,
            x.Country,
            x.City,
            x.Region,
            x.Browser,
            x.Os,
            x.Device,
            x.Platform,
            x.Engine
        })
    });

});

app.Run();
