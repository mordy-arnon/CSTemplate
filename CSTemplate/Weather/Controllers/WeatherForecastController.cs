using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSTemplate.Weather.Data;
using CSTemplate.Weather.Models;
using System.Text.Json;

namespace CSTemplate.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger, WeatherDbContext context, HttpClient httpClient) : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger = logger;
    private readonly WeatherDbContext _context = context;
    private readonly HttpClient _httpClient = httpClient;
    private const string OpenMeteoApiUrl = "https://api.open-meteo.com/v1/forecast?latitude=20.5&longitude=-13.4&current=temperature_2m";

    [HttpGet(Name =    "GetWeatherForecast")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>>    Get()
    {



        
        try
        {
            // Get all summaries from the database
            var summaries = await _context.Summaries.ToListAsync();

            if (summaries.Count == 0)
            {
                _logger.LogWarning("No summaries found in database");
                return NotFound("No summaries found in the database. Please populate the 'summaries' table.");
            }

            var forecastList = new List<WeatherForecast>();

            // Fetch real temperature from Open-Meteo API
            try
            {
                _logger.LogInformation("Fetching real temperature from Open-Meteo API");
                var response = await _httpClient.GetStringAsync(OpenMeteoApiUrl);
                var meteoData = JsonSerializer.Deserialize<OpenMeteoResponse>(response);

                if (meteoData?.Current != null)
                {
                    // Add real temperature forecast as the first item
                    forecastList.Add(new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        TemperatureC = (int)Math.Round(meteoData.Current.Temperature2m),
                        Summary = "Real-time data from Open-Meteo"
                    });

                    _logger.LogInformation("Successfully fetched real temperature: {Temp}°C", meteoData.Current.Temperature2m);
                }
            }
            catch (Exception apiEx)
            {
                _logger.LogWarning(apiEx, "Failed to fetch data from Open-Meteo API, continuing with database data only");
            }

            // Create additional weather forecasts using summaries from the database and random
            var additionalForecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Count)].Name
            });

            forecastList.AddRange(additionalForecasts);

            return Ok(forecastList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather forecast data");
            return StatusCode(500, "An error occurred while retrieving weather forecast data");
        }
    }
}

