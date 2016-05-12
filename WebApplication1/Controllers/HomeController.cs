using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.App_Start;
using Microsoft.Practices.Unity;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            //looks for a 'callable' service instance with the least number of queries made from it
            this.IpLocation = UnityConfig.GetConfiguredContainer().ResolveAll<Services.IpLocation.ILocationService>()
                .Where(x => x.IsUnderThresholdLimit)
                .OrderBy(x => x.NumberOfQueriesMade)
                .FirstOrDefault();
        }

        /// <summary>
        /// the chosen instance
        /// </summary>
        readonly Services.IpLocation.ILocationService IpLocation;

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult WhereAmI()
        {
            var ip = string.Empty;

            //needs to get the public ip address from 'localhost'
            var url = "https://api.ipify.org/?format=json";
            var res = WebRequest.CreateHttp(url).GetResponse();
            using (res)
            {
                var stream = res.GetResponseStream();
                var reader = new StreamReader(stream);
                using (reader)
                {
                    var json = JObject.Parse(reader.ReadToEnd());
                    ip = json.Value<string>("ip");
                }
            }

            var userlocation = this.IpLocation.Find(ip);
            return View(userlocation);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}