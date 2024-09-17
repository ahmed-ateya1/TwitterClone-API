using Microsoft.AspNetCore.Http;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.FilesTweetDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface ITweetFilesServices 
    {
        Task<IEnumerable<TweetFiles>> SaveTweetFileAsync(FileTweetAddRequest? fileTweetAdd);
        Task<bool> DeleteTweetFileAsync(IEnumerable<TweetFiles> files);
    }
}
