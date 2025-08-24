using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Data;
using TinyUrlApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database connection
builder.Services.AddDbContext<UrlDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// ✅ Health check
app.MapGet("/", () => "TinyURL API is running on Azure 🚀");

// ✅ Shorten a URL
app.MapPost("/shorten", async (UrlDbContext db, Url url) =>
{
    // Generate short code
    url.ShortUrl = Guid.NewGuid().ToString().Substring(0, 6);

    db.Urls.Add(url);
    await db.SaveChangesAsync();

    return Results.Ok(new { url.OriginalUrl, url.ShortUrl });
});

// ✅ Get all public URLs
app.MapGet("/urls", async (UrlDbContext db) =>
    await db.Urls.Where(u => !u.IsPrivate).ToListAsync()
);

// ✅ Redirect short URL → Original URL
app.MapGet("/{shortUrl}", async (UrlDbContext db, string shortUrl) =>
{
    var url = await db.Urls.FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);
    if (url == null) return Results.NotFound("Short URL not found");

    url.Clicks++;
    await db.SaveChangesAsync();

    return Results.Redirect(url.OriginalUrl);
});

app.Run();
