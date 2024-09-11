using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.RepositoriesContract;
using SocialMediaApp.Infrastructure.Data;
using SocialMediaApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaApp.Infrastructure.UnitOfWorkConfig
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }
        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return _repositories[typeof(T)] as IGenericRepository<T>;
            }
            var repository = new GenericRepository<T>(_db);
            _repositories.Add(typeof(T), repository);

            return repository;
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _db.Database.CommitTransactionAsync();
        }

        public async Task<int> CompleteAsync()
        {
            return await _db.SaveChangesAsync();
        }
        public async Task RollbackTransactionAsync()
        {
            await _db.Database.RollbackTransactionAsync();
        }
        public void Dispose()
        {
            _db.Dispose();
        }

    }
}
