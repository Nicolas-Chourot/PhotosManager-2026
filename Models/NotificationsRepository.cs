using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class NotificationsRepository : Repository<Notification>
    {
        public void Push(int targetUserId, string Message)
        {
            User connectedUser = (User)HttpContext.Current.Session["ConnectedUser"];
            if (connectedUser != null && connectedUser.Notify)
                Add(new Notification { TargetUserId = targetUserId, Message = Message });
        }
        public string Pop()
        {
            User connectedUser = (User)HttpContext.Current.Session["ConnectedUser"];
            if (connectedUser != null)
            {
                Notification notification = ToList().Where(n => n.TargetUserId == connectedUser.Id).FirstOrDefault()?.Copy();
                if (notification != null)
                {
                    Delete(notification.Id);
                    return notification.Message;
                }
            }
            return null;
        }
    }
}