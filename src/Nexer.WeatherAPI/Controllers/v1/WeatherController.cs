using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nexer.Domain.Interfaces.Notificator;
using Nexer.Domain.Interfaces.Services;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.WeatherAPI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nexer.WeatherAPI.Controllers
{
    ///<Summary>
    /// Weather controller responsible to provide JSON results containing the sensor records
    ///</Summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]    
    public class WeatherController : BaseController
    {
        private readonly IWeatherServices _weatherServices;

        ///<Summary>
        /// Weather controller constructor
        ///</Summary>
        public WeatherController(INotificator notificator,
            IWeatherServices weatherServices) : base(notificator)
        {
            _weatherServices = weatherServices;
        }

        /// <summary>
        /// Get the device records for a specific sensor (Temperature, Humidity or Rainfall)
        /// </summary>
        /// <remarks>
        ///  Request example:
        ///  
        /// GET api/v1/devices/dockan/data/2019-01-10/temperature
        /// </remarks>
        /// <param name="deviceId">The IoT device Id</param>
        /// <param name="date">The date regarding the records</param>
        /// <param name="sensorType">The sensor type, temperature, humidity and/or rainfall</param>
        /// <returns>All records of the provided device for a specific sensor</returns>
        /// <response code="200">Returns the records for a device</response>
        /// <response code="400">If the device or records were not found</response>
        [HttpGet("devices/{deviceId}/data/{date}/{sensorType}")]
        [ProducesResponseType(typeof(CustomResponse<WeatherResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecordsForDeviceAndSensorType(string deviceId, string date, string sensorType)
        {

            try
            {
                var records = await _weatherServices.GetDataForDeviceAndSensorType(deviceId, date, sensorType);

                return CustomResult(records);
            }
            catch
            {
                _notificator.AddErrorMessage("Something went wrong recovering the device records");
                return CustomResult();
            }
        }


        /// <summary>
        ///  Get the device records for all sensors (Temperature, Humidity and Rainfall)
        /// </summary>
        /// <remarks>
        /// Request example:
        /// 
        /// GET api/v1/devices/dockan/data/2019-01-10
        /// </remarks>
        /// <param name="deviceId">The IoT device Id</param>
        /// <param name="date">The date regarding the records</param>
        /// <returns>All records of the provided device for all sensors</returns>
        /// <response code="200">Returns the records for a device</response>
        /// <response code="400">If the device or records were not found</response>
        [HttpGet("devices/{deviceId}/data/{date}")]
        [ProducesResponseType(typeof(CustomResponse<WeatherResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRecordsForDevice(string deviceId, string date)
        {

            try
            {
                var records = await _weatherServices.GetDataForDevice(deviceId, date);

                return CustomResult(records);
            }
            catch
            {
                _notificator.AddErrorMessage("Something went wrong recovering the device records");
                return CustomResult();
            }
        }


        /// <summary>
        /// Get the device records for a specific sensor (Temperature, Humidity or Rainfall)
        /// </summary>
        /// <remarks>
        /// Request example:
        /// 
        /// api/v1/getdata?deviceId=dockan&amp;date=2019-01-10&amp;sensorType=temperature
        /// </remarks>
        /// <param name="deviceId">The IoT device Id</param>
        /// <param name="date">The date regarding the records</param>
        /// <param name="sensorType">The sensor type, temperature, humidity and/or rainfall</param>
        /// <returns>All records of the provided device for a specific sensor</returns>
        /// <response code="200">Returns the records for a device</response>
        /// <response code="400">If the device or records were not found</response>
        [HttpGet("getdata")]
        [ProducesResponseType(typeof(CustomResponse<WeatherResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetData(string deviceId, string date, string sensorType)
        {

            try
            {
                var records = await _weatherServices.GetDataForDeviceAndSensorType(deviceId, date, sensorType);

                return CustomResult(records);
            }
            catch
            {
                _notificator.AddErrorMessage("Something went wrong recovering the device records");
                return CustomResult();
            }
        }

        /// <summary>
        ///  Get the device records for all sensors (Temperature, Humidity and Rainfall)
        /// </summary>
        /// <remarks>
        /// Request example:
        /// 
        /// api/v1/getdatafordevice?deviceId=dockan&amp;date=2019-01-10
        /// </remarks>
        /// <param name="deviceId">The IoT device Id</param>
        /// <param name="date">The date regarding the records</param>
        /// <returns>All records of the provided device for all sensors</returns>
        /// <response code="200">Returns the records for a device</response>
        /// <response code="400">If the device or records were not found</response>
        [HttpGet("getdatafordevice")]
        [ProducesResponseType(typeof(CustomResponse<WeatherResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDataForDevice(string deviceId, string date)
        {
            try
            {
                var records = await _weatherServices.GetDataForDevice(deviceId, date);

                return CustomResult(records);
            }
            catch
            {
                _notificator.AddErrorMessage("Something went wrong recovering the device records");
                return CustomResult();
            }            
        }
    }
}
