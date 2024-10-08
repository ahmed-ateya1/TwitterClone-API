﻿using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.Repositories
{
    public class TweetFilesRepository : GenericRepository<TweetFiles>, ITweetFilesRepository
    {
        public TweetFilesRepository(ApplicationDbContext db) : base(db)
        {
        }
    }
}
