using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeatherForecast.Data;
using WeatherForecast.Models;
using WeatherForecast.Services;

namespace WeatherForecast.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        private readonly WeatherDbContext dbContext;
        private readonly WeatherProvider weatherProvider;
        private SelectList listOfCountries;
        private SelectList listOfCities;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, WeatherDbContext dbContext, WeatherProvider weatherProvider)
        {
            _logger = logger;
            this.configuration = configuration;
            this.dbContext = dbContext;
            PopulateDb();
            listOfCountries = new SelectList(dbContext.Countries.ToList(), "Name", "Name");
            listOfCities = new SelectList(dbContext.Cities.ToList(), "Name", "Name");
            if (!configuration.GetSection("AppSettings").Exists())
                throw new ApplicationException("Configuration file is missing AppSettings section");
            if (!configuration.GetSection("AppSettings").GetSection("GoogleMapsApiKey").Exists())
                throw new ApplicationException("Google Maps API key is missing from configuration file");
            if (!configuration.GetSection("AppSettings").GetSection("OpenWeatherMapApiKey").Exists())
                throw new ApplicationException("Open Weather Map API key is missing from configuration file");
            this.weatherProvider = weatherProvider;
        }

        public IActionResult Index()
        {
            ViewBag.ListOfCountries = listOfCountries;
            ViewBag.ListOfCities = listOfCities;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CurrentWeather(City cityName)
        {
            ViewBag.GoogleApiKey = configuration.GetSection("AppSettings").GetSection("GoogleMapsApiKey").Value;
            Dictionary<string, string> weatherData = await weatherProvider.GetCurrentWeatherInfo(cityName.Name);
            GeoCoordinate cityLocation = weatherProvider.GetGeoLocation(cityName.Name);
            return PartialView("CurrentWeather", dbContext.Cities.First(item => item.Name == cityName.Name));
        }

        /// <summary>
        /// Populates database with Countries and Cities.
        /// </summary>
        private void PopulateDb()
        {
            //Add countries
            if (!dbContext.Countries.Where(item => item.Name == "England").Any())
            {
                Country england = new Country { Name = "England" };
                dbContext.Countries.Add(england);
            }
            if (!dbContext.Countries.Where(item => item.Name == "Russia").Any())
            {
                Country russia = new Country { Name = "Russia" };
                dbContext.Countries.Add(russia);
            }
            if (!dbContext.Countries.Where(item => item.Name == "USA").Any())
            {
                Country usa = new Country { Name = "USA" };
                dbContext.Countries.Add(usa);
            }
            dbContext.SaveChanges();

            //Add cities
            if (!dbContext.Cities.Where(item => item.Name == "London").Any())
            {
                City london = new City { Name = "London" };
                Country england = dbContext.Countries.First(item => item.Name == "England");
                london.Country = england;
                dbContext.Cities.Add(london);
            }
            if (!dbContext.Cities.Where(item => item.Name == "Moscow").Any())
            {
                City moscow = new City { Name = "Moscow" };
                Country russia = dbContext.Countries.First(item => item.Name == "Russia");
                moscow.Country = russia;
                dbContext.Cities.Add(moscow);
            }
            if (!dbContext.Cities.Where(item => item.Name == "New York").Any())
            {
                City newYork = new City { Name = "New York" };
                Country usa = dbContext.Countries.First(item => item.Name == "USA");
                newYork.Country = usa;
                dbContext.Cities.Add(newYork);
            }
        }
    }
}
