using eapi.Data;
using eapi.interfaces.Models;
using Microsoft.Identity.Client;

namespace eapi.Repositories
{
    public class ProductRepository : ReposioryBase<Product>
    {
        public ProductRepository(DbFactory dbContext) : base(dbContext)
        {
            
        }
       
    }
}
