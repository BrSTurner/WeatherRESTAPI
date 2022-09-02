using Nexer.Domain.Models.DataTransferObjects;
using System.Threading.Tasks;

namespace Nexer.Domain.Repositories
{
    public interface IWeatherStorageRepository
    {
        Task<BlobDataDTO> GetFileFromAzureBlobStorage(string fileName);

    }
}
