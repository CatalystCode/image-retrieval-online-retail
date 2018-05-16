using System;
using System.Collections.Generic;

namespace ImageSimilarity.Web.Models
{
    public class TrackingInfo
    {
        public string Filename { get; set; }

        public Guid TrackingId { get; set; }

        public string SearchStatus { get; set; }       

        public List<string> ImageUrls { get; set; }

        

    }
}
