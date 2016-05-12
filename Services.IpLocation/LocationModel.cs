using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IpLocation
{
    /// <summary>
    /// location model
    /// </summary>
    public class LocationModel
    {
        public LocationModel()
        {

        }

        public LocationModel(string _ip): this()
        {
            this.Ip = _ip;
        }

        /// <summary>
        /// the ip that corresponds to this location
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// longitude of the location
        /// </summary>
        public float? Longitude { get; set; }
        /// <summary>
        /// latitude of the location
        /// </summary>
        public float? Latitude { get; set; }

        /// <summary>
        /// name of the city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State, province or whatever
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Region code
        /// </summary>
        public string RegionCode { get; set; }

        /// <summary>
        /// name of the country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Country code
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// ZipCode when present
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// Timezone
        /// </summary>
        public string TimeZone { get; set; }
    }
}
