module ProcessBlobCreated

open Microsoft.Azure.WebJobs
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Azure.Storage.Blobs
open System
open System.IO
open System.Threading.Tasks

[<FunctionName("ProcessBlobCreated")>]
let run ([<EventGridTrigger>] eventGridEvent: Newtonsoft.Json.Linq.JObject, log: ILogger) =
    task {
        try
            log.LogInformation($"Processing Event Grid event: {eventGridEvent}")

            // Extract blob information from the Event Grid event
            let data = eventGridEvent.["data"]
            let url = data.["url"].ToString()
            let blobName = Uri(url).Segments |> Array.last

            log.LogInformation($"Blob URL: {url}")
            log.LogInformation($"Blob Name: {blobName}")

            // Storage account connection string
            let connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")

            // Initialize Blob clients
            let photosContainer = BlobContainerClient(connectionString, "photos")
            let metadataContainer = BlobContainerClient(connectionString, "metadata")
            let metadataBlob = metadataContainer.GetBlobClient("metadata.json")

            // Download the existing metadata.json (if exists)
            let! metadataJson =
                task {
                    try
                        let! stream = metadataBlob.OpenReadAsync()
                        use reader = new StreamReader(stream)
                        let! content = reader.ReadToEndAsync()
                        return content
                    with :? Azure.RequestFailedException as ex when ex.Status = 404 ->
                        log.LogInformation("No existing metadata.json found. Creating a new one.")
                        return "[]"
                }

            // Parse and update metadata
            let mutable metadata =
                JsonConvert.DeserializeObject<System.Collections.Generic.List<obj>>(metadataJson)

            let newEntry =
                {| url = url
                   name = blobName
                   uploaded = DateTime.UtcNow |}

            metadata.Add(newEntry)

            // Write updated metadata back to Blob Storage
            let updatedJson = JsonConvert.SerializeObject(metadata, Formatting.Indented)
            use stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(updatedJson))
            do! metadataBlob.UploadAsync(stream, overwrite = true)

            log.LogInformation("Successfully updated metadata.json.")
        with ex ->
            log.LogError($"An error occurred: {ex.Message}")
    }
