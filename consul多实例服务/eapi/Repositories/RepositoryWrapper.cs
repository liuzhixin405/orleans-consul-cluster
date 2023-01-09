using eapi.Data;
using eapi.Models;

namespace eapi.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private readonly ProductDbContext _;
        public RepositoryWrapper(ProductDbContext context)
        {
            _ = context;
        }
        //public ProductDbContext context => _;
        public async Task Trans(Func<Task> func)
        {
            var trans =await _.Database.BeginTransactionAsync();
            try
            {
               await func();
               await  trans.CommitAsync();
            }
            catch (Exception ex)
            {
                trans.Rollback();
            }
            finally
            {
                trans.Dispose();
            }
        }
        public IRepository<Order> OrderRepository => new OrderRepository(_);
        public IRepository<Product> ProductRepository => new ProductRepository(_);
    }
}
