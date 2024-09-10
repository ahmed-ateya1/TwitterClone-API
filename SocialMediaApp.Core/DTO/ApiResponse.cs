using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.DTO
{
    /// <summary>
    /// Represents the API response.
    /// </summary>
    public class ApiResponse
    {
        public HttpStatusCode StatusCode;

        public bool IsSuccess = true;

        public string Messages { get; set; } 

        public object Result { get; set; }
    }
}
