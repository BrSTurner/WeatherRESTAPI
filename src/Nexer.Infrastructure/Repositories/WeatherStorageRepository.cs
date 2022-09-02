using Nexer.Domain.Models.DataTransferObjects;
using Nexer.Domain.Repositories;
using Nexer.Infrastructure.Context;
using System.Threading.Tasks;

namespace Nexer.Infrastructure.Repositories
{
    public sealed class WeatherStorageRepository : IWeatherStorageRepository
    {
        private readonly IAzureBlobContext _context;

        public WeatherStorageRepository(
            IAzureBlobContext context)
        {
            _context = context;
        }

        public async Task<BlobDataDTO> GetFileFromAzureBlobStorage(string fileName)
        {
            return await _context.DownloadAsync(fileName);
        }
    }
}
