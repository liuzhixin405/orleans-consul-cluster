using eapi.Data;
using eapi.interfaces.Models;

namespace eapi.Repositories
{
    public class OrderRepository : ReposioryBase<Order>
    {
        public OrderRepository(DbFactory dbContext) : base(dbContext)
        {
        }
    }
}
