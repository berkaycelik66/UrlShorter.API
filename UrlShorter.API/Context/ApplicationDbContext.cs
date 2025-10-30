using Microsoft.EntityFrameworkCore;
using UrlShorter.API.Entities;

namespace UrlShorter.API.Context
{
    public class ApplicationDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("LocalDb");
        }

        public DbSet<Url> Urls { get; set; }
        public DbSet<Analytic> Analytics { get; set; }
    }
}
