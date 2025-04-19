using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace PhotosManager.Models
{
    public class LikesRepository : Repository<Like>
    {

        public void ToggleLike(int photoId, int userId)
        {
            Like like = ToList().Where(l => (l.PhotoId == photoId && l.UserId == userId)).FirstOrDefault();
            
            if (like != null)
            {
                DB.Notifications.Push(like.Photo.OwnerId, like.User.Name + " n'aime plus votre photo \n[" + like.Photo.Title + "]");
                Delete(like.Id);
            }
            else
            {
                like = new Like { PhotoId = photoId, UserId = userId };
                DB.Notifications.Push(like.Photo.OwnerId, like.User.Name + " aime votre photo \n[" + like.Photo.Title + "]");
                Add(like);
            }
        }
        public void ToggleCommentLike(int commentId, int userId)
        {
            Like like = ToList().Where(l => (l.CommentId == commentId && l.UserId == userId)).FirstOrDefault()?.Copy(); 
            string text = "";
            if (like != null)
            {
                text = like.Comment.Text;
                if (text.Length > 32)
                    text = text.Substring(0,32) + "...";
                byte[] bytes = Encoding.Default.GetBytes(text);

                DB.Notifications.Push(like.Comment.OwnerId, like.User.Name + " n'aime plus votre commentaire \n[" + text + "]");
                Delete(like.Id);
            }
            else
            {
                like = new Like { CommentId = commentId, UserId = userId };
                text = like.Comment.Text;
                if (text.Length > 32)
                    text = text.Substring(0,32) + "...";
                byte[] bytes = Encoding.Default.GetBytes(text);

                DB.Notifications.Push(like.Comment.OwnerId, like.User.Name + " aime votre commentaire \n [" + text + "]");
                Add(like);
            }
        }
        public void DeleteByPhotoId(int photoId)
        {
            List<Like> list = ToList().Where(l => l.PhotoId == photoId).ToList();
            list.ForEach(l => Delete(l.Id));
        }
        public void DeleteByUserId(int userId)
        {
            List<Like> list = ToList().Where(l => l.UserId == userId).ToList();
            list.ForEach(l => Delete(l.Id));
        }
    }
}