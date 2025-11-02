using System.Text.Json.Serialization;

namespace CSTemplate.Weather.Models
{
    public class OpenMeteoResponse
    {
        [JsonPropertyName("current")]
        public CurrentWeather? Current { get; set; }
    }

    public class CurrentWeather
    {
        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; set; }
    }
}

