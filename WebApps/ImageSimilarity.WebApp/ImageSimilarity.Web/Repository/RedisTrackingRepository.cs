using System;
using System.Threading.Tasks;

using Newtonsoft.Json;

using StackExchange.Redis;

namespace ImageSimilarity.Web.Repository
{
    using Models;
    using Interfaces;

    public class RedisTrackingRepository : ITrackingRepository
    {

        public String ConnectionString { get; private set; }

        public String Database { get; set; }

        public ConnectionMultiplexer connectionMultiplexer;
       

        public RedisTrackingRepository(string connectionString)
        {
            this.ConnectionString = connectionString;
            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
        }


        public async Task<bool> AddOrUpdateTracking(Guid trackingkey, TrackingInfo trackingInfo)
        {
            IDatabase cache = connectionMultiplexer.GetDatabase();
            bool result = await cache.StringSetAsync(trackingInfo.TrackingId.ToString(), JsonConvert.SerializeObject(trackingInfo));

            return result;
        }

        public async Task<TrackingInfo> GetTracking(Guid trackingKey)
        {
            IDatabase cache = connectionMultiplexer.GetDatabase();
            TrackingInfo trackingInfo = JsonConvert.DeserializeObject<TrackingInfo>(await cache.StringGetAsync(trackingKey.ToString()));

            return trackingInfo;               
        }

      
    }
}
