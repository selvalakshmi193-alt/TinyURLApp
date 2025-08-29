using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Models;

namespace TinyUrlApi.Data
{
    public class UrlDbContext : DbContext
    {
        public UrlDbContext(DbContextOptions<UrlDbContext> options) : base(options) { }
        public DbSet<UrlEntry> Urls => Set<UrlEntry>();
    }
}
