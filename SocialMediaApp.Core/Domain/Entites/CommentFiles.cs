using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.Domain.Entites
{
    public class CommentFiles
    {
        public Guid CommentFileID { get; set; }
        public string FileUrl { get; set; }
        public Guid CommentID { get; set; }
        public Comment Comment { get; set; }
    }
}
