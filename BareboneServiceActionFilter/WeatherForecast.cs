using System;

namespace BareboneServiceActionFilter
{
    public class DailyForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; }
    }

    public class HourlyForecast
    {
        public DateTime Time { get; set; }

        public int TemperatureC { get; set; }
    }
}
