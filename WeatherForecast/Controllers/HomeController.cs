using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
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
            foreach (var item in dbContext.Countries.ToList())
            {
                Console.WriteLine($"Country: {item.Name}");
                List<City> cities = dbContext.Cities.Where(col => col.Country.Id == item.Id).ToList();
                Console.WriteLine($"Cities count: {cities.Count}");
                foreach(var item2 in cities)
                {
                    Console.WriteLine($"City: {item2.Name}");
                }
            }
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
            ViewBag.ListOfCountries = new SelectList(dbContext.Countries, "Id", "Name");
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

        [HttpPost]
        public IActionResult SelectCountry(Country model)
        {
            
            //ViewBag.ListOfCities =
            return View();
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
