using Nexer.Domain.Models.Enumerations;
using System;

namespace Nexer.Domain.Models.DataTransferObjects
{
    public class SensorValueDTO
    {
        public DateTime Date { get; set; }
        public SensorTypeEnum Type { get; set; }       
        public string Value { get; set; }
        public float NumericValue { 
            get 
            {
                if (Value.StartsWith(","))
                {
                    return float.Parse($"0{Value.Replace(",", ".")}");
                }

                return float.Parse(Value);
            } 
        }
    }
}
