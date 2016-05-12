using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace Services.IpLocation.Concrete
{
    /// <summary>
    /// Ip Info Db location service
    /// </summary>
    public class IpInfoDb : ILocationService
    {
        /// <summary>
        /// Needs an API Key to begin with
        /// </summary>
        /// <param name="_apiKey"></param>
        public IpInfoDb(string _apiKey)
        {
            this.ApiKey = _apiKey;
        }

        /// <summary>
        /// Api Key (service provider requires one, so register it first)
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// Sync Root object for multi-threading
        /// </summary>
        static object SyncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public bool IsUnderThresholdLimit
        {
            get
            {
                return this.NumberOfQueriesMade < this.ThresoldLimit;
            }
        }

        /// <summary>
        /// Number of calls made within the time threshold
        /// </summary>
        public int NumberOfQueriesMade
        {
            get
            {
                var key = "ipinfodb-threshold-" + ApiKey;

                if (HttpContext.Current.Cache[key] == null)
                {
                    lock (SyncRoot)
                    {
                        if (HttpContext.Current.Cache[key] == null)
                        {
                            //although there's no limit for queries
                            HttpContext.Current.Cache.Add(key, 0, null, DateTime.Now.AddHours(5), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                        }
                    }
                }

                return (int)HttpContext.Current.Cache[key];
            }

            set
            {
                //dummy call to create cache (if needed)
                var current = this.NumberOfQueriesMade;
                HttpContext.Current.Cache["ipinfodb-threshold-" + ApiKey] = value;
            }
        }

        /// <summary>
        /// According to the website, up to 10000 queries an hour
        /// </summary>
        public int ThresoldLimit
        {
            get { return int.MaxValue; }
        }

        public LocationModel Find(string ip)
        {
            //composed url
            var url = "http://api.ipinfodb.com/v3/ip-city/?key=" + this.ApiKey + "&ip=" + ip + "&format=json";
            var req = WebRequest.CreateHttp(url);
            var res = req.GetResponse();

            //adds this call to the number of queries
            this.NumberOfQueriesMade += 1;

            using (res)
            {
                var stream = res.GetResponseStream();
                var reader = new StreamReader(stream);
                var json = JObject.Parse(reader.ReadToEnd());

                //sample response from it
                //{
                //    "statusCode" : "ERROR",
                // "statusMessage" : "Invalid API key.",
                // "ipAddress" : "74.125.45.100",
                // "countryCode" : "",
                // "countryName" : "",
                // "regionName" : "",
                // "cityName" : "",
                // "zipCode" : "",
                // "latitude" : "0",
                // "longitude" : "0",
                // "timeZone" : ""
                //}

                if (json.Value<string>("statusCode") != "ERROR")
                {
                    return new LocationModel(ip)
                    {
                        City = json.Value<string>("cityName"),
                        Country = json.Value<string>("countryName"),
                        CountryCode = json.Value<string>("countryCode"),
                        Region = json.Value<string>("regionName"),
                        Latitude = string.IsNullOrWhiteSpace(json.Value<string>("latitude")) == false ? json.Value<float>("latitude") : (float?)null,
                        Longitude = string.IsNullOrWhiteSpace(json.Value<string>("longitude")) == false ? json.Value<float>("longitude") : (float?)null,
                        ZipCode = json.Value<string>("zipCode"),
                        TimeZone = json.Value<string>("timeZone")
                    };
                }
            }

            return null;
        }
    }
}
