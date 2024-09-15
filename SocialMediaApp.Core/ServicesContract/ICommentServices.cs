using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.CommentDTO;
using SocialMediaApp.Core.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface ICommentServices
    {
        Task<CommentResponse> CreateAsync(CommentAddRequest? commentAddRequest);
        Task<CommentResponse> UpdateAsync(CommentUpdateRequest? commentUpdateRequest);
        Task<bool> DeleteAsync(Guid? commentID);
        Task<CommentResponse> GetByAsync(Expression<Func<Comment , bool>> filter , bool isTracked = true);
        Task<IEnumerable<CommentResponse>> GetAllAsync(Expression<Func<Comment, bool>>? filter, int pageIndex = 1 , int pageSize = 5);

    }
}
