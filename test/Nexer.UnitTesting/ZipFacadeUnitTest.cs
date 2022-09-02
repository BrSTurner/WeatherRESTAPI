using Nexer.Domain.Facades;
using Nexer.Domain.Interfaces.ZipFacade;
using System.IO;
using Xunit;

namespace Nexer.UnitTesting
{
    public class ZipFacadeUnitTest
    {
        private IZipFacade ZipFacade { get; set; }

        [Fact]
        public void ShouldGetZipArchiveFromStream()
        {
            //Arrange
            ZipFacade = GetZipFacade();
            
            var fileName = "2012-09-23.csv";
            
            lock (fileName)
            {
                //Act
                using (var stream = new FileStream("Resources/historical.zip", FileMode.Open))
                using (var zipArchive = ZipFacade.ReadZipArchive(stream))
                {
                    //Assert
                    Assert.NotNull(zipArchive);
                    Assert.NotEmpty(zipArchive.Entries);
                }
            }
        }

        [Fact]
        public void ShouldGetEntryFromZipArchive()
        {
            //Arrange
            ZipFacade = GetZipFacade();
            var fileName = "2012-09-23.csv";

            lock (fileName)
            {
                //Act
                using (var stream = new FileStream("Resources/historical.zip", FileMode.Open))
                using (var zipArchive = ZipFacade.ReadZipArchive(stream))
                {
                    var entry = ZipFacade.GetFileByName(zipArchive, fileName);

                    //Assert
                    Assert.NotNull(zipArchive);
                    Assert.NotEmpty(zipArchive.Entries);
                    Assert.NotNull(entry);
                    Assert.True(entry.Length > 0);
                    Assert.Equal(entry.Name, fileName);
                }
            }
        }

        private IZipFacade GetZipFacade()
        {
            return new ZipFacade();
        }
    }
}
