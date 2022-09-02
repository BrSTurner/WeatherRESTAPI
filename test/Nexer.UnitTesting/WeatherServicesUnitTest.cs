using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Nexer.Domain.Interfaces.CSVFacade;
using Nexer.Domain.Interfaces.Notificator;
using Nexer.Domain.Interfaces.Services;
using Nexer.Domain.Interfaces.ZipFacade;
using Nexer.Domain.Models.Configurations;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Models.Enumerations;
using Nexer.Domain.Models.ValidationModels;
using Nexer.Domain.Notificator;
using Nexer.Domain.Repositories;
using Nexer.Domain.Services;
using Nexer.Domain.Validations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Nexer.UnitTesting
{
    public class WeatherServicesUnitTest : BaseUnitTest
    {
        private const string DEVICE_ID = "dockan";
        private const string DATE = "2019-01-10";

        private Mock<IWeatherStorageRepository> MockWeatherRepository { get; set; }
      
        private Mock<ICsvFacade> MockCsvFacade { get; set; }
        private Mock<IZipFacade> MockZipFacade { get; set; }

        private IOptions<WeatherConfiguration> OptionsWeatherConfiguration { get; set; }
        private IValidator<GetDataValidationModel> Validator { get; set; }
        private WeatherConfiguration WeatherConfiguration { get; set; }
        private INotificator Notificator { get; set; }

        private IWeatherServices WeatherServices { get; set; } 

        public WeatherServicesUnitTest() : base()
        {
            MockWeatherRepository = new Mock<IWeatherStorageRepository>();         
            MockCsvFacade = new Mock<ICsvFacade>();
            MockZipFacade = new Mock<IZipFacade>();

            WeatherConfiguration = Configuration.GetSection("WeatherConfiguration").Get<WeatherConfiguration>();
            OptionsWeatherConfiguration = Options.Create(WeatherConfiguration);

            Validator = new GetDataValidation(OptionsWeatherConfiguration);
        }

        [Theory]
        [InlineData("2019/01/10")]
        [InlineData("10/01/2019")]
        [InlineData("10-01-2019")]
        public async Task ShouldValidateWrongDateFormat(string date)
        {
            //Arrange
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, date);

            //Assert
            Assert.Null(actualResult); 
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == "Date is not in correct format, try yyyy-mm-dd" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldValidateEmptyDate()
        {
            //Arrange
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, string.Empty);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == "Date cannot be empty" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldValidateEmptyDeviceId()
        {
            //Arrange
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(string.Empty, DATE);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == "Device Id cannot be empty" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldValidateEmptySensorType()
        {
            //Arrange
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDeviceAndSensorType(DEVICE_ID, DATE, string.Empty);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == "Sensor Type cannot be empty" && x.Type == NotificationTypeEnum.Error);
        }

        [Theory]
        [InlineData("wind")]
        [InlineData("water")]
        [InlineData("proximity")]
        [InlineData("speed")]
        [InlineData("light")]
        public async Task ShouldValidateWrongSensorType(string sensorType)
        {
            //Arrange
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDeviceAndSensorType(DEVICE_ID, DATE, sensorType);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == "The Sensor Type is invalid for this operation" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldNotFindFileForDevice()
        {
            //Arrange
            MockWeatherRepository.Setup(x => x.GetFileFromAzureBlobStorage(It.IsAny<string>())).Returns(Task.FromResult<BlobDataDTO>(null));
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"File {DATE}{WeatherConfiguration.RecordsFileExtension} could not be found" && x.Type == NotificationTypeEnum.Error);            
        }

        [Fact]
        public async Task ShouldNotFindHistoricalFileForDevice()
        {
            //Arrange
            WeatherConfiguration.SensorTypes.ToList().ForEach(sensorType =>
            {
                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{DATE}{WeatherConfiguration.RecordsFileExtension}"))
                    .Returns(Task.FromResult<BlobDataDTO>(null));

                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                    .Returns(Task.FromResult<BlobDataDTO>(null));
            });

            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"File {DATE}{WeatherConfiguration.RecordsFileExtension} could not be found" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldNotFindFileForDeviceInHistorical()
        {
            //Arrange            
            var blobData = GetBlobData(DATE);
            blobData.Content = new MemoryStream(Encoding.UTF8.GetBytes("Temperature"));
 

            WeatherConfiguration.SensorTypes.ToList().ForEach(sensorType =>
            {
                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{DATE}{WeatherConfiguration.RecordsFileExtension}"))
                    .Returns(Task.FromResult<BlobDataDTO>(null));

                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                    .ReturnsAsync(blobData);
            });

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    MockZipFacade.Setup(x => x.ReadZipArchive(It.IsAny<Stream>())).Returns(archive);
                    MockZipFacade.Setup(x => x.GetFileByName(archive, It.IsAny<string>())).Returns<ZipArchiveEntry>(null);
                    WeatherServices = GetWeatherServices();

                    //Act
                    var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

                    //Assert
                    Assert.Null(actualResult);
                    Assert.True(Notificator.HasErrors());
                    Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"File {DATE}{WeatherConfiguration.RecordsFileExtension} could not be found" && x.Type == NotificationTypeEnum.Error);
                }
            }
        }

        [Theory]
        [InlineData("temperature")]
        [InlineData("humidity")]
        [InlineData("rainfall")]
        public async Task ShouldNotFindFileForDeviceAndSensorTypeInHistorical(string sensorType)
        {
            //Arrange            
            var blobData = GetBlobData(DATE);
            blobData.Content = new MemoryStream(Encoding.UTF8.GetBytes("Temperature"));

            MockWeatherRepository
                .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{DATE}{WeatherConfiguration.RecordsFileExtension}"))
                .Returns(Task.FromResult<BlobDataDTO>(null));

            MockWeatherRepository
                .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                .ReturnsAsync(blobData);

            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    MockZipFacade.Setup(x => x.ReadZipArchive(It.IsAny<Stream>())).Returns(archive);
                    MockZipFacade.Setup(x => x.GetFileByName(archive, It.IsAny<string>())).Returns<ZipArchiveEntry>(null);
                    WeatherServices = GetWeatherServices();

                    //Act
                    var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

                    //Assert
                    Assert.Null(actualResult);
                    Assert.True(Notificator.HasErrors());
                    Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"File {DATE}{WeatherConfiguration.RecordsFileExtension} could not be found" && x.Type == NotificationTypeEnum.Error);
                }
            }
        }

        [Theory]
        [InlineData("temperature")]
        [InlineData("humidity")]
        [InlineData("rainfall")]
        public async Task ShouldNotFindHistoricalFileForDeviceAndSensorType(string sensorType)
        {
            //Arrange            
            MockWeatherRepository
                .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{DATE}{WeatherConfiguration.RecordsFileExtension}"))
                .Returns(Task.FromResult<BlobDataDTO>(null));

            MockWeatherRepository
                .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                .Returns(Task.FromResult<BlobDataDTO>(null));

            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"File {DATE}{WeatherConfiguration.RecordsFileExtension} could not be found" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldNotFindRecordsForDeviceInHistorical()
        {
            //Arrange            
            var sensorValues = GetSensorValues(DATE);
            var expectedResult = GetWeatherResponse(DATE, sensorValues);

            var temperatureBlob = GetBlobData(DATE);
            temperatureBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Temperature"));
            var humidityBlob = GetBlobData(DATE);
            humidityBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Humidity"));
            var rainfallBlob = GetBlobData(DATE);
            rainfallBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Rainfall"));

            WeatherConfiguration.SensorTypes.ToList().ForEach(sensorType =>
            {
                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{DATE}{WeatherConfiguration.RecordsFileExtension}"))
                    .Returns(Task.FromResult<BlobDataDTO>(null));

                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                    .ReturnsAsync(sensorType == "temperature" ? temperatureBlob : (sensorType == "humidity" ? humidityBlob : rainfallBlob));
            });

            using (var temperatureStream = new MemoryStream())
            using (var humidityStream = new MemoryStream())
            using (var rainfallStream = new MemoryStream())
            {
                using (var temperatureArchive = new ZipArchive(temperatureStream, ZipArchiveMode.Create, true))
                using (var humidityArchive = new ZipArchive(humidityStream, ZipArchiveMode.Create, true))
                using (var rainfallArchive = new ZipArchive(rainfallStream, ZipArchiveMode.Create, true))
                {
                    var temperatureRecordFile = temperatureArchive.CreateEntry($"{DATE}.csv");
                    var humidityRecordFile = humidityArchive.CreateEntry($"{DATE}.csv");
                    var rainfallRecordFile = rainfallArchive.CreateEntry($"{DATE}.csv");

                    MockZipFacade.Setup(x => x.ReadZipArchive(temperatureBlob.Content)).Returns(temperatureArchive);
                    MockZipFacade.Setup(x => x.ReadZipArchive(humidityBlob.Content)).Returns(humidityArchive);
                    MockZipFacade.Setup(x => x.ReadZipArchive(rainfallBlob.Content)).Returns(rainfallArchive);

                    MockZipFacade.Setup(x => x.GetFileByName(temperatureArchive, It.IsAny<string>())).Returns(temperatureRecordFile);
                    MockZipFacade.Setup(x => x.GetFileByName(humidityArchive, It.IsAny<string>())).Returns(humidityRecordFile);
                    MockZipFacade.Setup(x => x.GetFileByName(rainfallArchive, It.IsAny<string>())).Returns(rainfallRecordFile);

                    MockCsvFacade
                        .Setup(x => x.GetRecords<SensorValueDTO>(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()))
                        .Returns<IEnumerable<SensorValueDTO>>(null);

                    WeatherServices = GetWeatherServices();

                    //Act
                    var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

                    //Assert
                    Assert.Null(actualResult);
                    Assert.True(Notificator.HasErrors());
                    Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"No records found for file {DATE}{WeatherConfiguration.RecordsFileExtension}" && x.Type == NotificationTypeEnum.Error);
                }
            }
        }

        [Theory]
        [InlineData("temperature")]
        [InlineData("humidity")]
        [InlineData("rainfall")]
        public async Task ShouldNotFindFileForDeviceAndSensorType(string sensorType)
        {
            //Arrange
            MockWeatherRepository.Setup(x => x.GetFileFromAzureBlobStorage(It.IsAny<string>())).Returns(Task.FromResult<BlobDataDTO>(null));
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDeviceAndSensorType(DEVICE_ID, DATE, sensorType);

            //Assert
            Assert.Null(actualResult);
            Assert.True(Notificator.HasErrors());
            Assert.Contains(Notificator.GetNotifications(), x => x.Message == $"File {DATE}{WeatherConfiguration.RecordsFileExtension} could not be found" && x.Type == NotificationTypeEnum.Error);
        }

        [Fact]
        public async Task ShouldGetDataForDevice()
        {
            //Arrange
            var blobData = GetBlobData(DATE);
            var sensorValues = GetSensorValues(DATE);
            var expectedResult = GetWeatherResponse(DATE, sensorValues);

            MockWeatherRepository.Setup(x => x.GetFileFromAzureBlobStorage(It.IsAny<string>())).ReturnsAsync(blobData);
            MockCsvFacade.Setup(x => x.GetRecords<SensorValueDTO>(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(sensorValues);
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

            //Assert
            Assert.NotNull(actualResult);
            Assert.NotEmpty(expectedResult.FileName);
            Assert.NotNull(expectedResult.HumidityList);
            Assert.NotNull(expectedResult.TemperatureList);
            Assert.NotNull(expectedResult.RainfallList);
            Assert.Equal(expectedResult.Date, actualResult.Date);
            Assert.Equal(expectedResult.FileName, actualResult.FileName);
            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Date, actualResult.HumidityList.FirstOrDefault().Date);
            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Value, actualResult.HumidityList.FirstOrDefault().Value);
            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Type, actualResult.HumidityList.FirstOrDefault().Type);
            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().NumericValue, actualResult.HumidityList.FirstOrDefault().NumericValue);
            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Date, actualResult.TemperatureList.FirstOrDefault().Date);
            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Value, actualResult.TemperatureList.FirstOrDefault().Value);
            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Type, actualResult.TemperatureList.FirstOrDefault().Type);
            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().NumericValue, actualResult.TemperatureList.FirstOrDefault().NumericValue);
            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Date, actualResult.RainfallList.FirstOrDefault().Date);
            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Value, actualResult.RainfallList.FirstOrDefault().Value);
            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Type, actualResult.RainfallList.FirstOrDefault().Type);
            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().NumericValue, actualResult.RainfallList.FirstOrDefault().NumericValue);
        }

        [Fact]
        public async Task ShouldGetDataForDeviceInHistorical()
        {
            //Arrange            
            var sensorValues = GetSensorValues(DATE);
            var expectedResult = GetWeatherResponse(DATE, sensorValues);

            var temperatureBlob = GetBlobData(DATE);
            temperatureBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Temperature"));
            var humidityBlob = GetBlobData(DATE);
            humidityBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Humidity"));
            var rainfallBlob = GetBlobData(DATE);
            rainfallBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Rainfall"));

            WeatherConfiguration.SensorTypes.ToList().ForEach(sensorType =>
            {
                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{DATE}{WeatherConfiguration.RecordsFileExtension}"))
                    .Returns(Task.FromResult<BlobDataDTO>(null));

                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{DEVICE_ID}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                    .ReturnsAsync(sensorType == "temperature" ? temperatureBlob : (sensorType == "humidity" ? humidityBlob : rainfallBlob));
            });

            using (var temperatureStream = new MemoryStream())
            using (var humidityStream = new MemoryStream())
            using (var rainfallStream = new MemoryStream())
            {
                using (var temperatureArchive = new ZipArchive(temperatureStream, ZipArchiveMode.Create, true))
                using (var humidityArchive = new ZipArchive(humidityStream, ZipArchiveMode.Create, true))
                using (var rainfallArchive = new ZipArchive(rainfallStream, ZipArchiveMode.Create, true))
                {
                    var temperatureRecordFile = temperatureArchive.CreateEntry($"{DATE}.csv");
                    var humidityRecordFile = humidityArchive.CreateEntry($"{DATE}.csv");
                    var rainfallRecordFile = rainfallArchive.CreateEntry($"{DATE}.csv");

                    MockZipFacade.Setup(x => x.ReadZipArchive(temperatureBlob.Content)).Returns(temperatureArchive);
                    MockZipFacade.Setup(x => x.ReadZipArchive(humidityBlob.Content)).Returns(humidityArchive);
                    MockZipFacade.Setup(x => x.ReadZipArchive(rainfallBlob.Content)).Returns(rainfallArchive);

                    MockZipFacade.Setup(x => x.GetFileByName(temperatureArchive, It.IsAny<string>())).Returns(temperatureRecordFile);
                    MockZipFacade.Setup(x => x.GetFileByName(humidityArchive, It.IsAny<string>())).Returns(humidityRecordFile);
                    MockZipFacade.Setup(x => x.GetFileByName(rainfallArchive, It.IsAny<string>())).Returns(rainfallRecordFile);

                    MockCsvFacade.Setup(x => x.GetRecords<SensorValueDTO>(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(sensorValues);

                    WeatherServices = GetWeatherServices();

                    //Act
                    var actualResult = await WeatherServices.GetDataForDevice(DEVICE_ID, DATE);

                    //Assert
                    Assert.NotNull(actualResult);
                    Assert.NotEmpty(expectedResult.FileName);
                    Assert.NotNull(expectedResult.HumidityList);
                    Assert.NotNull(expectedResult.TemperatureList);
                    Assert.NotNull(expectedResult.RainfallList);
                    Assert.Equal(expectedResult.Date, actualResult.Date);
                    Assert.Equal(expectedResult.FileName, actualResult.FileName);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Date, actualResult.HumidityList.FirstOrDefault().Date);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Value, actualResult.HumidityList.FirstOrDefault().Value);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Type, actualResult.HumidityList.FirstOrDefault().Type);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().NumericValue, actualResult.HumidityList.FirstOrDefault().NumericValue);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Date, actualResult.TemperatureList.FirstOrDefault().Date);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Value, actualResult.TemperatureList.FirstOrDefault().Value);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Type, actualResult.TemperatureList.FirstOrDefault().Type);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().NumericValue, actualResult.TemperatureList.FirstOrDefault().NumericValue);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Date, actualResult.RainfallList.FirstOrDefault().Date);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Value, actualResult.RainfallList.FirstOrDefault().Value);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Type, actualResult.RainfallList.FirstOrDefault().Type);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().NumericValue, actualResult.RainfallList.FirstOrDefault().NumericValue);
                }
            }                         
        }

        [Theory]
        [InlineData(DEVICE_ID, DATE, "temperature")]
        [InlineData(DEVICE_ID, DATE, "humidity")]
        [InlineData(DEVICE_ID, DATE, "rainfall")]
        public async Task ShouldGetDataForDeviceAndSensorType(string deviceId, string date, string sensorType)
        {
            //Arrange
            var blobData = GetBlobData(date);
            var sensorValues = GetSensorValues(date);
            var expectedResult = new WeatherResponseDTO
            {
                Date = DateTime.Parse(date),
                FileName = $"{date}{WeatherConfiguration.RecordsFileExtension}",
                HumidityList = sensorType == "humidity" ? sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Humidity }).ToList() : null,
                TemperatureList = sensorType == "temperature" ? sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Temperature }).ToList() : null,
                RainfallList = sensorType == "rainfall" ?  sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Rainfall }).ToList() : null
            };

            MockWeatherRepository.Setup(x => x.GetFileFromAzureBlobStorage(It.IsAny<string>())).ReturnsAsync(blobData);
            MockCsvFacade.Setup(x => x.GetRecords<SensorValueDTO>(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(sensorValues);
            WeatherServices = GetWeatherServices();

            //Act
            var actualResult = await WeatherServices.GetDataForDeviceAndSensorType(deviceId, date, sensorType);

            //Assert
            Assert.NotNull(actualResult);
            Assert.NotEmpty(expectedResult.FileName);
            Assert.Equal(expectedResult.Date, actualResult.Date);
            Assert.Equal(expectedResult.FileName, actualResult.FileName);

            switch (sensorType)
            {
                case "temperature":
                    Assert.Null(expectedResult.HumidityList);
                    Assert.Null(expectedResult.RainfallList);
                    Assert.NotNull(expectedResult.TemperatureList);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Date, actualResult.TemperatureList.FirstOrDefault().Date);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Value, actualResult.TemperatureList.FirstOrDefault().Value);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Type, actualResult.TemperatureList.FirstOrDefault().Type);
                    Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().NumericValue, actualResult.TemperatureList.FirstOrDefault().NumericValue);
                    break;
                case "humidity":
                    Assert.Null(expectedResult.TemperatureList);
                    Assert.Null(expectedResult.RainfallList);
                    Assert.NotNull(expectedResult.HumidityList);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Date, actualResult.HumidityList.FirstOrDefault().Date);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Value, actualResult.HumidityList.FirstOrDefault().Value);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Type, actualResult.HumidityList.FirstOrDefault().Type);
                    Assert.Equal(expectedResult.HumidityList.FirstOrDefault().NumericValue, actualResult.HumidityList.FirstOrDefault().NumericValue);
                    break;
                case "rainfall":
                    Assert.Null(expectedResult.TemperatureList);
                    Assert.Null(expectedResult.HumidityList);
                    Assert.NotNull(expectedResult.RainfallList);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Date, actualResult.RainfallList.FirstOrDefault().Date);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Value, actualResult.RainfallList.FirstOrDefault().Value);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Type, actualResult.RainfallList.FirstOrDefault().Type);
                    Assert.Equal(expectedResult.RainfallList.FirstOrDefault().NumericValue, actualResult.RainfallList.FirstOrDefault().NumericValue);
                    break;
            }
        }

        [Theory]
        [InlineData(DEVICE_ID, DATE, "temperature")]
        [InlineData(DEVICE_ID, DATE, "humidity")]
        [InlineData(DEVICE_ID, DATE, "rainfall")]
        public async Task ShouldGetDataForDeviceAndSensorTypeInHistorical(string deviceId, string date, string sensorType)
        {
            //Arrange            
            var sensorValues = GetSensorValues(date);
            var expectedResult = new WeatherResponseDTO
            {
                Date = DateTime.Parse(date),
                FileName = $"{date}{WeatherConfiguration.RecordsFileExtension}",
                HumidityList = sensorType == "humidity" ? sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Humidity }).ToList() : null,
                TemperatureList = sensorType == "temperature" ? sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Temperature }).ToList() : null,
                RainfallList = sensorType == "rainfall" ? sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Rainfall }).ToList() : null
            };

            var temperatureBlob = GetBlobData(date);
            temperatureBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Temperature"));
            var humidityBlob = GetBlobData(date);
            humidityBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Humidity"));
            var rainfallBlob = GetBlobData(date);
            rainfallBlob.Content = new MemoryStream(Encoding.UTF8.GetBytes("Rainfall"));

            WeatherConfiguration.SensorTypes.ToList().ForEach(sensorType =>
            {
                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{deviceId}/{sensorType}/{date}{WeatherConfiguration.RecordsFileExtension}"))
                    .Returns(Task.FromResult<BlobDataDTO>(null));

                MockWeatherRepository
                    .Setup(x => x.GetFileFromAzureBlobStorage($"{deviceId}/{sensorType}/{WeatherConfiguration.HistoricalRecord}"))
                    .ReturnsAsync(sensorType == "temperature" ? temperatureBlob : (sensorType == "humidity" ? humidityBlob : rainfallBlob));
            });

            using (var temperatureStream = new MemoryStream())
            using (var humidityStream = new MemoryStream())
            using (var rainfallStream = new MemoryStream())
            {
                using (var temperatureArchive = new ZipArchive(temperatureStream, ZipArchiveMode.Create, true))
                using (var humidityArchive = new ZipArchive(humidityStream, ZipArchiveMode.Create, true))
                using (var rainfallArchive = new ZipArchive(rainfallStream, ZipArchiveMode.Create, true))
                {
                    var temperatureRecordFile = temperatureArchive.CreateEntry($"{date}.csv");
                    var humidityRecordFile = humidityArchive.CreateEntry($"{date}.csv");
                    var rainfallRecordFile = rainfallArchive.CreateEntry($"{date}.csv");

                    MockZipFacade.Setup(x => x.ReadZipArchive(temperatureBlob.Content)).Returns(temperatureArchive);
                    MockZipFacade.Setup(x => x.ReadZipArchive(humidityBlob.Content)).Returns(humidityArchive);
                    MockZipFacade.Setup(x => x.ReadZipArchive(rainfallBlob.Content)).Returns(rainfallArchive);

                    MockZipFacade.Setup(x => x.GetFileByName(temperatureArchive, It.IsAny<string>())).Returns(temperatureRecordFile);
                    MockZipFacade.Setup(x => x.GetFileByName(humidityArchive, It.IsAny<string>())).Returns(humidityRecordFile);
                    MockZipFacade.Setup(x => x.GetFileByName(rainfallArchive, It.IsAny<string>())).Returns(rainfallRecordFile);

                    MockCsvFacade.Setup(x => x.GetRecords<SensorValueDTO>(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>())).Returns(sensorValues);

                    WeatherServices = GetWeatherServices();

                    //Act
                    var actualResult = await WeatherServices.GetDataForDeviceAndSensorType(deviceId, date, sensorType);

                    //Assert
                    Assert.NotNull(actualResult);
                    Assert.NotEmpty(expectedResult.FileName);
                    Assert.Equal(expectedResult.Date, actualResult.Date);
                    Assert.Equal(expectedResult.FileName, actualResult.FileName);

                    switch (sensorType)
                    {
                        case "temperature":
                            Assert.Null(expectedResult.HumidityList);
                            Assert.Null(expectedResult.RainfallList);
                            Assert.NotNull(expectedResult.TemperatureList);
                            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Date, actualResult.TemperatureList.FirstOrDefault().Date);
                            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Value, actualResult.TemperatureList.FirstOrDefault().Value);
                            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().Type, actualResult.TemperatureList.FirstOrDefault().Type);
                            Assert.Equal(expectedResult.TemperatureList.FirstOrDefault().NumericValue, actualResult.TemperatureList.FirstOrDefault().NumericValue);
                            break;
                        case "humidity":
                            Assert.Null(expectedResult.TemperatureList);
                            Assert.Null(expectedResult.RainfallList);
                            Assert.NotNull(expectedResult.HumidityList);
                            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Date, actualResult.HumidityList.FirstOrDefault().Date);
                            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Value, actualResult.HumidityList.FirstOrDefault().Value);
                            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().Type, actualResult.HumidityList.FirstOrDefault().Type);
                            Assert.Equal(expectedResult.HumidityList.FirstOrDefault().NumericValue, actualResult.HumidityList.FirstOrDefault().NumericValue);
                            break;
                        case "rainfall":
                            Assert.Null(expectedResult.TemperatureList);
                            Assert.Null(expectedResult.HumidityList);
                            Assert.NotNull(expectedResult.RainfallList);
                            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Date, actualResult.RainfallList.FirstOrDefault().Date);
                            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Value, actualResult.RainfallList.FirstOrDefault().Value);
                            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().Type, actualResult.RainfallList.FirstOrDefault().Type);
                            Assert.Equal(expectedResult.RainfallList.FirstOrDefault().NumericValue, actualResult.RainfallList.FirstOrDefault().NumericValue);
                            break;
                    }
                }
            }
        }


        #region Setup Data
        private IEnumerable<SensorValueDTO> GetSensorValues(string date)
        {
            return new List<SensorValueDTO> {
                new SensorValueDTO { Date = DateTime.Parse(date), Value = "0"}
            };
        }

        private BlobDataDTO GetBlobData(string date, Stream content = null)
        {
            return new BlobDataDTO
            {
                Name = $"{date}{WeatherConfiguration.RecordsFileExtension}",
                Content = content
            };
        }

        private WeatherResponseDTO GetWeatherResponse(string date, IEnumerable<SensorValueDTO> sensorValues)
        {
            return new WeatherResponseDTO
            {
                Date = DateTime.Parse(date),
                FileName = $"{date}{WeatherConfiguration.RecordsFileExtension}",
                HumidityList = sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Humidity }).ToList(),
                TemperatureList = sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Temperature }).ToList(),
                RainfallList = sensorValues.Select(x => new SensorValueDTO { Date = x.Date, Value = x.Value, Type = SensorTypeEnum.Rainfall }).ToList()
            };
        }

        private WeatherServices GetWeatherServices()
        {
            Notificator = new Notificator();

            return new WeatherServices(
                MockWeatherRepository.Object,
                Notificator,
                MockCsvFacade.Object,
                OptionsWeatherConfiguration,
                Validator,
                MockZipFacade.Object);
        }
        #endregion
    }
}
