using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.GenreDTO;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Core.ServicesContract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SocialMediaApp.Core.Helper;

namespace SocialMediaApp.Core.Services
{
    public class GenreServces : IGenreServces
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenreRepository _genreRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GenreServces> _logger;

        public GenreServces(IUnitOfWork unitOfWork, IGenreRepository genreRepository, IMapper mapper, ILogger<GenreServces> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _genreRepository = genreRepository;
            _logger = logger;
        }

        public async Task<GenreResponse> CreateAsync(GenreAddRequest? genreAdd)
        {
            _logger.LogInformation("Creating new genre...");

            if (genreAdd == null)
            {
                _logger.LogError("GenreAddRequest is null.");
                throw new ArgumentNullException(nameof(genreAdd));
            }

            if (string.IsNullOrEmpty(genreAdd.GenreName))
            {
                _logger.LogError("GenreName is missing.");
                throw new ArgumentNullException(nameof(genreAdd.GenreName));
            }

            if (await _unitOfWork.Repository<Genre>().GetByAsync(x => x.GenreName == genreAdd.GenreName) != null)
            {
                _logger.LogError("Genre already exists: {GenreName}", genreAdd.GenreName);
                throw new Exception("Genre already exists.");
            }

            ValidationHelper.ValidateModel(genreAdd);

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var genre = _mapper.Map<Genre>(genreAdd);
                    genre.GenreID = Guid.NewGuid();
                    await _unitOfWork.Repository<Genre>().CreateAsync(genre);
                    await _unitOfWork.CompleteAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Genre created successfully: {GenreName}", genreAdd.GenreName);
                    return _mapper.Map<GenreResponse>(genre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating genre.");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteAsync(Guid? id)
        {
            _logger.LogInformation("Deleting genre with ID: {GenreID}", id);

            var genre = await _unitOfWork.Repository<Genre>().GetByAsync(x => x.GenreID == id);

            if (genre == null)
            {
                _logger.LogError("Genre not found for ID: {GenreID}", id);
                throw new Exception("Genre not found");
            }

            var result = await _unitOfWork.Repository<Genre>().DeleteAsync(genre);

            if (result)
            {
                _logger.LogInformation("Genre deleted successfully: {GenreID}", id);
            }
            else
            {
                _logger.LogWarning("Failed to delete genre: {GenreID}", id);
            }

            return result;
        }

        public async Task<IEnumerable<GenreResponse>> GetAllAsync(int pageIndex = 1, int pageSize = 10)
        {
            _logger.LogInformation("Fetching all genres...");

            var genres = await _unitOfWork.Repository<Genre>().GetAllAsync(null,"",null,pageIndex,pageSize);
            return _mapper.Map<IEnumerable<GenreResponse>>(genres);
        }

        public async Task<GenreResponse> GetByAsync(Expression<Func<Genre, bool>> predicate, bool IsTracked = true)
        {
            _logger.LogInformation("Fetching genre with predicate: {Predicate}", predicate);

            var genre = await _unitOfWork.Repository<Genre>().GetByAsync(predicate, IsTracked);

            if (genre == null)
            {
                _logger.LogError("Genre not found with provided predicate.");
                throw new Exception("Genre not found");
            }

            return _mapper.Map<GenreResponse>(genre);
        }

        public async Task<GenreResponse> UpdateAsync(GenreUpdateRequest? genreUpdate)
        {
            _logger.LogInformation("Updating genre...");

            if (genreUpdate == null)
            {
                _logger.LogError("GenreUpdateRequest is null.");
                throw new ArgumentNullException(nameof(genreUpdate));
            }

            if (string.IsNullOrEmpty(genreUpdate.GenreName))
            {
                _logger.LogError("GenreName is missing in the update request.");
                throw new ArgumentNullException(nameof(genreUpdate.GenreName));
            }
            ValidationHelper.ValidateModel(genreUpdate);


            var existingGenre = await _unitOfWork.Repository<Genre>().GetByAsync(x => x.GenreName == genreUpdate.GenreName);
            if (existingGenre != null)
            {
                _logger.LogError("Genre name already exists: {GenreName}", genreUpdate.GenreName);
                throw new Exception("Genre already exists");
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var genre = _mapper.Map<Genre>(genreUpdate);
                    var genreResult = await _genreRepository.UpdateAsync(genre);
                    if (genreResult == null)
                    {
                        throw new Exception("Failed to update genre.");
                    }

                    await _unitOfWork.CompleteAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Genre updated successfully: {GenreID}", genre.GenreID);
                    return _mapper.Map<GenreResponse>(genre);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating genre.");
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}
