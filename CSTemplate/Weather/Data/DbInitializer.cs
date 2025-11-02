using CSTemplate.Weather.Models;
using Microsoft.EntityFrameworkCore;

namespace CSTemplate.Weather.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(WeatherDbContext context, ILogger logger)
    {
        try
        {
            // Ensure the database is created and all migrations are applied
            logger.LogInformation("Checking database and applying migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database ready.");

            // Check if summaries table has data
            if (await context.Summaries.AnyAsync())
            {
                logger.LogInformation("Database already contains summaries data.");
                return; // Database has been seeded
            }

            logger.LogInformation("Seeding summaries data...");

            // Add default weather summaries
            var summaries = new[]
            {
                new WeatherSummary { Name = "Freezing" },
                new WeatherSummary { Name = "Bracing" },
                new WeatherSummary { Name = "Chilly" },
                new WeatherSummary { Name = "Cool" },
                new WeatherSummary { Name = "Mild" },
                new WeatherSummary { Name = "Warm" },
                new WeatherSummary { Name = "Balmy" },
                new WeatherSummary { Name = "Hot" },
                new WeatherSummary { Name = "Sweltering" },
                new WeatherSummary { Name = "Scorching" },
                new WeatherSummary { Name = "Rainy" },
                new WeatherSummary { Name = "Cloudy" },
                new WeatherSummary { Name = "Sunny" },
                new WeatherSummary { Name = "Windy" },
                new WeatherSummary { Name = "Foggy" }
            };

            await context.Summaries.AddRangeAsync(summaries);
            await context.SaveChangesAsync();

            logger.LogInformation("Successfully seeded {Count} weather summaries.", summaries.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}

