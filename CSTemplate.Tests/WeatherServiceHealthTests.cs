using CSTemplate.Weather.Controllers;
using CSTemplate.Weather.Data;
using CSTemplate.Weather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSTemplate.Tests
{
    /// <summary>
    /// Health check tests to verify the service logic is working correctly
    /// Tests run without network or real database
    /// </summary>
    public class WeatherServiceHealthTests
    {
        [Fact]
        public void WeatherForecast_TemperatureConversionIsCorrect()
        {
            // Arrange & Act
            var forecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 0,
                Summary = "Test"
            };

            // Assert
            Assert.Equal(32, forecast.TemperatureF); // 0°C = 32°F
        }

        [Fact]
        public void WeatherForecast_TemperatureConversion_100C()
        {
            // Arrange & Act
            var forecast = new WeatherForecast
            {
                TemperatureC = 100
            };

            // Assert
            Assert.Equal(211, forecast.TemperatureF); // Using the formula: 32 + (int)(100 / 0.5556)
        }

        [Fact]
        public void WeatherForecast_TemperatureConversion_Negative()
        {
            // Arrange & Act
            var forecast = new WeatherForecast
            {
                TemperatureC = -40
            };

            // Assert
            Assert.Equal(-39, forecast.TemperatureF); // Using the formula: 32 + (int)(-40 / 0.5556)
        }

        [Fact]
        public async Task Controller_WithEmptyDatabase_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new WeatherDbContext(options);

            var mockLogger = new Mock<ILogger<WeatherForecastController>>();
            var mockHttpClient = new HttpClient();

            var controller = new WeatherForecastController(mockLogger.Object, context, mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void WeatherSummary_CanBeCreated()
        {
            // Arrange & Act
            var summary = new WeatherSummary
            {
                Id = 1,
                Name = "Sunny"
            };

            // Assert
            Assert.Equal(1, summary.Id);
            Assert.Equal("Sunny", summary.Name);
        }

        [Fact]
        public async Task DbContext_CanAddAndRetrieveSummaries()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Act
            using (var context = new WeatherDbContext(options))
            {
                context.Summaries.Add(new WeatherSummary { Id = 1, Name = "Cloudy" });
                context.Summaries.Add(new WeatherSummary { Id = 2, Name = "Rainy" });
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new WeatherDbContext(options))
            {
                var summaries = await context.Summaries.ToListAsync();
                Assert.Equal(2, summaries.Count);
                Assert.Contains(summaries, s => s.Name == "Cloudy");
                Assert.Contains(summaries, s => s.Name == "Rainy");
            }
        }

        [Fact]
        public void WeatherForecast_AllPropertiesCanBeSet()
        {
            // Arrange
            var date = DateOnly.FromDateTime(DateTime.Now);
            var temp = 25;
            var summary = "Warm";

            // Act
            var forecast = new WeatherForecast
            {
                Date = date,
                TemperatureC = temp,
                Summary = summary
            };

            // Assert
            Assert.Equal(date, forecast.Date);
            Assert.Equal(temp, forecast.TemperatureC);
            Assert.Equal(summary, forecast.Summary);
            Assert.Equal(76, forecast.TemperatureF); // Using the formula: 32 + (int)(25 / 0.5556)
        }

        [Fact]
        public void WeatherForecast_SummaryCanBeNull()
        {
            // Arrange & Act
            var forecast = new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = 20,
                Summary = null
            };

            // Assert
            Assert.Null(forecast.Summary);
        }
    }
}

