namespace eapi.Data
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
    }
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbFactory _dbFactory;
        public UnitOfWork(DbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task<int> CommitAsync()
        {
            return await _dbFactory.DbContext.SaveChangesAsync();
        }
    }
}
