using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class TweetFiles
    {
        public Guid TweetFilesID { get; set; }
        public string FileURL { get; set; }
        public Guid TweetID { get; set; }
        public Tweet Tweet { get; set; }
    }
}
