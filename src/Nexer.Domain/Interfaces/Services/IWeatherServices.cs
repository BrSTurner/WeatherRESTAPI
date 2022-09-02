using Nexer.Domain.Models.DataTransferObjects;
using System.Threading.Tasks;

namespace Nexer.Domain.Interfaces.Services
{
    public interface IWeatherServices
    {
        Task<WeatherResponseDTO> GetDataForDevice(string deviceName, string date);
        Task<WeatherResponseDTO> GetDataForDeviceAndSensorType(string deviceName, string date, string sensorType);
    }
}
