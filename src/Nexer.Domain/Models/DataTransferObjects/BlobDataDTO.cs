using System.IO;

namespace Nexer.Domain.Models.DataTransferObjects
{
    public class BlobDataDTO
    {
        public string Name { get; set; }
        public Stream Content { get; set; }
    }
}
