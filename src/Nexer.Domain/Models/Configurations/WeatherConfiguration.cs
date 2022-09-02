using System.Collections.Generic;

namespace Nexer.Domain.Models.Configurations
{
    public class WeatherConfiguration
    {
        public string RecordsFileExtension { get; set; }
        public string HistoricalRecord { get; set; }
        public IList<string> SensorTypes { get; set; }
    }
}
