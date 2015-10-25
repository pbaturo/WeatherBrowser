using Newtonsoft.Json;
using OpenWheatherMap.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebClientDemo
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "Weather checker";

            Run();
        }


        static void Run()
        {
            while (true)
            {
                var consoleInput = ReadFromConsole();
                if (string.IsNullOrWhiteSpace(consoleInput)) continue;

                try
                {
                    // Execute the command:
                    string result = Execute(consoleInput);

                    // Write out the result:
                    WriteToConsole(result);
                }
                catch (Exception ex)
                {
                    // OOPS! Something went wrong - Write out the problem:
                    WriteToConsole(ex.Message);
                }
            }
        }


        static string Execute(string command)
        {
            string openConditionWhetherMapApiKey = "0ed8e0838eed7063ebe1e0abca7ebb87";
            OpenWeatherMapServiceClient weatherClient = new OpenWeatherMapServiceClient(openConditionWhetherMapApiKey);
            WhetherConditionConsolePresenter presenter = new WhetherConditionConsolePresenter();
            try
            {
                //string city = "Tor";
                //var wheather = weatherClient.GetWetherCondition(city);

                string cityNamePrefix = command;
                var weatherCityList = weatherClient.GetWetherConditionList(cityNamePrefix);
                return presenter.GenerateWatherConditionListMessage(weatherCityList);
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public static void WriteToConsole(string message = "")
        {
            if (message.Length > 0)
            {
                Console.WriteLine(message);
            }
        }


        const string _readPrompt = "city (weather)> ";

        public static string ReadFromConsole(string promptMessage = "")
        {
            // Show a prompt, and get input:
            Console.Write(_readPrompt + promptMessage);
            return Console.ReadLine();
        }
    }

    public class WhetherConditionConsolePresenter
    {
        IFormatProvider _invariantFormatProvider = CultureInfo.InvariantCulture;

        public string GenerateWatherConditionMessage(CurrentWeather weather)
        {
     
            if (weather == null)
            {
                throw new ArgumentNullException(nameof(weather));
            }
            string message = string.Format(_invariantFormatProvider, "City: {0}, CountryCode: {1}\n", weather.name, weather.sys?.country);
            message += string.Format(_invariantFormatProvider, "Temperature: {0:f1} \u00B0C\n", weather.main?.temp);
            message += string.Format(_invariantFormatProvider, "Atmospheric pressure: {0:f1} hPa\n", weather.main?.pressure);
            message += "Weather condtion details:\n";
            foreach (var weatherCondition in weather?.weather)
            { 
                message += string.Format(_invariantFormatProvider, "\t{0}\t", weatherCondition.main);
                message += string.Format(_invariantFormatProvider, "({0})\n", weatherCondition.description);
            }
            return message;
        }


        public string GenerateWatherConditionListMessage(CurrentWeatherList weatherList)
        {
            string message = string.Empty;
            foreach (var weather in weatherList?.list)
            {
                message += GenerateWatherConditionMessage(weather);
                message += "\n";
            }
            return message;
        }
    }

    public class OpenWeatherMapServiceClient
    {

        private readonly string _apiKey;

        public OpenWeatherMapServiceClient(string apiKey)
        {
            _apiKey = apiKey;
        }

        public CurrentWeatherList GetWetherConditionList(string searchPatern)
        {
            if (searchPatern == null)
            {
                throw new ArgumentNullException(nameof(searchPatern));
            }
            using (WebClient client = new WebClient())
            {
                var uriStringFindCities = generateUriStringForFindCity(searchPatern);
                var response = client.DownloadString(uriStringFindCities);
                CurrentWeatherList foundCities = JsonConvert.DeserializeObject<CurrentWeatherList>(response);
                return foundCities;
            }
        }

        public CurrentWeather GetWetherCondition(string cityName)
        {
            if (cityName == null)
            {
                throw new ArgumentNullException(nameof(cityName));
            }
            using (WebClient client = new WebClient())
            {
                var uriStringCurrentWether = generateUriStringForCurrentWheather(cityName);
                var response = client.DownloadString(uriStringCurrentWether);
                CurrentWeather wheather = JsonConvert.DeserializeObject<CurrentWeather>(response);
                return wheather;
            }
        }

        private string generateUriStringForCurrentWheather(string city)
        {
            return string.Format(@"http://api.openweathermap.org/data/2.5/weather?q={0}&units=metric&lang=en&type=accurate&APPID={1}", city, _apiKey);
        }

        private string generateUriStringForFindCity(string city)
        {
            
            return string.Format(@"http://api.openweathermap.org/data/2.5/find?q={0}&type=like&units=metric&sort=population&cnt=30&APPID={1}", city, _apiKey);
        }

    }
}

namespace OpenWheatherMap.Model
{
    public class CurrentWeather
    {
        //cityName
        public string name { get; set; }
        public WeatherMain main { get; set; }
        public WeatherCondition[] weather { get; set; }
        public InfoSys sys { get; set; }
    }

    public class WeatherMain
    {
        //main.temp - Temperature.Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.
        public float temp { get; set; }
        //main.pressure Atmospheric pressure(on the sea level, if there is no sea_level or grnd_level data), hPa
        public float pressure { get; set; }
        //main.humidity Humidity, %
        //main.temp_min Minimum temperature at the moment. This is deviation from current temp that is possible for large cities and megalopolises geographically expanded (use these parameter optionally). Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.
        //main.temp_max Maximum temperature at the moment.This is deviation from current temp that is possible for large cities and megalopolises geographically expanded (use these parameter optionally). Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit.
        //main.sea_level Atmospheric pressure on the sea level, hPa
        //main.grnd_level Atmospheric pressure on the ground level, hPa
    }

    public class WeatherCondition
    {
        //weather(more info Weather condition codes)
        //weather.id Weather condition id

        //weather.main Group of weather parameters(Rain, Snow, Extreme etc.)
        public string main { get; set; }

        //weather.description Weather condition within the group
        public string description { get; set; }
        //weather.icon Weather icon id
    }

    public class InfoSys
    {
        //sys
        //sys.type Internal parameter
        //sys.id Internal parameter
        //sys.message Internal parameter

        public string country { get; set; }

        //sys.country Country code (GB, JP etc.)
        //sys.sunrise Sunrise time, unix, UTC
        //sys.sunset Sunset time, unix, UTC
    }
    public class CurrentWeatherList
    {
        public int count { get; set; }
        public CurrentWeather[] list { get; set; }
    }
}
