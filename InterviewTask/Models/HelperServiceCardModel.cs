using System;
using System.Collections.Generic;

namespace InterviewTask.Models
{
    public class HelperServiceCardModel
    {
        public HelperServiceModel HelperService { get; set; }
        public CurrentWeatherModel CurrentWeather { get; set; } 
        public bool IsOpen { get; set; } = false;
        public int ClosedHour { get; set; } = -1;
        public string OpeningHoursMessage { get; set; } = string.Empty;
    }
}

