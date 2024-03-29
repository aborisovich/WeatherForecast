﻿using GeoCoordinatePortable;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherForecast.Controllers;

namespace WeatherForecast.Services
{
    public class WeatherProvider
    {
        private readonly OpenWeatherMapController weatherApi;
        private readonly Dictionary<string, GeoCoordinate> cityLocations;
        public WeatherProvider(OpenWeatherMapController weatherApi)
        {
            this.weatherApi = weatherApi;
            cityLocations = new Dictionary<string, GeoCoordinate>();
        }

        public async Task<Dictionary<string, string>> GetCurrentWeatherInfo(string cityName)
        {
            ActionResult<string> apiResponse = await weatherApi.GetWeather(cityName);
            if (!(apiResponse.Result is OkObjectResult))
                return new Dictionary<string, string>();

            OkObjectResult responseData = apiResponse.Result as OkObjectResult;
            JObject jsonData = JObject.Parse((string)responseData.Value);
            Dictionary<string, string> weatherOutput = new Dictionary<string, string>();
            weatherOutput.Add("Description", jsonData["weather"][0]["description"].ToString());
            weatherOutput.Add("Temperature", jsonData["main"]["temp"].ToString() + "C");
            weatherOutput.Add("Perceived Temperature", jsonData["main"]["feels_like"].ToString() + "C");
            weatherOutput.Add("Pressure", jsonData["main"]["pressure"].ToString() + "hPa");
            weatherOutput.Add("Humidity", jsonData["main"]["humidity"].ToString() + "%");
            weatherOutput.Add("Wind speed", jsonData["wind"]["speed"].ToString() + "km/h");

            if (!cityLocations.ContainsKey(cityName))
            {
                cityLocations.Add(cityName, new GeoCoordinate(
                    latitude: double.Parse(jsonData["coord"]["lat"].ToString()),
                    longitude: double.Parse(jsonData["coord"]["lon"].ToString())
                    ));
            }
            return weatherOutput;
        }

        public GeoCoordinate GetGeoLocation(string cityName)
        {
            return cityLocations.ContainsKey(cityName) ? cityLocations[cityName] : null;
        }
    }
}
