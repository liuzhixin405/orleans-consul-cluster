using eapi.Data;
using eapi.Models;

namespace eapi.Repositories
{
    public class OrderRepository : ReposioryBase<Order>
    {
        public OrderRepository(ProductDbContext dbContext) : base(dbContext)
        {
        }
    }
}
