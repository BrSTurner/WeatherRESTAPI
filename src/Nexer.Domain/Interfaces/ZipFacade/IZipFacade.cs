using System.IO;
using System.IO.Compression;

namespace Nexer.Domain.Interfaces.ZipFacade
{
    public interface IZipFacade
    {
        ZipArchive ReadZipArchive(Stream stream);
        ZipArchiveEntry GetFileByName(ZipArchive zipFile, string fileName);
    }
}
