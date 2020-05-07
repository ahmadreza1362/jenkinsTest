using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datingapp.API.Dtos
{
    public class PhotosForDetailedDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Descrioption { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public bool IsAproved { get; set; }
    }
}
