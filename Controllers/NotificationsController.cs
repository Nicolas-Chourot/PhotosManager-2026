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
            Notification notification = DB.Notifications.Pop();
            return Json(new { notification.User.Avatar, notification.Message}, JsonRequestBehavior.AllowGet);
        }
    }
}