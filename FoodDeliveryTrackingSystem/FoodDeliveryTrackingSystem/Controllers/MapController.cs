using PusherServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FoodDeliveryTrackingSystem.Controllers
{
    public class MapController : Controller
    {
        private Pusher pusher;

        public MapController()
        {
            var options = new PusherOptions();
            options.Cluster = "app-cluster";

          pusher = new Pusher(
        "id",
        "yourkey",
        "your secret key",
        options);
        }

        [HttpPost]
        public JsonResult Index()
        {
            var latitude = Request.Form["lat"];
            var longitude = Request.Form["lng"];

            var location = new
            {
                latitude = latitude,
                longitude = longitude
            };

            pusher.TriggerAsync("location_channel", "new_location", location);

            return Json(new { status = "success", data = location });
        }

    }
}