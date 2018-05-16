using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

using Newtonsoft.Json;
using StackExchange.Redis;



namespace ImageSimilarity.Functions
{
    using Models;

    public static class ImageMLModelQuery
    {
        [FunctionName("ImageMLModelQuery")]
        public async static Task Run([QueueTrigger("%ResizedImageQueueName%", Connection = "ResizedImageQueueStorageConnectionString")]string myQueueItem, TraceWriter log)
        {
            log.Info($"ML Query Request received : {myQueueItem}");

            QueueNotificationImageUploaded notificationImageUploaded = JsonConvert.DeserializeObject<QueueNotificationImageUploaded>(myQueueItem);
            string trackingDbConnectionString = ConfigurationManager.AppSettings["TrackingDbConnectionString"];

            //Update the result
            ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(trackingDbConnectionString);
            IDatabase database = connectionMultiplexer.GetDatabase();
            TrackingInfo trackingInfo = JsonConvert.DeserializeObject<TrackingInfo>(await database.StringGetAsync(notificationImageUploaded.TrackingId.ToString()));

            //Update Status
            trackingInfo.SearchStatus = "Search Started";
            await database.StringSetAsync(notificationImageUploaded.TrackingId.ToString(),
                                   JsonConvert.SerializeObject(trackingInfo));
            
            string[] defaultImageList = { "0004/12000004fe.jpg", "0004/12010004pm.jpg", "0004/37760004mp.jpg" };

            //Query the model
            string[] images = await QueryMLModel(log, notificationImageUploaded);
            images = images?.Length > 0 ? images : defaultImageList;            
            
            //Update Results
            trackingInfo.SearchStatus = "Search Complete";
            trackingInfo.ImageUrls = new List<string>();
            foreach (string imageUrl in images) { trackingInfo.ImageUrls.Add(imageUrl); }

            await database.StringSetAsync(notificationImageUploaded.TrackingId.ToString(),
                                   JsonConvert.SerializeObject(trackingInfo));

        }

        private static async Task<string[]> QueryMLModel(TraceWriter log, QueueNotificationImageUploaded notificationImageUploaded)
        {
            string[] matchingImages = { };
            try
            {
                HttpClient httpClient = new HttpClient();

                string mlServiceEndpoint = ConfigurationManager.AppSettings["MLQueryServiceEndpoint"];

                /* Only if the ML Service Endpoint is defined */
                if (!String.IsNullOrEmpty(mlServiceEndpoint))
                {
                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage
                    {
                        // Uri: http://52.178.119.58/api/v1/service/irisapp/score
                        RequestUri = new Uri(ConfigurationManager.AppSettings["MLQueryServiceEndpoint"]),
                        Method = HttpMethod.Post
                    };

                    if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["MLQueryServiceAuthHeader"]))
                    {
                        httpRequestMessage.Headers.Add("Authorization", ConfigurationManager.AppSettings["MLQueryServiceAuthHeader"]);
                        //httpRequestMessage.Headers.Add("Authorization", "Bearer 44a392fb3a6a4c30bfa5bc668c10508e");
                    }

                    string httpBody = JsonConvert.SerializeObject(notificationImageUploaded.ImageFile);
                    httpBody = "{\"input_df\": [{\"petal length\": 1.3, \"sepal length\": 3.0, \"sepal width\": 3.6, \"petal width\": 0.25}]}";

                    httpRequestMessage.Content = new StringContent(httpBody, null, "application/json");
                    httpRequestMessage.Content.Headers.Remove("Content-Type");
                    httpRequestMessage.Content.Headers.Add("Content-Type", "application/json");

                    HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
                    string response = await httpResponseMessage.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                log.Error("Error occured communicating with the ML Model Service.", ex);
            }
        
            return matchingImages;
        }
    }
}
