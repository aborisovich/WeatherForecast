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
using WeatherForecast.Data;

namespace WeatherForecast.Controllers
{
    /// <summary>
    /// Creates local REST API providing weather information using external OpenWeatherMap API.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OpenWeatherMapController : ControllerBase
    {
        /// <summary>
        /// Contains OpenWeatherAPI prefixes and suffixes used to build http calls.
        /// </summary>
        private static class ApiCommands
        {
            public static readonly string locationQueryPrefix = "?q=";
            public static readonly string apiKeyPrefix = "&appid=";
            public static readonly string metricUnitsPrefix = "&units=";
        }

        private HttpClient apiClient { get; set; }
        private string ApiKey { get; set; }
        private const string apiUrl = "http://api.openweathermap.org/data/2.5/weather";

        /// <summary>
        /// Creates object instance containing OpenWeatherMap API key.
        /// </summary>
        /// <param name="configuration">Injected application configuration.</param>
        /// <param name="clientFactory">Injected http client used to create calls to external API.</param>
        public OpenWeatherMapController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            apiClient = clientFactory.CreateClient();
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
        }

        /// <summary>
        /// Performs call to OpenWeatherMap API querying current weather in the requested <paramref name="city"/>.
        /// </summary>
        /// <param name="city">City name to check current weather.</param>
        /// <returns><see cref="System.Net.HttpStatusCode.OK"/> with API response content or redirected status code from API call.</returns>
        [HttpGet("{city}")]
        public async Task<ActionResult<string>> GetWeather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return BadRequest("City name argument is missing");
            string apiCall = $"{apiUrl}{ApiCommands.locationQueryPrefix}{city}{ApiCommands.apiKeyPrefix}{ApiKey}{ApiCommands.metricUnitsPrefix}metric";
            HttpResponseMessage responseMessage = await apiClient.GetAsync(apiCall);
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseContent = responseMessage.Content.ReadAsStringAsync().Result;
                return Ok(responseContent);
            }

            return StatusCode((int)responseMessage.StatusCode);
        }
    }
}
