using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Models;

namespace TinyUrlApi.Data;

public class UrlDbContext : DbContext
{
    public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }
    public DbSet<ShortUrl> ShortUrls { get; set; }
}
