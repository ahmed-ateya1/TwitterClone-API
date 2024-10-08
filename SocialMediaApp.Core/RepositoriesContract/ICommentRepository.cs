﻿using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.ServicesContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Core.RepositoriesContract
{
    public interface ICommentRepository : IGenericRepository<Comment>
    {
        Task<Comment> UpdateAsync(Comment comment);
    }
}
