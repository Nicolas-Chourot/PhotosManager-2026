using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PhotoId { get; set; }
        public int CommentId { get; set; }
        public DateTime CreationDate { get; set; }

        public Like()
        {
            Id = 0;
            CommentId = 0;
            CreationDate = DateTime.Now;
        }
        [JsonIgnore]
        public User User => DB.Users.Get(UserId);
        [JsonIgnore]
        public Photo Photo => DB.Photos.Get(PhotoId);
        [JsonIgnore]
        public Comment Comment => DB.Comments.Get(CommentId);
    }
}