using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccessBlobStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;

        public NotesController(BlobServiceClient blobServiceClient)
        {
            this._blobServiceClient = blobServiceClient;
        }

        [HttpPost("{blobName}")]
        public async Task AddLine(string blobName, [FromQuery] string line)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("notes");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            var oldContent = await ReadWholeContent(blobClient);
            var newContent = line;
            if (oldContent != null)
            {
                newContent = string.Join('\n', new string[] { oldContent, line });
            }
            await WriteWholeContent(blobClient, newContent);
        }

        private async Task<string?> ReadWholeContent(BlobClient blobClient)
        {
            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            using var read = await blobClient.OpenReadAsync();
            using var streamReader = new StreamReader(read);
            return await streamReader.ReadToEndAsync();
        }

        private async Task WriteWholeContent(BlobClient blobClient, string content)
        {
            using var writer = await blobClient.OpenWriteAsync(true);
            using var streamWriter = new StreamWriter(writer);
            await streamWriter.WriteAsync(content);
        }

        public class CreateSasTokenResponse
        {
            public string Token { get; set; }
        }

        [HttpPost("CreateSas/{blobName}")]
        public async Task<ActionResult<CreateSasTokenResponse>> CreateSas(string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("notes");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync())
            {
                return NotFound();
            }

            BlobSasBuilder sasBuilder = new()
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(20)
            };

            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            // blobClient.CanGenerateSasUri

            var uri = blobClient.GenerateSasUri(sasBuilder);
            return new CreateSasTokenResponse
            {
                Token = uri.ToString(),
            };
        }
    }
}
