using System;
using System.Collections.Generic;

namespace Nexer.Domain.Models.DataTransferObjects
{
    public class WeatherResponseDTO
    {
        public DateTime Date { get; set; }
        public string FileName { get; set; }
        public IList<SensorValueDTO> TemperatureList { get; set; }
        public IList<SensorValueDTO> HumidityList { get; set; }
        public IList<SensorValueDTO> RainfallList { get; set; }
    }
}
