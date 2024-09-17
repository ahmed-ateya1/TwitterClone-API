using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.GenreDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.ServicesContract
{
    /// <summary>
    /// Represents the contract for genre services.
    /// </summary>
    public interface IGenreServces 
    {
        /// <summary>
        /// Retrieves all genres asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of genre responses.</returns>
        Task<IEnumerable<GenreResponse>> GetAllAsync(int pageIndex = 1, int pageSize = 10);

        /// <summary>
        /// Retrieves a genre asynchronously based on the specified predicate.
        /// </summary>
        /// <param name="predict">The predicate used to filter the genres.</param>
        /// <param name="IsTracked">Indicates whether the genre should be tracked by the context.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the genre response.</returns>
        Task<GenreResponse> GetByAsync(Expression<Func<Genre, bool>> predict, bool IsTracked = true);

        /// <summary>
        /// Creates a new genre asynchronously.
        /// </summary>
        /// <param name="genreAdd">The genre add request containing the genre details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the created genre response.</returns>
        Task<GenreResponse> CreateAsync(GenreAddRequest? genreAdd);

        /// <summary>
        /// Updates an existing genre asynchronously.
        /// </summary>
        /// <param name="genreUpdate">The genre update request containing the updated genre details.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated genre response.</returns>
        Task<GenreResponse> UpdateAsync(GenreUpdateRequest? genreUpdate);

        /// <summary>
        /// Deletes a genre asynchronously based on the specified genre ID.
        /// </summary>
        /// <param name="id">The ID of the genre to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the genre was successfully deleted.</returns>
        Task<bool> DeleteAsync(Guid? id);
    }
}