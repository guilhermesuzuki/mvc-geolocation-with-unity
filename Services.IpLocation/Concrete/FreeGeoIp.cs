﻿using Newtonsoft.Json.Linq;
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
    /// location service implementation for freegeoip
    /// </summary>
    public class FreeGeoIp : ILocationService
    {
        /// <summary>
        /// Sync Root object for multi-threading
        /// </summary>
        static object SyncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public FreeGeoIp()
        {

        }

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
                var key = "freegeoip-threshold";

                if (HttpContext.Current.Cache[key] == null)
                {
                    lock (SyncRoot)
                    {
                        if (HttpContext.Current.Cache[key] == null)
                        {
                            //first call within the hour limit
                            HttpContext.Current.Cache.Add(key, 0, null, DateTime.Now.AddHours(1), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                        }
                    }
                }

                return (int)HttpContext.Current.Cache[key];
            }

            set
            {
                //dummy call to create cache (if needed)
                var current = this.NumberOfQueriesMade;
                HttpContext.Current.Cache["freegeoip-threshold"] = value;
            }
        }

        /// <summary>
        /// According to the website, up to 10000 queries an hour
        /// </summary>
        public int ThresoldLimit
        {
            get { return 10000; }
        }

        /// <summary>
        /// Tries to find a location from an ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public LocationModel Find(string ip)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ip) == false)
                {
                    var url = "http://freegeoip.net/json/" + ip;
                    var req = WebRequest.CreateHttp(url);
                    var res = req.GetResponse();

                    //adds this call to the current threshold
                    this.NumberOfQueriesMade += 1;

                    using (res)
                    {
                        var stream = res.GetResponseStream();
                        var reader = new StreamReader(stream);
                        var json = JObject.Parse(reader.ReadToEnd());

                        //{"ip":"174.119.112.99","country_code":"CA","country_name":"Canada","region_code":"ON","region_name":"Ontario","city":"Toronto","zip_code":"M6E","time_zone":"America/Toronto","latitude":43.6889,"longitude":-79.4507,"metro_code":0}

                        return new LocationModel(ip)
                        {
                            City = json.Value<string>("city"),
                            Country = json.Value<string>("country_name"),
                            CountryCode = json.Value<string>("country_code"),
                            Region = json.Value<string>("region_name"),
                            RegionCode = json.Value<string>("region_code"),
                            Latitude = string.IsNullOrWhiteSpace(json.Value<string>("latitude")) == false ? json.Value<float>("latitude") : (float?)null,
                            Longitude = string.IsNullOrWhiteSpace(json.Value<string>("longitude")) == false ? json.Value<float>("longitude") : (float?)null,
                            ZipCode = json.Value<string>("zip_code"),
                            TimeZone = json.Value<string>("time_zone")
                        };
                    }
                }
            }
            catch (Exception)
            {
                //TODO: log this error?
            }
            finally
            {

            }

            return null;
        }
    }
}