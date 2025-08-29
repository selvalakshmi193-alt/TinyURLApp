using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using TinyUrlApi.Data;
using TinyUrlApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UrlDbContext>(options =>
    options.UseSqlite("Data Source=urls.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Create DB file if not exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UrlDbContext>();
    db.Database.EnsureCreated();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Short code generator
static string GenerateCode(int length = 6)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    var bytes = new byte[length];
    RandomNumberGenerator.Fill(bytes);
    return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
} 
app.MapGet("/", () => "TinyURL API is running!");
// POST /api/shorten
app.MapPost("/api/shorten", async (ShortenRequest req, UrlDbContext db, HttpRequest http) =>
{
    if (!Uri.TryCreate(req.OriginalUrl, UriKind.Absolute, out _))
        return Results.BadRequest("Invalid URL");

    string code;
    do { code = GenerateCode(); }
    while (await db.Urls.AnyAsync(u => u.ShortCode == code));

    var entry = new UrlEntry { OriginalUrl = req.OriginalUrl, ShortCode = code, IsPrivate = req.IsPrivate };
    db.Urls.Add(entry);
    await db.SaveChangesAsync();

    var baseUrl = $"{http.Scheme}://{http.Host}";
    return Results.Created($"/api/urls/{entry.Id}", new ShortenResponse(code, $"{baseUrl}/{code}"));
});

// Redirect
app.MapGet("/{code}", async (string code, UrlDbContext db) =>
{
    var entry = await db.Urls.FirstOrDefaultAsync(u => u.ShortCode == code);
    if (entry is null) return Results.NotFound();

    entry.Clicks++;
    await db.SaveChangesAsync();
    return Results.Redirect(entry.OriginalUrl);
});

// List public URLs
app.MapGet("/api/urls", async (UrlDbContext db) =>
    await db.Urls.Where(u => !u.IsPrivate).ToListAsync());

// Get by id
app.MapGet("/api/urls/{id:int}", async (int id, UrlDbContext db) =>
    await db.Urls.FindAsync(id) is UrlEntry e ? Results.Ok(e) : Results.NotFound());

// Delete
app.MapDelete("/api/urls/{id:int}", async (int id, UrlDbContext db) =>
{
    var e = await db.Urls.FindAsync(id);
    if (e is null) return Results.NotFound();
    db.Urls.Remove(e);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();