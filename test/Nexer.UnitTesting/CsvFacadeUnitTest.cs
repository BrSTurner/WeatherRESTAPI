using Nexer.CSV.Facade;
using Nexer.Domain.Facades;
using Nexer.Domain.Interfaces.CSVFacade;
using Nexer.Domain.Interfaces.ZipFacade;
using Nexer.Domain.Models.DataTransferObjects;
using System.IO;
using Xunit;

namespace Nexer.UnitTesting
{
    public class CsvFacadeUnitTest
    {
        private ICsvFacade CsvFacade { get; set; }
        private IZipFacade ZipFacade { get; set; }

        [Fact]
        public void ShouldGetRecordsFromCSVFile()
        {
            //Arrange
            CsvFacade = GetCsvFacade();
            ZipFacade = GetZipFacade();

            var fileName = "2012-09-23.csv";

            lock (fileName)
            {
                //Act
                using (var stream = new FileStream("Resources/historical.zip", FileMode.Open))
                using (var zipArchive = ZipFacade.ReadZipArchive(stream))
                {
                    var entry = ZipFacade.GetFileByName(zipArchive, fileName);
                    var records = CsvFacade.GetRecords<SensorValueDTO>(entry.Open());

                    //Assert
                    Assert.NotNull(zipArchive);
                    Assert.NotNull(records);
                    Assert.NotEmpty(zipArchive.Entries);
                    Assert.NotNull(entry);
                    Assert.True(entry.Length > 0);
                    Assert.Equal(entry.Name, fileName);
                    Assert.NotEmpty(records);
                }
            }
        }


        private ICsvFacade GetCsvFacade()
        {
            return new CsvFacade();
        }

        private IZipFacade GetZipFacade()
        {
            return new ZipFacade();
        }
    }
}
