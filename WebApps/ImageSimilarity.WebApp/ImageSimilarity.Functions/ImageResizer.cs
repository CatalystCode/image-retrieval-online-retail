using System.Configuration;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

using Newtonsoft.Json;

using StackExchange.Redis;

using ImageResizer;


namespace ImageSimilarity.Functions
{

    using Models;


    public static class ImageResizer
    {
        [FunctionName("ImageProcessor")]
        public async static Task Run([QueueTrigger("%UploadedImagesQueueName%", 
                                                   Connection = "UploadedImagesQueueStorageConnectionString")]string myQueueItem, 
                                    [Queue("%ResizedImageQueueName%", 
                                            Connection = "ResizedImageQueueStorageConnectionString")] ICollector<QueueNotificationImageUploaded> outputQueue,
                                    TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");

            QueueNotificationImageUploaded notificationImageUploaded = JsonConvert.DeserializeObject<QueueNotificationImageUploaded>(myQueueItem);
            string trackingDbConnectionString = ConfigurationManager.AppSettings["TrackingDbConnectionString"];

            int configuredWidth = 1571;
            int configureHeight = 2000;

            configuredWidth = int.TryParse(ConfigurationManager.AppSettings["PreferredWidth"], out configuredWidth) ? configuredWidth : 1571;
            configureHeight = int.TryParse(ConfigurationManager.AppSettings["PreferredHeight"], out configureHeight) ? configureHeight : 2000;

            await ResizeImage(notificationImageUploaded, configuredWidth, configureHeight);

            UpdateOutboundQueue(outputQueue, notificationImageUploaded);

            await UpdateTrackingDatabase(notificationImageUploaded, trackingDbConnectionString);

        }

        private static async Task ResizeImage(QueueNotificationImageUploaded notificationImageUploaded, int configuredWidth, int configureHeight)
        {
            var instructions = new Instructions
            {
                Width = configuredWidth,
                Height = configureHeight,
                Mode = FitMode.Crop,
                Scale = ScaleMode.Both
            };


            CloudStorageAccount inputStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["UploadedImageStorageConnectionString"]);
            CloudBlobClient inputBlobClient = inputStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer inputContainer = inputBlobClient.GetContainerReference(ConfigurationManager.AppSettings["UploadedImageContainer"]);
            CloudBlob inputBlob = inputContainer.GetBlobReference(notificationImageUploaded.ImageFile);

           
            if (inputBlob != null)
            {
                using (var inputBlobStream = inputBlob.OpenRead())
                {
                    CloudStorageAccount outputBlobStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["ResizedImageStorageConnectionString"]);
                    CloudBlobClient outputBlobClient = outputBlobStorageAccount.CreateCloudBlobClient();
                    CloudBlobContainer outputContainer = outputBlobClient.GetContainerReference(ConfigurationManager.AppSettings["ResizedImageContainer"]);
                    CloudBlockBlob outputBlockBlob = outputContainer.GetBlockBlobReference("rs_" + notificationImageUploaded.ImageFile);

                    using (MemoryStream myStream = new MemoryStream())
                    {
                        // Resize the image with the given instructions into the stream.
                        ImageBuilder.Current.Build(new ImageJob(inputBlobStream, myStream, instructions));

                        // Reset the stream's position to the beginning.
                        myStream.Position = 0;

                        // Write the stream to the new blob.
                        await outputBlockBlob.UploadFromStreamAsync(myStream);
                    }
                }
            }
        }

        private static void UpdateOutboundQueue(ICollector<QueueNotificationImageUploaded> outputQueue, QueueNotificationImageUploaded notificationImageUploaded)
        {
            //update with resized filename
            notificationImageUploaded.ImageFile = "rs_" + notificationImageUploaded.ImageFile;
            outputQueue.Add(notificationImageUploaded);          
        }


        private static async Task UpdateTrackingDatabase(QueueNotificationImageUploaded notificationImageUploaded, string trackingDbConnectionString)
        {
            //Update Azure Redis cache
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(trackingDbConnectionString);
            IDatabase database = connectionMultiplexer.GetDatabase();

            TrackingInfo trackingInfo = JsonConvert.DeserializeObject<TrackingInfo>(await database.StringGetAsync(notificationImageUploaded.TrackingId.ToString()));
            trackingInfo.SearchStatus = "Image Resized";

            await database.StringSetAsync(notificationImageUploaded.TrackingId.ToString(),
                                    JsonConvert.SerializeObject(trackingInfo));
        }

    }
}
