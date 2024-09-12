using AutoMapper;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class GenreRepository : GenericRepository<Genre>, IGenreRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public GenreRepository(ApplicationDbContext db, IMapper mapper) : base(db)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Genre> UpdateAsync(Genre genre)
        {
            var genreToUpdate = await _db.Genres.FirstOrDefaultAsync(x => x.GenreID == genre.GenreID);
            if (genreToUpdate != null)
            {
                _mapper.Map(genre, genreToUpdate);
                _db.Genres.Update(genreToUpdate); 
                await SaveAsync(); 
            }

            return genreToUpdate;
        }
    }
}
