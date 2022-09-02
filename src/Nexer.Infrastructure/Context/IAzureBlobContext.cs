using Nexer.Domain.Models.DataTransferObjects;
using System.Threading.Tasks;

namespace Nexer.Infrastructure.Context
{
    public interface IAzureBlobContext
    {
        Task<BlobDataDTO> DownloadAsync(string blobFilename);
    }
}
