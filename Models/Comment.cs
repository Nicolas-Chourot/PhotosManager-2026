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
        
        public int ParentId { get; set; }

        [JsonIgnore] public List<Comment> Comments => DB.Comments.ToList().Where(c => c.PhotoId == PhotoId && c.ParentId == Id).ToList();
        public int OwnerId { get; set; }
        
        [JsonIgnore] public User Owner => DB.Users.Get(OwnerId);
        public DateTime CreationDate { get; set; }
        
        public string Text { get; set; }

    }
}