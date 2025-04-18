using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PhotosManager.Models
{
    public class CommentsRepository : Repository<Comment>
    {
        public override bool Delete(int Id)
        {
            List<Comment> responses = ToList().Where(c=>c.ParentId == Id).ToList();
            foreach (Comment c in responses)
            {
                Delete(c.Id);
            }
            return base.Delete(Id);
        }

        public void DeleteByPhotoId(int Id)
        {
            List<Comment> responses = ToList().Where(c => c.PhotoId == Id).ToList();
            foreach (Comment c in responses)
            {
                Delete(c.Id);
            }
        }
        public void DeleteByUserId(int Id)
        {
            List<Comment> responses = ToList().Where(c => c.OwnerId == Id).ToList();
            foreach (Comment c in responses)
            {
                Delete(c.Id);
            }
        }
    }
}