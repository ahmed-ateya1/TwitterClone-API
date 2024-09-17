using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface IMailingService 
    {
        Task SendMessageAsync(string mailTo, string subject, string body, IList<IFormFile>? attach);
    }
}
