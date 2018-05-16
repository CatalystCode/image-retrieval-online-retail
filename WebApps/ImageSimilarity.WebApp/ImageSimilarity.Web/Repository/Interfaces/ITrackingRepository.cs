using System;
using System.Threading.Tasks;

namespace ImageSimilarity.Web.Repository.Interfaces
{
    using Models;

    public interface ITrackingRepository
    {
        Task<bool> AddOrUpdateTracking(Guid trackingkey, TrackingInfo trackingInfo);

        Task<TrackingInfo> GetTracking(Guid trackingKey);
    }
}
