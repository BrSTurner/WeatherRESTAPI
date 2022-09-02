using FluentValidation;
using Microsoft.Extensions.Options;
using Nexer.Domain.Helpers;
using Nexer.Domain.Interfaces.CSVFacade;
using Nexer.Domain.Interfaces.Notificator;
using Nexer.Domain.Interfaces.Services;
using Nexer.Domain.Interfaces.ZipFacade;
using Nexer.Domain.Models.Configurations;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Models.Enumerations;
using Nexer.Domain.Models.ValidationModels;
using Nexer.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nexer.Domain.Services
{
    public sealed class WeatherServices : IWeatherServices
    {
        private readonly IWeatherStorageRepository _weatherStorageRepository;
        private readonly INotificator _notificator;
        private readonly ICsvFacade _csvFacade;
        private readonly WeatherConfiguration _weatherConfiguration;
        private readonly IValidator<GetDataValidationModel> _validator;
        private readonly IZipFacade _zipFacade;

        public WeatherServices(
            IWeatherStorageRepository weatherStorageRepository,
            INotificator notificator,
            ICsvFacade csvFacade,
            IOptions<WeatherConfiguration> weatherConfigurationOptions,
            IValidator<GetDataValidationModel> validator,
            IZipFacade zipFacade)
        {
            _weatherStorageRepository = weatherStorageRepository;
            _notificator = notificator;
            _csvFacade = csvFacade;
            _weatherConfiguration = weatherConfigurationOptions.Value;
            _validator = validator;
            _zipFacade = zipFacade;
        }


        public async Task<WeatherResponseDTO> GetDataForDevice(string deviceName, string date)
        {
            var getDataValidationModel = new GetDataValidationModel
            {
                Date = date,
                DeviceId = deviceName
            };

            var validationResult = await _validator.ValidateAsync(getDataValidationModel);

            if (validationResult.IsValid == false)
            {
                _notificator.AddValidationErrorMessages(validationResult.Errors);
                return null;
            }

            var weatherResponse = new WeatherResponseDTO
            {
                Date = DateTime.Parse(date),
                FileName = $"{date}{_weatherConfiguration.RecordsFileExtension}"
            };

            foreach (var sensorType in _weatherConfiguration.SensorTypes)
            {
                var records = await GetRecordsForDeviceAndSensorType(deviceName, date, sensorType);

                if (records == null || !records.Any())
                    return null;

                FillCorrectList(weatherResponse, records);
            }

            return weatherResponse;
        }


        public async Task<WeatherResponseDTO> GetDataForDeviceAndSensorType(string deviceName, string date, string sensorType)
        {
            var getDataValidationModel = new GetDataValidationModel
            {
                Date = date,
                DeviceId = deviceName,
                SensorType = sensorType
            };

            var validationResult = await _validator.ValidateAsync(getDataValidationModel, options => options.IncludeAllRuleSets());

            if (validationResult.IsValid == false)
            {
                _notificator.AddValidationErrorMessages(validationResult.Errors);
                return null;
            }

            var records = await GetRecordsForDeviceAndSensorType(deviceName, date, sensorType);

            if (records == null || !records.Any())
                return null;

            var weatherResponse = new WeatherResponseDTO
            {
                Date = DateTime.Parse(date),
                FileName = $"{date}{_weatherConfiguration.RecordsFileExtension}"
            };

            FillCorrectList(weatherResponse, records);

            return weatherResponse;
        }

        private async Task<IEnumerable<SensorValueDTO>> GetRecordsForDeviceAndSensorType(string deviceName, string date, string sensorType)
        {
            IEnumerable<SensorValueDTO> records = null;

            var fileContent = await _weatherStorageRepository.GetFileFromAzureBlobStorage($"{deviceName}/{sensorType}/{date}{_weatherConfiguration.RecordsFileExtension}");

            if (fileContent == null)
            {
                records = await GetDataForDeviceAndSensorTypeFromHistorical(deviceName, date, sensorType);
            }
            else
            {
                var sensorTypeValue = EnumerationHelper.GetEnumValue<SensorTypeEnum>(sensorType);
                records = _csvFacade.GetRecords<SensorValueDTO>(fileContent.Content);

                if (records == null || !records.Any())
                {
                    _notificator.AddErrorMessage($"No records found for file {date}{_weatherConfiguration.RecordsFileExtension}");
                    return null;
                }

                records = records?.Select(x => new SensorValueDTO
                {
                    Date = x.Date,
                    Value = x.Value,
                    Type = sensorTypeValue
                });
            }

            return records;
        }

        private async Task<IEnumerable<SensorValueDTO>> GetDataForDeviceAndSensorTypeFromHistorical(string deviceName, string date, string sensorType)
        {
            var historicalFileContent = await _weatherStorageRepository.GetFileFromAzureBlobStorage($"{deviceName}/{sensorType}/{_weatherConfiguration.HistoricalRecord}");

            if (historicalFileContent == null)
            {
                _notificator.AddErrorMessage($"File {date}{_weatherConfiguration.RecordsFileExtension} could not be found");
                return null;
            }

            IEnumerable<SensorValueDTO> records = null;
            SensorTypeEnum sensorTypeValue;

            using (var historicalZipFile = _zipFacade.ReadZipArchive(historicalFileContent.Content))
            {
                var fileEntry = _zipFacade.GetFileByName(historicalZipFile, $"{date}{_weatherConfiguration.RecordsFileExtension}");

                if (fileEntry == null)
                {
                    _notificator.AddErrorMessage($"File {date}{_weatherConfiguration.RecordsFileExtension} could not be found");
                    return null;
                }

                sensorTypeValue = EnumerationHelper.GetEnumValue<SensorTypeEnum>(sensorType);
                records = _csvFacade.GetRecords<SensorValueDTO>(fileEntry.Open());
            }

            if (records == null || !records.Any())
            {
                _notificator.AddErrorMessage($"No records found for file {date}{_weatherConfiguration.RecordsFileExtension}");
                return null;
            }

            records = records?.Select(x => new SensorValueDTO
            {
                Date = x.Date,
                Value = x.Value,
                Type = sensorTypeValue
            });

            return records;
        }

        private void FillCorrectList(WeatherResponseDTO weatherResponse, IEnumerable<SensorValueDTO> sensorValues)
        {

            if (sensorValues == null || !sensorValues.Any())
                return;

            var sensorType = sensorValues.FirstOrDefault().Type;

            switch (sensorType)
            {
                case SensorTypeEnum.Temperature:
                    weatherResponse.TemperatureList = sensorValues.ToList();
                    break;
                case SensorTypeEnum.Humidity:
                    weatherResponse.HumidityList = sensorValues.ToList();
                    break;
                case SensorTypeEnum.Rainfall:
                    weatherResponse.RainfallList = sensorValues.ToList();
                    break;
            }
        }
    }
}
