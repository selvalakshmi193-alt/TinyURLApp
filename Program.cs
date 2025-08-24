using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Data;
using TinyUrlApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});

// Use connection string from appsettings.json
builder.Services.AddDbContext<UrlDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Shorten a URL
app.MapPost("/shorten", async (UrlDbContext db, string originalUrl, bool isPrivate) =>
{
    var shortCode = Guid.NewGuid().ToString().Substring(0, 6);
    var shortUrl = new ShortUrl { OriginalUrl = originalUrl, ShortCode = shortCode, IsPrivate = isPrivate };
    db.ShortUrls.Add(shortUrl);
    await db.SaveChangesAsync();
    return Results.Ok(shortUrl);
});

// Redirect
app.MapGet("/r/{code}", async (UrlDbContext db, string code) =>
{
    var url = await db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);
    if (url == null) return Results.NotFound();
    url.Clicks++;
    await db.SaveChangesAsync();
    return Results.Redirect(url.OriginalUrl);
});

// List Public URLs
app.MapGet("/public", async (UrlDbContext db) =>
    await db.ShortUrls.Where(u => !u.IsPrivate).ToListAsync()
);

// Delete URL
app.MapDelete("/delete/{code}", async (UrlDbContext db, string code) =>
{
    var url = await db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);
    if (url == null) return Results.NotFound();
    db.ShortUrls.Remove(url);
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.MapGet("/", () => "TinyURL API is running on Azure 🚀");


app.Run();
