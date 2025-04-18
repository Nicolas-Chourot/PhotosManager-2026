using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotosManager.Controllers
{
    public class NotificationsController : Controller
    {
        public JsonResult Pop()
        {
            string message = DB.Notifications.Pop();
            return Json(message, JsonRequestBehavior.AllowGet);
        }
    }
}