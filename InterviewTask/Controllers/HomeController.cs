using InterviewTask.Models;
using InterviewTask.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace InterviewTask.Controllers
{
    public class HomeController : Controller
    {

        private readonly IHelperServiceRepository _repository;
        private readonly SimpleServiceLogger _logger;

        public HomeController(IHelperServiceRepository repository, SimpleServiceLogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /*
         * Prepare your opening times here using the provided HelperServiceRepository class.       
         */
        public ActionResult Index()
        {
            _logger.Log(LogLevel.Information, "Fetching all HelperServiceModels");
            _logger.Log(LogLevel.Information, "UserHostAddress: " + Request.UserHostAddress);
            _logger.Log(LogLevel.Information, "UserAgent: " + Request.UserAgent);

            IEnumerable<HelperServiceModel> allHelperServiceModels = _repository.Get();
            List<HelperServiceCardModel> helperServiceCardModels = new List<HelperServiceCardModel>();
            
            if (allHelperServiceModels == null)
            {
                _logger.Log(LogLevel.Error, "No Helper Services found.");
            }

            foreach (HelperServiceModel helperServiceModel in allHelperServiceModels)
            {
                helperServiceCardModels.Add(CalculateOpenClosedTime(helperServiceModel));
            }

            return View(helperServiceCardModels);
        }


        [HttpGet]
        public async Task<ActionResult> GetWeatherResult(string serviceName)
        {
            string roughLocation = GetRoughLocationFromServiceName(serviceName);
            CurrentWeatherModel myModel = new CurrentWeatherModel();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.openweathermap.org");
                StringBuilder sb1 = new StringBuilder();

                //deprecated
                _logger.Log(LogLevel.Warning, "Making call to openweather endpoint - endpoint is deprecated");
                sb1.AppendFormat("/data/2.5/weather?q={0}&appid={1}", roughLocation, "c153ca78917125a8da2621c2f8617ac6");
                var response = await client.GetAsync(sb1.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(data);
                    try
                    {
                        string description = jsonObject["weather"]?[0]["description"]?.ToString();
                        string currentKelvin = jsonObject["main"]?["temp"]?.ToString();

                        double kelvinDouble = 0;
                        if (Double.TryParse(currentKelvin, out kelvinDouble))
                        {
                            double currentCelcius = ConvertKelvinToCelcius(kelvinDouble);
                            currentCelcius = Math.Round(currentCelcius, 1); 
                            myModel.CurrentWeather = description;
                            myModel.CurrentTemperature = currentCelcius + "\u00B0";
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, "Unable to parse temperature from API response");
                            return Json("Error fetching data", JsonRequestBehavior.AllowGet);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, "Unable to parse json response from Weather API.");
                        _logger.Log(LogLevel.Error, "Exception message: " + ex.Message);
                        return Json("Error fetching data", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Error fetching data", JsonRequestBehavior.AllowGet);
                }
            }

            return PartialView("_HelperServiceWeather", myModel);
        }



        /// <summary>
        /// Takes a helper service model and calculates the string to display whether service is currently open
        /// or when it is next due to open
        /// </summary>
        /// <param name="helperServiceModel"></param>
        /// <returns></returns>
        private HelperServiceCardModel CalculateOpenClosedTime(HelperServiceModel helperServiceModel)
        {
            HelperServiceCardModel helperServiceCard = new HelperServiceCardModel();
            helperServiceCard.HelperService = helperServiceModel;
            helperServiceCard.CurrentWeather = new CurrentWeatherModel();

            DayOfWeek today = DateTime.Now.DayOfWeek;
            List<int> todaysHours = GetHoursForGivenDay(today, helperServiceModel);

            if (todaysHours != null)
            {
                helperServiceCard.ClosedHour = todaysHours.Last();
                int currentHour = DateTime.Now.Hour;

                if (currentHour >= todaysHours.First() && currentHour < helperServiceCard.ClosedHour)
                {
                    helperServiceCard.IsOpen = true;
                    DateTime time = new DateTime(1, 1, 1, helperServiceCard.ClosedHour, 0, 0);
                    helperServiceCard.OpeningHoursMessage = "OPEN - OPEN TODAY UNTIL " + time.ToString("htt").ToLower();
                }
                else
                {
                    bool nextDayIsClosed = true;

                    DateTime nextDay = DateTime.Now.AddDays(1);
                    List<int> nextDaysHours = GetHoursForGivenDay(nextDay.DayOfWeek, helperServiceModel);

                    while (nextDayIsClosed)
                    {
                        if (nextDaysHours.First() == 0 && nextDaysHours.Last() == 0)
                        {
                            nextDay = nextDay.AddDays(1);
                            nextDaysHours = GetHoursForGivenDay(nextDay.DayOfWeek, helperServiceModel);
                        }
                        else
                        {
                            nextDayIsClosed = false;
                            DateTime time = new DateTime(1, 1, 1, nextDaysHours.First(), 0, 0);
                            helperServiceCard.OpeningHoursMessage = "CLOSED - REOPENS " + nextDay.DayOfWeek + " at " + time.ToString("htt").ToLower();

                        }
                    }

                }
            }
            else
            {
                helperServiceCard.OpeningHoursMessage = "Currently unable to display opening hours";
            }
            return helperServiceCard;

        }

        /// <summary>
        /// Takes a given day of the week and corresponding HelperServiceModel and returns
        /// the list of ints containing the operating hours for that given day
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <param name="helperServiceModel"></param>
        /// <returns></returns>
        private List<int> GetHoursForGivenDay(DayOfWeek dayOfWeek, HelperServiceModel helperServiceModel)
        {
            List<int> givenDaysHours = new List<int>();

            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    givenDaysHours = helperServiceModel.SundayOpeningHours;
                    break;
                case DayOfWeek.Monday:
                    givenDaysHours = helperServiceModel.MondayOpeningHours;
                    break;
                case DayOfWeek.Tuesday:
                    givenDaysHours = helperServiceModel.TuesdayOpeningHours;
                    break;
                case DayOfWeek.Wednesday:
                    givenDaysHours = helperServiceModel.WednesdayOpeningHours;
                    break;
                case DayOfWeek.Thursday:
                    givenDaysHours = helperServiceModel.ThursdayOpeningHours;
                    break;
                case DayOfWeek.Friday:
                    givenDaysHours = helperServiceModel.FridayOpeningHours;
                    break;
                case DayOfWeek.Saturday:
                    givenDaysHours = helperServiceModel.SaturdayOpeningHours;
                    break;
            }
            return givenDaysHours;
        }

        private string GetRoughLocationFromServiceName(string serviceName)
        {
            string roughLocation = string.Empty;
            if (serviceName != null)
            {
                roughLocation = serviceName.ToLower();
                roughLocation = roughLocation.Replace("helper service", "").Trim();
                roughLocation = roughLocation.Replace("north", "").Trim();
                roughLocation = roughLocation.Replace("south", "").Trim();
                roughLocation = roughLocation.Replace("east", "").Trim();
                roughLocation = roughLocation.Replace("west", "").Trim();
            }
            else
            {
                _logger.Log(LogLevel.Error, "Service Name provided is null. Cannot calculate rough location.");
            }

            return roughLocation;
        }

        private double ConvertKelvinToCelcius(double currentKelvin)
        {
            double celciusDouble = currentKelvin - 273.15;
            return celciusDouble;
        }

    }
}