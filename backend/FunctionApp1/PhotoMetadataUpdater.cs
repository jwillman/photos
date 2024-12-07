using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PhotoMetadataUpdater
{
    public static class UpdateMetadataFunction
    {
        private const string MetadataFileName = "metadata.json";
        private const string BlobContainerName = "photos";

        [Function(nameof(PhotoMetadataUpdater))]
        public static async Task Run(
            [BlobTrigger("photos/{name}", Connection = "AzureWebJobsStorage")]
            Stream blobStream,
            string name,
            Uri uri,
            ILogger log)
        {
            log.LogInformation($"Blob trigger function processing blob: {name}");

            // Skip metadata.json file itself
            if (name.Equals(MetadataFileName, StringComparison.OrdinalIgnoreCase))
            {
                log.LogInformation("Skipped updating metadata.json as it triggered the function.");
                return;
            }

            // Check if the environment variable is set
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                log.LogError("AzureWebJobsStorage environment variable is not set.");
                throw new InvalidOperationException("AzureWebJobsStorage environment variable is not set.");
            }

            // Initialize BlobServiceClient and BlobContainerClient
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(BlobContainerName);

            // Ensure the container exists
            if (!await containerClient.ExistsAsync())
            {
                log.LogError($"Blob container '{BlobContainerName}' does not exist.");
                throw new InvalidOperationException($"Blob container '{BlobContainerName}' does not exist.");
            }

            // Get the existing metadata file
            BlobClient metadataBlobClient = containerClient.GetBlobClient(MetadataFileName);
            MetadataFile metadataFile = await GetExistingMetadataAsync(metadataBlobClient, log);

            // Create new photo metadata entry
            PhotoMetadata newPhotoMetadata = new PhotoMetadata
            {
                FileName = name,
                Uri = uri.ToString(),
                UploadDate = DateTime.UtcNow
            };

            log.LogInformation($"Adding new photo metadata: {JsonSerializer.Serialize(newPhotoMetadata)}");

            // Update the metadata file
            metadataFile.Photos.Add(newPhotoMetadata);
            await UploadMetadataFileAsync(metadataBlobClient, metadataFile, log);

            log.LogInformation("Successfully updated metadata.json.");
        }

        private static async Task<MetadataFile> GetExistingMetadataAsync(BlobClient metadataBlobClient, ILogger log)
        {
            try
            {
                if (await metadataBlobClient.ExistsAsync())
                {
                    BlobDownloadInfo download = await metadataBlobClient.DownloadAsync();
                    using var streamReader = new StreamReader(download.Content);
                    string json = await streamReader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<MetadataFile>(json) ?? new MetadataFile();
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to read existing metadata.json: {ex.Message}");
                throw; // Rethrow the exception to inform the caller
            }

            // Return an empty metadata file if none exists or deserialization fails
            return new MetadataFile();
        }

        private static async Task UploadMetadataFileAsync(BlobClient metadataBlobClient, MetadataFile metadataFile, ILogger log)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                await JsonSerializer.SerializeAsync(memoryStream, metadataFile, new JsonSerializerOptions { WriteIndented = true });
                memoryStream.Position = 0;
                await metadataBlobClient.UploadAsync(memoryStream, overwrite: true);
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to upload metadata.json: {ex.Message}");
                throw;
            }
        }

        // Classes to represent metadata structure
        public class MetadataFile
        {
            [JsonPropertyName("photos")] public List<PhotoMetadata> Photos { get; set; } = new List<PhotoMetadata>();
        }

        public class PhotoMetadata
        {
            [JsonPropertyName("fileName")] public string FileName { get; set; }

            [JsonPropertyName("uri")] public string Uri { get; set; }

            [JsonPropertyName("uploadDate")] public DateTime UploadDate { get; set; }
        }
    }
}