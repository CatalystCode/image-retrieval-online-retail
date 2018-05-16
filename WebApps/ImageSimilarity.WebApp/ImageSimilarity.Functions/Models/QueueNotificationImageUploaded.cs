using System;

namespace ImageSimilarity.Functions.Models
{
    public class QueueNotificationImageUploaded
    {

        public Guid TrackingId { get; set; }
        public string ImageFile { get; set; }
    }
}
