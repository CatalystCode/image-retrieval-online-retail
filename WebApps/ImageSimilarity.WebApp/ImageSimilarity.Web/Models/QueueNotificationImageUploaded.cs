﻿using System;

namespace ImageSimilarity.Web.Models
{
    public class QueueNotificationImageUploaded
    {

        public Guid TrackingId { get; set; }
        public string ImageFile { get; set; }
    }
}
