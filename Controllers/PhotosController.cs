using PhotosManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer.Symbols;
using System.Web.Services.Description;
using static PhotosManager.Controllers.AccessControl;

namespace PhotosManager.Controllers
{
    [UserAccess]
    public class PhotosController : Controller
    {
        const string IllegalAccessUrl = "/Accounts/Login?message=Tentative d'accès illégal!&success=false";

        public ActionResult ToggleSearch()
        {
            Session["ShowSearch"] = !(bool)Session["ShowSearch"];
            return RedirectToAction("List");
        }

        public ActionResult ToggleSortDirection()
        {
            Session["Ascendant"] = !(bool)Session["Ascendant"];
            return RedirectToAction("List");
        }
        public ActionResult SetPhotoOwnerSearchId(int id)
        {
            Session["photoOwnerSearchId"] = id;
            return RedirectToAction("List");
        }
        public ActionResult SetSearchKeywords(string keywords)
        {
            Session["searchKeywords"] = keywords;
            return RedirectToAction("List");
        }
        public List<Photo> SortPhotos()
        {
            DB.Photos.ResetLikesCount();
            List<Photo> list = DB.Photos.ToList();

            if ((bool)Session["ShowSearch"])
            {

                List<Photo> templist = new List<Photo>();
                string[] keywords = ((string)Session["searchKeywords"]).Split(' ');
                foreach (var photo in list)
                {
                    bool keep = true;

                    switch ((int)Session["photoOwnerSearchId"])
                    {
                        case 0: break;
                        case -1: keep = photo.OwnerId == ((User)Session["ConnectedUser"]).Id; break;
                        default: keep = photo.OwnerId == (int)Session["photoOwnerSearchId"]; break;
                    }
                    if (!string.IsNullOrEmpty((string)Session["searchKeywords"]))
                    {
                        foreach (string keyword in keywords)
                        {
                            string kw = keyword.Trim().ToLower();
                            string userName = photo.Owner.Name;
                            if (!photo.Title.ToLower().Contains(kw) &&
                                !photo.Description.ToLower().Contains(kw) &&
                                !userName.ToLower().Contains(kw))
                            {
                                keep = false;
                                break;
                            }
                        }
                    }
                    if (keep)
                        templist.Add(photo);
                }
                list = templist;
            }
            List<Photo> tmp = new List<Photo>();
            int ownerId = ((User)Session["ConnectedUser"]).Id;
            switch ((string)Session["PhotosSortType"])
            {
                case "date":
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.CreationDate).ToList();
                    break;
                case "ownerLike":
                    foreach (Photo photo in list)
                        if (photo.Likes.ToList().Where(l => l.UserId == ownerId).Any())
                            tmp.Add(photo);
                    list = tmp;
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.Likes.Count).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.Likes.Count).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "ownerDontLike":
                    foreach (Photo photo in list)
                        if (!photo.Likes.ToList().Where(l => l.UserId == ownerId).Any())
                            tmp.Add(photo);
                    list = tmp;
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.Likes.Count).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.Likes.Count).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                default:
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.Likes.Count).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.Likes.Count).ThenByDescending(p => p.CreationDate).ToList();
                    break;
            }

            return list;
        }
        public ActionResult GetPhotos(bool forceRefresh = false)
        {
            if (forceRefresh || DB.Photos.HasChanged || DB.Likes.HasChanged || DB.Users.HasChanged)
            {
                return PartialView(SortPhotos());
            }
            return null;
        }
        public ActionResult List(string sortType = "")
        {
            Session["id"] = null;
            Session["IsOwner"] = null;
            if (Session["Ascendant"] == null) Session["Ascendant"] = false;
            if (Session["photoOwnerSearchId"] == null) Session["photoOwnerSearchId"] = 0;
            if (Session["searchKeywords"] == null) Session["searchKeywords"] = "";
            if (Session["PhotosSortType"] == null) Session["PhotosSortType"] = "date";
            Session["PhotosSortType"] = sortType != "" ? sortType : Session["PhotosSortType"];
            if (Session["ShowSearch"] == null) Session["ShowSearch"] = false;
            Session["UsersList"] = DB.Users.ToList().OrderBy(u => u.Name).ToList();
            return View();
        }
        public ActionResult Create()
        {
            return View(new Photo());
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Create(Photo photo)
        {
            photo.OwnerId = ((User)Session["ConnectedUser"]).Id;
            photo.CreationDate = DateTime.Now;
            DB.Photos.Add(photo);
            Session["PhotosSortType"] = "date";
            Session["Ascendant"] = false;
            return RedirectToAction("List");
        }
        public ActionResult Edit()
        {
            if (Session["id"] != null && Session["IsOwner"] != null && (bool)Session["IsOwner"])
            {
                int id = (int)Session["id"];
                Photo photo = DB.Photos.Get(id);
                User connectedUser = (User)Session["ConnectedUser"];
                if (photo != null)
                {
                    if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                    {
                        return View(photo);
                    }
                    return Redirect(IllegalAccessUrl);
                }
            }
            return Redirect(IllegalAccessUrl);
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(Photo photo)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            if (Session["IsOwner"] != null ? (bool)Session["IsOwner"] : false)
            {
                Photo storedPhoto = DB.Photos.Get((int)Session["id"]);
                photo.Id = storedPhoto.Id;
                photo.OwnerId = storedPhoto.OwnerId;
                photo.CreationDate = storedPhoto.CreationDate;
                DB.Photos.Update(photo);
                Session["PhotosSortType"] = "date";
                return RedirectToAction("List");
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult Details(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                Session["id"] = id;
                Session["commentId"] = 0; // photo comment
                User connectedUser = ((User)Session["ConnectedUser"]);
                Session["IsOwner"] = connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id;
                if ((bool)Session["IsOwner"] || photo.Shared)
                    return View(photo);
                else
                    return Redirect(IllegalAccessUrl);
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult Delete()
        {
            if (Session["IsOwner"] != null ? (bool)Session["IsOwner"] : false)
            {
                int id = (int)Session["id"];
                Photo photo = DB.Photos.Get(id);
                if (photo != null)
                {
                    User connectedUser = (User)Session["ConnectedUser"];
                    if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                    {
                        DB.Photos.Delete(id);
                        Session["PhotosSortType"] = "date";
                        Session["Ascendant"] = false;
                        return RedirectToAction("List");
                    }
                    else
                        return Redirect(IllegalAccessUrl);
                }
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult TogglePhotoLike(int id)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            DB.Likes.ToggleLike(id, connectedUser.Id);
            return RedirectToAction("Details/" + id);
        }

        public ActionResult Comments(int photoId, int commentId = 0)
        {
            List<Comment> comments = DB.Comments.ToList().Where(c => c.PhotoId == photoId && c.CommentId == commentId).ToList();

            return PartialView(comments);
        }
        public ActionResult GetComments(bool forceRefresh = false)
        {
            int photoId = (int)Session["id"];
            
            if (forceRefresh || true || DB.Comments.HasChanged)
            {
                List<Comment> comments = DB.Comments.ToList().Where(c => c.PhotoId == photoId && c.CommentId == 0).ToList();

                return PartialView(comments);
            }
            return null;
        }
        public ActionResult CreateComment(int parentId, string text)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = new Comment();
            comment.CommentId = parentId;
            comment.PhotoId = (int)Session["id"];   
            comment.UserId = connectedUser.Id;
            comment.Text = text;
            comment.CreationDate = DateTime.Now;    
            DB.Comments.Add(comment);
            return null;
        }
        public ActionResult UpdateComment(int id, string text)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = DB.Comments.Get(id);
            if (comment != null && comment.User.Id == connectedUser.Id)
            {
                comment.Text = text;
                DB.Comments.Update(comment);
            }
            return null;
        }
        public ActionResult DeleteComment(int id)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = DB.Comments.Get(id);
            if (comment != null && comment.User.Id == connectedUser.Id)
            {
                List<Comment> responses = DB.Comments.ToList().Where(c => c.CommentId == comment.Id).ToList();
                foreach (Comment response in responses)
                {
                    DB.Comments.Delete(response.Id);
                }
                DB.Comments.Delete(comment.Id);
            }
            return null;
        }
    }
}