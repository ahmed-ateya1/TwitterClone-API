using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class CommentRepository : GenericRepository<Comment>, ICommentRepository
    {
        private readonly ApplicationDbContext _db;
        public CommentRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Comment> UpdateAsync(Comment comment)
        {
            var commentToUpdate =  await _db.Comments.FirstOrDefaultAsync(x => x.CommentID == comment.CommentID);
            if (commentToUpdate == null)
                throw new ArgumentNullException("Comment not found");

            _db.Entry(commentToUpdate).CurrentValues.SetValues(comment);
            commentToUpdate.UpdatedAt = DateTime.UtcNow;
            commentToUpdate.IsUpdated = true;
            await SaveAsync();
            return commentToUpdate;
        }
    }
}
