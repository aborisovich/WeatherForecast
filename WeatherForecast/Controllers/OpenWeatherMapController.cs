using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WeatherForecast.Controllers
{
    /// <summary>
    /// Redirects calls from local API to external OpenWeatherMap API.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OpenWeatherMapController : ControllerBase
    {
        private static class ApiCommands
        {
            public static readonly string locationQueryPrefix = "?q=";
            public static readonly string apiKeyPrefix = "&appid=";
            public static readonly string metricUnitsPostfix = "&units=metric";
        }

        private HttpClient apiClient { get; set; }
        private string ApiKey { get; set; }
        private readonly string apiUrl;

        public OpenWeatherMapController(IConfiguration configuration)
        {
            apiClient = HttpClientFactory.Create();
            IConfigurationSection appSettings = configuration.GetSection("AppSettings");
            if (appSettings == null)
                throw new ApplicationException("Application configuration file is malformed. AppSettings section is missing");
            IConfigurationSection apiKey;
            try
            {
                apiKey = appSettings.GetChildren().First(item => item.Key == "OpenWeatherMapApiKey");
            }
            catch (Exception e) when (e is ArgumentNullException || e is InvalidOperationException)
            {
                throw new ApplicationException("Application configuration file is malformed. 'OpenWeatherMapApiKey' entry is missing");
            }
            if (!apiKey.Exists())
                throw new ApplicationException("Application configuration entry 'OpenWeatherMapApiKey' is missing a value. Please provide API key");
            ApiKey = apiKey.Value;
            apiUrl = $"http://api.openweathermap.org/data/2.5/weather";
        }

        [HttpGet("{city}")]
        public async Task<ActionResult<string>> GetWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City name argument is missing");
            string apiCall = $"{apiUrl}{ApiCommands.locationQueryPrefix}{city}{ApiCommands.apiKeyPrefix}{ApiKey}{ApiCommands.metricUnitsPostfix}";
            HttpResponseMessage response = await apiClient.GetAsync(apiCall);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return Ok(response.Content);
            return StatusCode((int)response.StatusCode);
        }
    }
}
