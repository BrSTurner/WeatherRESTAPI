using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Nexer.Domain.Models.DataTransferObjects;
using System.Threading.Tasks;

namespace Nexer.Infrastructure.Context
{
    public class AzureBlobContext : BlobContainerClient, IAzureBlobContext
    {
        public AzureBlobContext(string connectionString, string containerName) : base(connectionString, containerName)
        {
               
        }

        public async Task<BlobDataDTO> DownloadAsync(string blobFilename)
        {
            
            try
            {
                var file = GetBlobClient(blobFilename);

                if (await file.ExistsAsync())
                {
                    var name = blobFilename;
                    var data = (await file.DownloadStreamingAsync()).Value.Content;

                    return new BlobDataDTO { Content = data, Name = name };
                }
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                
            }

            return null;
        }
    }
}
