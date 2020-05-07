using System;

namespace Datingapp.API.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Descrioption { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public string PublicId { get; set; }
        public bool IsAproved { get; set; }
        public virtual User User { get; set; }
        public int UserId { get; set; }
    }
}