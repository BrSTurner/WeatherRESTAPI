using Moq;
using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Repositories;
using Nexer.Infrastructure.Context;
using Nexer.Infrastructure.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Nexer.UnitTesting
{
    public class WeatherRepositoryUnitTest
    {
        private IWeatherStorageRepository _weatherStorageRepository;
        private Mock<IAzureBlobContext> _mockAzureBlobContext;

  
        public WeatherRepositoryUnitTest()
        {
            _mockAzureBlobContext = new Mock<IAzureBlobContext>();
        }

        [Fact]
        public async Task ShouldGetRecordsFile()
        {
            //Arrange
            var fileName = "/dockan/temperature/2019-01-10.csv";

            var expectedResult = new BlobDataDTO
            {
                Name = "2019-01-10.csv",
                Content = new MemoryStream()
            };

            _mockAzureBlobContext.Setup(x => x.DownloadAsync(fileName)).ReturnsAsync(expectedResult);
            _weatherStorageRepository = new WeatherStorageRepository(_mockAzureBlobContext.Object);

            //Act
            var actualResult = await _weatherStorageRepository.GetFileFromAzureBlobStorage(fileName);

            //Assert
            Assert.NotNull(actualResult);
            Assert.Equal(expectedResult, actualResult);
            Assert.Equal(expectedResult.Name, actualResult.Name);
            Assert.Equal(expectedResult.Content, actualResult.Content);
        }

        [Fact]
        public async Task ShouldNotGetRecordsFile()
        {
            //Arrange
            var fileName = "/dockan/temperature/2022-01-10.csv";

            var expectedResult = new BlobDataDTO
            {
                Name = "2022-01-10.csv",
                Content = new MemoryStream()
            };

            _mockAzureBlobContext.Setup(x => x.DownloadAsync(fileName)).Returns(Task.FromResult<BlobDataDTO>(null));
            _weatherStorageRepository = new WeatherStorageRepository(_mockAzureBlobContext.Object);

            //Act
            var actualResult = await _weatherStorageRepository.GetFileFromAzureBlobStorage(fileName);

            //Assert
            Assert.Null(actualResult);
        }
    }
}
