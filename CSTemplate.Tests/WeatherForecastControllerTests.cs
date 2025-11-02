using System.Net;
using System.Text;
using CSTemplate.Weather.Controllers;
using CSTemplate.Weather.Data;
using CSTemplate.Weather.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CSTemplate.Tests
{
    public class WeatherForecastControllerTests
    {
        private readonly Mock<ILogger<WeatherForecastController>> _mockLogger;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _mockHttpClient;
        private WeatherDbContext _mockDbContext;

        public WeatherForecastControllerTests()
        {
            _mockLogger = new Mock<ILogger<WeatherForecastController>>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        private WeatherDbContext CreateMockDbContext()
        {
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new WeatherDbContext(options);
            
            // Seed with test data
            context.Summaries.AddRange(
                new WeatherSummary { Id = 1, Name = "Freezing" },
                new WeatherSummary { Id = 2, Name = "Bracing" },
                new WeatherSummary { Id = 3, Name = "Chilly" },
                new WeatherSummary { Id = 4, Name = "Cool" },
                new WeatherSummary { Id = 5, Name = "Mild" }
            );
            context.SaveChanges();

            return context;
        }

        [Fact]
        public async Task GetWeatherForecast_ReturnsSuccessWithMockedData()
        {
            // Arrange
            _mockDbContext = CreateMockDbContext();

            // Mock HTTP response from Open-Meteo API
            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""current"": {
                        ""time"": ""2025-11-02T10:15"",
                        ""temperature_2m"": 34.5
                    }
                }", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var forecasts = Assert.IsAssignableFrom<IEnumerable<WeatherForecast>>(okResult.Value);
            Assert.NotNull(forecasts);
            Assert.NotEmpty(forecasts);
        }

        [Fact]
        public async Task GetWeatherForecast_ReturnsRealTimeDataAsFirstItem()
        {
            // Arrange
            _mockDbContext = CreateMockDbContext();

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""current"": {
                        ""time"": ""2025-11-02T10:15"",
                        ""temperature_2m"": 25.8
                    }
                }", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var forecasts = Assert.IsAssignableFrom<List<WeatherForecast>>(okResult.Value);
            
            var firstForecast = forecasts.First();
            Assert.Equal(26, firstForecast.TemperatureC); // Rounded from 25.8
            Assert.Equal("Real-time data from Open-Meteo", firstForecast.Summary);
        }

        [Fact]
        public async Task GetWeatherForecast_HandlesMissingApiDataGracefully()
        {
            // Arrange
            _mockDbContext = CreateMockDbContext();

            // Simulate API failure
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var forecasts = Assert.IsAssignableFrom<List<WeatherForecast>>(okResult.Value);
            
            // Should still return forecasts from database even if API fails
            Assert.NotEmpty(forecasts);
            Assert.Equal(5, forecasts.Count); // Only DB forecasts, no API data
        }

        [Fact]
        public async Task GetWeatherForecast_ReturnsNotFoundWhenNoSummariesInDatabase()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<WeatherDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockDbContext = new WeatherDbContext(options); // Empty database

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains("No summaries found", notFoundResult.Value?.ToString());
        }

        [Fact]
        public async Task GetWeatherForecast_UsesCorrectApiUrl()
        {
            // Arrange
            _mockDbContext = CreateMockDbContext();
            HttpRequestMessage? capturedRequest = null;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{""current"":{""temperature_2m"":20}}", Encoding.UTF8, "application/json")
                });

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            await controller.Get();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("https://api.open-meteo.com/v1/forecast?latitude=20.5&longitude=-13.4&current=temperature_2m", 
                capturedRequest.RequestUri?.ToString());
        }

        [Fact]
        public async Task GetWeatherForecast_TemperatureFahrenheitCalculationIsCorrect()
        {
            // Arrange
            _mockDbContext = CreateMockDbContext();

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""current"":{""temperature_2m"":0}}", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var forecasts = Assert.IsAssignableFrom<List<WeatherForecast>>(okResult.Value);
            
            foreach (var forecast in forecasts)
            {
                int expectedFahrenheit = 32 + (int)(forecast.TemperatureC / 0.5556);
                Assert.Equal(expectedFahrenheit, forecast.TemperatureF);
            }
        }

        [Fact]
        public async Task GetWeatherForecast_ReturnsCorrectNumberOfForecasts()
        {
            // Arrange
            _mockDbContext = CreateMockDbContext();

            var mockResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""current"":{""temperature_2m"":20}}", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(mockResponse);

            var controller = new WeatherForecastController(_mockLogger.Object, _mockDbContext, _mockHttpClient);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var forecasts = Assert.IsAssignableFrom<List<WeatherForecast>>(okResult.Value);
            
            // Should return 6 forecasts: 1 from API + 5 random
            Assert.Equal(6, forecasts.Count);
        }
    }
}

