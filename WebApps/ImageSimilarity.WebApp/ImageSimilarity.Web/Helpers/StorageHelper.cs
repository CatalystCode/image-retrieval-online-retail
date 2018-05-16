using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImageSimilarity.Web.Helpers
{
    using Models;
    public static class StorageHelper
    {

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(fileStream);



            return await Task.FromResult(true);
        }


        public static async Task<bool> QueueMessageForProcessingImage(QueueNotificationImageUploaded notificationMessage, AzureStorageConfig queueStorageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(queueStorageConfig.AccountName, queueStorageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference(queueStorageConfig.QueueName);

            // Create the queue if it doesn't already exist
            await queue.CreateIfNotExistsAsync();

            // Create a message and add it to the queue.
           CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(notificationMessage));
            await queue.AddMessageAsync(message);

            return await Task.FromResult(true);
        }

        public async static Task<bool> StartTracking(Guid TrackigId, string storeConnectionString)
        {
            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            List<string> thumbnailUrls = new List<string>();

            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ThumbnailContainer);

            // Set the permission of the container to public
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            BlobContinuationToken continuationToken = null;

            BlobResultSegment resultSegment = null;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            do
            {
                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                //or by calling a different overload.
                resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);

                foreach (var blobItem in resultSegment.Results)
                {
                    thumbnailUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());
                }

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }

            while (continuationToken != null);

            return await Task.FromResult(thumbnailUrls);
        }


        public static async Task<List<string>> GetImageUrls(AzureStorageConfig _storageConfig, List<string> images)
        {
            List<string> imageUrls = new List<string>();

            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Set the permission of the container to public
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            
            foreach (var imageFile in images)
            {
                CloudBlob blobItem = container.GetBlobReference(imageFile);
                if (blobItem != null)
                {
                    imageUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());
                }
            }            
            
            return await Task.FromResult(imageUrls);
        }



    }
}
