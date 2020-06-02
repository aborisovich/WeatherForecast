using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WeatherForecast.Data;
using WeatherForecast.Models;

namespace WeatherForecast.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        private WeatherDbContext dbContext;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, WeatherDbContext dbContext)
        {
            _logger = logger;
            this.configuration = configuration;
            this.dbContext = dbContext;
            PopulateDb();
        }

        public IActionResult Index()
        {
            if (!configuration.GetSection("AppSettings").Exists())
                throw new ApplicationException("Configuration file is missing AppSettings section");
            if (!configuration.GetSection("AppSettings").GetSection("GoogleMapsApiKey").Exists())
                throw new ApplicationException("Google Maps API key is missing from configuration file");
            if (!configuration.GetSection("AppSettings").GetSection("OpenWeatherMapApiKey").Exists())
                throw new ApplicationException("Open Weather Map API key is missing from configuration file");
            ViewBag.GoogleApiKey = configuration.GetSection("AppSettings").GetSection("GoogleMapsApiKey").Value;
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

        /// <summary>
        /// Populates database with Countries and Cities.
        /// </summary>
        private void PopulateDb()
        {
            //Add countries
            if (!dbContext.Countries.Where(item => item.Name == "England").Any())
            {
                Country england = new Country { Name = "England" };
                dbContext.Add(england);
            }
            if (!dbContext.Countries.Where(item => item.Name == "Russia").Any())
            {
                Country russia = new Country { Name = "Russia" };
                dbContext.Add(russia);
            }
            if (!dbContext.Countries.Where(item => item.Name == "USA").Any())
            {
                Country usa = new Country { Name = "USA" };
                dbContext.Add(usa);
            }
            dbContext.SaveChanges();

            //Add cities
            if (!dbContext.Cities.Where(item => item.Name == "London").Any())
            {
                City london = new City { Name = "London" };
                Country england = dbContext.Countries.First(item => item.Name == "England");
                london.Country = england;
                if (england.Cities == null)
                    england.Cities = new List<City>() { london };
                else
                    england.Cities.Add(london);
            }
            if (!dbContext.Cities.Where(item => item.Name == "Moscow").Any())
            {
                City moscow = new City { Name = "Moscow" };
                Country russia = dbContext.Countries.First(item => item.Name == "Russia");
                moscow.Country = russia;
                if (russia.Cities == null)
                    russia.Cities = new List<City>() { moscow };
                else
                    russia.Cities.Add(moscow);
            }
            if (!dbContext.Cities.Where(item => item.Name == "New York").Any())
            {
                City newYork = new City { Name = "New York" };
                Country usa = dbContext.Countries.First(item => item.Name == "USA");
                newYork.Country = usa;
                if (usa.Cities == null)
                    usa.Cities = new List<City>() { newYork };
                else
                    usa.Cities.Add(newYork);
            }
            dbContext.SaveChanges();
        }
    }
}
