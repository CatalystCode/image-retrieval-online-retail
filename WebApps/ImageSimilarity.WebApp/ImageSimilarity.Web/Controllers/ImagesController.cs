using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;


namespace ImageSimilarity.Web.Controllers
{

    using Helpers;    
    using Models;
    using Repository.Interfaces;

    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig storageConfig = null;

        private readonly AzureStorageConfig outBoundNotificationQueue = null;

        private readonly AzureStorageConfig catalogStorageConfig = null;

        private ITrackingRepository trackingRepository = null;

        public ImagesController(IOptions<Dictionary<string, AzureStorageConfig>> config, ITrackingRepository trackingRepository)
        {
            this.storageConfig = config.Value["upload-image-container"];
            this.outBoundNotificationQueue = config.Value["outbound-notification-queue"];
            this.catalogStorageConfig = config.Value["catalog-image-container"];
            this.trackingRepository = trackingRepository;
        }

        // POST /api/images/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            bool isUploaded = false;
            bool isNotified = false;
            bool isTracking = false;

            try
            {

                if (files.Count == 0)

                    return BadRequest("No files received from the upload");

                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)

                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                var trackingId = Guid.Empty;

                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                                if (isUploaded)
                                {
                                    trackingId = Guid.NewGuid();
                                    isNotified = await StorageHelper.QueueMessageForProcessingImage(
                                                                        new QueueNotificationImageUploaded()                                            {
                                                                            ImageFile = formFile.FileName,
                                                                            TrackingId = trackingId
                                                                        }, 
                                                                        outBoundNotificationQueue);

                                    if (isNotified)
                                    {
                                        isTracking = await trackingRepository.
                                                            AddOrUpdateTracking(trackingId, 
                                                                                new TrackingInfo()
                                                                            {
                                                                                Filename = formFile.FileName,
                                                                                TrackingId = trackingId,
                                                                                SearchStatus = "Image Uploaded",
                                                                                ImageUrls = null
                                                                            });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                {
                    if (storageConfig.ThumbnailContainer != string.Empty)
                    {
                        return new AcceptedResult("", trackingId);
                    }
                    else
                    {
                        return new AcceptedResult();
                    }
                }
                else
                {
                    return BadRequest("Look like the image couldnt upload to the storage");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails()
        {

            try
            {
                if (storageConfig.AccountKey == string.Empty || storageConfig.AccountName == string.Empty)

                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (storageConfig.ImageContainer == string.Empty)

                    return BadRequest("Please provide a name for your image container in the azure blob storage");

                List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(storageConfig);

                return new ObjectResult(thumbnailUrls);
            
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("track/{id}")]
        public async Task<IActionResult> GetTrackingInfo(string id)
        {
            TrackingInfo trackingInfo = await trackingRepository.GetTracking(Guid.Parse(id));
            return new ObjectResult(trackingInfo);
        }

        [HttpGet("matches/{id}")]
        public async Task<IActionResult> GetMatches(string id)
        {
            List<string> imageUrls = new List<string>();
            TrackingInfo trackingInfo = await trackingRepository.GetTracking(Guid.Parse(id));
            if (trackingInfo.ImageUrls != null)
            {
                imageUrls = await StorageHelper.GetImageUrls(catalogStorageConfig, trackingInfo.ImageUrls);
            }

            return new ObjectResult(imageUrls);

        }

    }
}