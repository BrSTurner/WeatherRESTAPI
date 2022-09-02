using Nexer.Domain.Interfaces.ZipFacade;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Nexer.Domain.Facades
{
    public class ZipFacade : IZipFacade
    {
        public ZipArchive ReadZipArchive(Stream stream)
        {
            return new ZipArchive(stream, ZipArchiveMode.Read);
        }

        public ZipArchiveEntry GetFileByName(ZipArchive zipFile, string fileName)
        {
            return zipFile.Entries.FirstOrDefault(x => x.Name == fileName);
        }
    }
}
