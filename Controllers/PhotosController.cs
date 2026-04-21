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
            DB.Events.Add("ToggleSearch");
            Session["ShowSearch"] = !(bool)Session["ShowSearch"];
            return RedirectToAction("List");
        }

        public ActionResult ToggleSortDirection()
        {
            DB.Events.Add("ToggleSortDirection");
            Session["Ascendant"] = !(bool)Session["Ascendant"];
            return RedirectToAction("List");
        }
        public ActionResult SetPhotoOwnerSearchId(int id)
        {
            DB.Events.Add("SetPhotoOwnerSearchId");
            Session["photoOwnerSearchId"] = id;
            return RedirectToAction("List");
        }
        public ActionResult SetSearchKeywords(string keywords)
        {
            DB.Events.Add("SetSearchKeywords", keywords);
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
                case "dates":
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.CreationDate).ToList();
                    break;
                case "comments":
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.CommentsCount).ToList();
                    else
                        list = list.OrderByDescending(p => p.CommentsCount).ToList();
                    break;
                case "likes":
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.LikesCount).ToList();
                    else
                        list = list.OrderByDescending(p => p.LikesCount).ToList();
                    break;
                case "ownerLikes":
                    foreach (Photo photo in list)
                        if (photo.Likes.ToList().Where(l => l.UserId == ownerId).Any())
                            tmp.Add(photo);
                    list = tmp;
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.LikesCount).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.LikesCount).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "ownerDontLike":
                    foreach (Photo photo in list)
                        if (!photo.Likes.ToList().Where(l => l.UserId == ownerId).Any())
                            tmp.Add(photo);
                    list = tmp;
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.LikesCount).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.LikesCount).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "ownerComments":
                    foreach (Photo photo in list)
                        if (photo.Comments.ToList().Where(l => l.OwnerId == ownerId).Any())
                            tmp.Add(photo);
                    list = tmp;
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.CommentsCount).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.CommentsCount).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                case "ownerNoComment":
                    foreach (Photo photo in list)
                        if (!photo.Comments.ToList().Where(l => l.OwnerId == ownerId).Any())
                            tmp.Add(photo);
                    list = tmp;
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.CommentsCount).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.CommentsCount).ThenByDescending(p => p.CreationDate).ToList();
                    break;
                default:
                    if ((bool)Session["Ascendant"])
                        list = list.OrderBy(p => p.CreationDate).ThenBy(p => p.CreationDate).ToList();
                    else
                        list = list.OrderByDescending(p => p.CreationDate).ThenByDescending(p => p.CreationDate).ToList();
                    break;
            }

            return list;
        }
        public ActionResult GetPhotos(bool forceRefresh = false)
        {
            if (DB.Photos.HasChanged || DB.Likes.HasChanged || DB.Users.HasChanged || forceRefresh)
            {
                return PartialView(SortPhotos());
            }
            return null;
        }
        public ActionResult List(string sortType = "")
        {
            Session["currentPhotoId"] = null;
            Session["IsOwner"] = null;
            if (Session["Ascendant"] == null) Session["Ascendant"] = false;
            if (Session["photoOwnerSearchId"] == null) Session["photoOwnerSearchId"] = 0;
            if (Session["searchKeywords"] == null) Session["searchKeywords"] = "";
            if (Session["PhotosSortType"] == null) Session["PhotosSortType"] = "dates";
            Session["PhotosSortType"] = sortType != "" ? sortType : Session["PhotosSortType"];
            if (Session["ShowSearch"] == null) Session["ShowSearch"] = false;
            Session["UsersList"] = DB.Users.ToList().OrderBy(u => u.Name).ToList();
            //DB.Events.Add("List");
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
            DB.Events.Add("Create", photo.Title);
            photo.OwnerId = ((User)Session["ConnectedUser"]).Id;
            photo.CreationDate = DateTime.Now;
            int photoId = DB.Photos.Add(photo);
            Session["PhotosSortType"] = "dates";
            Session["Ascendant"] = false;
            return RedirectToAction("Details/" + photoId);
        }
        public ActionResult Edit()
        {
            if (Session["currentPhotoId"] != null && Session["IsOwner"] != null && (bool)Session["IsOwner"])
            {
                int id = (int)Session["currentPhotoId"];
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
                DB.Events.Add("Edit", photo.Title);
                Photo storedPhoto = DB.Photos.Get((int)Session["currentPhotoId"]);
                photo.Id = storedPhoto.Id;
                photo.OwnerId = storedPhoto.OwnerId;
                photo.CreationDate = storedPhoto.CreationDate;
                DB.Photos.Update(photo);
                Session["PhotosSortType"] = "dates";
                return RedirectToAction("Details/" + photo.Id);
            }
            return Redirect(IllegalAccessUrl);
        }

        public ActionResult GetDetails(bool forceRefresh = false)
        {
            if (forceRefresh || true) //DB.Photos.HasChanged || DB.Users.HasChanged || DB.Comments.HasChanged || DB.Likes.HasChanged)
            {
                int photoId = Session["currentPhotoId"] != null ? (int)Session["currentPhotoId"] : 0;
                Photo photo = DB.Photos.Get(photoId);
                if (photo != null)
                    return PartialView(photo);
            }
            return null;
        }
        public ActionResult Details(int id)
        {
            Photo photo = DB.Photos.Get(id);
            if (photo != null)
            {
                DB.Events.Add("Details", photo.Title);
                Session["currentPhotoId"] = id;
                User connectedUser = ((User)Session["ConnectedUser"]);
                Session["IsOwner"] = connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id;
                if ((bool)Session["IsOwner"] || photo.Shared)
                    return View();
                else
                    return Redirect(IllegalAccessUrl);
            }
            return Redirect(IllegalAccessUrl);
        }
        public ActionResult Delete()
        {
            if (Session["IsOwner"] != null ? (bool)Session["IsOwner"] : false)
            {
                int id = (int)Session["currentPhotoId"];
                Photo photo = DB.Photos.Get(id);
                if (photo != null)
                {
                    DB.Events.Add("Delete", photo.Title);
                    User connectedUser = (User)Session["ConnectedUser"];
                    if (connectedUser.IsAdmin || photo.OwnerId == connectedUser.Id)
                    {
                        DB.Photos.Delete(id);
                        Session["PhotosSortType"] = "dates";
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
            Photo photo = DB.Photos.Get(id);
            photo.ResetCountsCalc();
            DB.Events.Add("TogglePhotoLike", photo.Title);
            return null;
        }
        public ActionResult ToggleCommentLike(int id)
        {
            User connectedUser = (User)Session["ConnectedUser"];
            DB.Likes.ToggleCommentLike(id, connectedUser.Id);
            Comment comment = DB.Comments.Get(id);
            if (comment != null)
                DB.Events.Add("ToggleCommentLike", comment.Photo.Title + " - " + comment.Text);
            return null;
        }

        public ActionResult Comments(int photoId, int parentId = 0)
        {
            List<Comment> comments = DB.Comments.ToList().Where(c => c.PhotoId == photoId && c.ParentId == parentId).ToList();
            return PartialView("RenderComments", comments);
        }
        public ActionResult GetComments(bool forceRefresh = false)
        {
            if (Session["currentPhotoId"] != null)
            {
                int photoId = (int)Session["currentPhotoId"];
                if (forceRefresh || true)
                {
                    List<Comment> comments = DB.Comments.ToList().Where(c => c.PhotoId == photoId && c.ParentId == 0).ToList();
                    return PartialView("RenderComments", comments);
                }
            }
            return null;
        }
        [HttpPost]
        public ActionResult CreateComment(int parentId, string commentText)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = new Comment();
            comment.ParentId = parentId;
            comment.PhotoId = (int)Session["currentPhotoId"];
            comment.OwnerId = connectedUser.Id;
            comment.Text = commentText;
            comment.CreationDate = DateTime.Now;
            DB.Comments.Add(comment);
            DB.Events.Add("CreateComment", comment.Photo.Title + " - " + commentText);
            return null;
        }
        [HttpPost]
        public ActionResult UpdateComment(int commentId, string commentText)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = DB.Comments.Get(commentId);
            if (comment != null && comment.Owner.Id == connectedUser.Id)
            {
                comment.Text = commentText;
                DB.Comments.Update(comment);
                DB.Events.Add("UpdateComment", comment.Photo.Title + " - " + comment.Text);
            }
            return null;
        }
        public ActionResult DeleteComment(int id)
        {
            User connectedUser = ((User)Session["ConnectedUser"]);
            Comment comment = DB.Comments.Get(id);
            if (comment != null && comment.Owner.Id == connectedUser.Id)
            {
                DB.Events.Add("DeleteComment", comment.Photo.Title + " - " + comment.Text);
                DB.Comments.BeginTransaction();
                DB.Comments.Delete(comment.Id);
                DB.Comments.EndTransaction();
            }
            return null;
        }
    }
}