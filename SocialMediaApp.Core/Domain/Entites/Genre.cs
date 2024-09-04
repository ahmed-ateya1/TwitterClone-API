using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class Genre
    {
        public Guid GenreID { get; set; }
        public string GenreName { get; set; }
        public ICollection<Tweet> Tweets { get; set; } = new List<Tweet>();
    }
}
