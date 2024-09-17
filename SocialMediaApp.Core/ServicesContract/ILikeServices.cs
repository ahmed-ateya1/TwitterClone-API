using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.LikeDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface ILikeServices
    {
        Task<LikeResponse> LikeAsync(Guid? id);
        Task<bool> UnLikeAsync(Guid? id);
        Task<bool> IsLikeToAsync(Guid? id);
        Task<LikeResponse> GetByAsync(Expression<Func<Like, bool>> predicate, bool isTracked = true);
        Task<IEnumerable<LikeResponse>> GetAllAsync(Expression<Func<Like, bool>>? predicate , int pageIndex = 1, int pageSize = 10);
    }
}
