﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherForecast.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public City CityOfResidence { get; set; }
    }
}