using AutoMapper;
using SocialMediaApp.Core.DTO.ProfileDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    public interface IProfileServices
    {
        /// <summary>
        /// Creates a new profile asynchronously.
        /// </summary>
        /// <param name="profileAddRequest">The profile add request.</param>
        /// <returns>The created profile response.</returns>
        Task<ProfileResponse> CreateAsync(ProfileAddRequest? profileAddRequest);

        /// <summary>
        /// Updates an existing profile asynchronously.
        /// </summary>
        /// <param name="profileUpdateRequest">The profile update request.</param>
        /// <returns>The updated profile response.</returns>
        Task<ProfileResponse> UpdateAsync(ProfileUpdateRequest? profileUpdateRequest);

        /// <summary>
        /// Deletes a profile asynchronously.
        /// </summary>
        /// <param name="id">The ID of the profile to delete.</param>
        /// <returns>A boolean indicating whether the profile was deleted successfully.</returns>
        Task<bool> DeleteAsync(Guid? id);

        /// <summary>
        /// Retrieves a profile asynchronously based on the specified expression.
        /// </summary>
        /// <param name="expression">The expression used to filter the profiles.</param>
        /// <param name="IsTracked">A flag indicating whether to track the profile in the context.</param>
        /// <returns>The retrieved profile response.</returns>
        Task<ProfileResponse> GetProfileByAsync(Expression<Func<SocialMediaApp.Core.Domain.Entites.Profile, bool>> expression, bool IsTracked = false);

        Task<IEnumerable<ProfileResponse>> GetAllAsync(int pageIndex = 1, int pageSize = 10);
    }
}
