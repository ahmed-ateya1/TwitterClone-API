using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp.Core.DTO;
using SocialMediaApp.Core.DTO.GenreDTO;
using SocialMediaApp.Core.Enumeration;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.ServicesContract;
using System.Net;

namespace SocialMediaApp.API.Controllers
{
    /// <summary>
    /// Controller for managing genres.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreServces _genreServices;
        private readonly ILogger<GenreController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenreController"/> class.
        /// </summary>
        /// <param name="genreServices">The genre services.</param>
        /// <param name="logger">The logger.</param>
        public GenreController(IGenreServces genreServices, ILogger<GenreController> logger)
        {
            _genreServices = genreServices;
            _logger = logger;
        }

        /// <summary>
        /// Gets the genres.
        /// </summary>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>The list of genres.</returns>
        [HttpGet("getGenres")]
        public async Task<ActionResult<ApiResponse>> GetGenres(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var genres = await _genreServices.GetAllAsync();
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    Messages = "Genres are fetched successfully",
                    Result = genres,
                    StatusCode = HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetGenres method: An error occurred while fetched Genres");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while fetched Genres"
                });
            }
        }

        /// <summary>
        /// Gets a genre by ID.
        /// </summary>
        /// <param name="id">The ID of the genre.</param>
        /// <returns>The genre.</returns>
        [HttpGet("getGenre/{id}")]
        public async Task<ActionResult<ApiResponse>> GetGenre(Guid id)
        {
            try
            {
                var genre = await _genreServices.GetByAsync(x => x.GenreID == id);
                return Ok(new ApiResponse
                {
                    IsSuccess = true,
                    Messages = "Genre fetched successfully",
                    Result = genre,
                    StatusCode = HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetGenre method: An error occurred while fetched Genre");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while fetched Genre"
                });
            }

        }

        /// <summary>
        /// Creates a new genre.
        /// </summary>
        /// <param name="genreAdd">The genre add request.</param>
        /// <returns>The created genre.</returns>
        [HttpPost("createGenre")]
        public async Task<ActionResult<ApiResponse>> CreateGenre([FromForm] GenreAddRequest genreAdd)
        {
            try
            {
                var genre = await _genreServices.CreateAsync(genreAdd);
                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Messages = "Genre created successfully",
                    Result = genre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateGenre method: An error occurred while creating Genre");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while creating Genre"
                });
            }
        }

        /// <summary>
        /// Updates an existing genre.
        /// </summary>
        /// <param name="genreUpdate">The genre update request.</param>
        /// <returns>The updated genre.</returns>
        [HttpPut("updateGenre")]
        public async Task<ActionResult<ApiResponse>> UpdateGenre([FromForm] GenreUpdateRequest genreUpdate)
        {
            try
            {
                var genre = await _genreServices.UpdateAsync(genreUpdate);
                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Messages = "Genre updated successfully",
                    Result = genre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateGenre method: An error occurred while updating Genre");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while updating Genre"
                });
            }
        }

        /// <summary>
        /// Deletes a genre by ID.
        /// </summary>
        /// <param name="id">The ID of the genre to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        [HttpDelete("deleteGenre/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteGenre(Guid id)
        {
            try
            {
                var result = await _genreServices.DeleteAsync(id);
                return Ok(new ApiResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Messages = "Genre deleted successfully",
                    Result = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteGenre method: An error occurred while deleting Genre");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Messages = "An error occurred while deleting Genre"
                });
            }
        }
    }
}
