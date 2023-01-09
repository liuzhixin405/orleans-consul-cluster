using eapi.Data;
using eapi.Models;
using Microsoft.Identity.Client;

namespace eapi.Repositories
{
    public class ProductRepository : ReposioryBase<Product>
    {
        public ProductRepository(ProductDbContext dbContext) : base(dbContext)
        {
            
        }
    }
}
