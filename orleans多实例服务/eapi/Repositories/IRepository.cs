using eapi.Data;
using Microsoft.EntityFrameworkCore;
using ServiceStack;
using System.Linq.Expressions;

namespace eapi.Repositories
{
    public interface IRepository<T>
    {
        Task Create(T entity);
        Task Delete(T entity);
        Task<IQueryable<T>> FindAll();
        Task<IReadOnlyList<T>> FindByCondition(Expression<Func<T, bool>> expression);
        Task<T> GetById(int id);
        Task Update(T entity);
    }
    public abstract class ReposioryBase<T> : IRepository<T> where T : class
    {
        private  DbSet<T> _dbSet;
        private readonly DbFactory _dbFactory;
        protected DbSet<T> DbSet => _dbSet ?? (_dbSet = _dbFactory.DbContext.Set<T>());
        public ReposioryBase(DbFactory dbFactory)
        {
            this._dbFactory = dbFactory;
        }
        public async Task Create(T entity)
        {
            await DbSet.AddAsync(entity);
            //await dbContext.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            DbSet.Remove(entity);
            //await dbContext.SaveChangesAsync();
        }

        public async Task<IQueryable<T>> FindAll()
        {
            await Task.CompletedTask;
            return DbSet.AsNoTracking();
        }

        public async Task<IReadOnlyList<T>> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return await DbSet.Where(expression).ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public Task Update(T entity)
        {
            try
            {
                DbSet.Update(entity);
                return Task.CompletedTask;
                //await dbContext.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
