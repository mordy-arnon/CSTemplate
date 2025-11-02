using Microsoft.EntityFrameworkCore;
using CSTemplate.Weather.Models;

namespace CSTemplate.Weather.Data
{
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherSummary> Summaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the WeatherSummary entity
            modelBuilder.Entity<WeatherSummary>(entity =>
            {
                entity.ToTable("summaries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });
        }
    }
}

