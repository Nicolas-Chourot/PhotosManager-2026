using JSON_DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Comment : Record
    {
        public int PhotoId { get; set; }
        public int CommentId { get; set; }
        [JsonIgnore] public List<Comment> Comments => DB.Comments.ToList().Where(c => c.PhotoId == PhotoId && c.CommentId == Id).ToList();
        public int UserId { get; set; }
        [JsonIgnore] public User User => DB.Users.Get(UserId);
        public DateTime CreationDate { get; set; }
        public string Text { get; set; }

    }
}