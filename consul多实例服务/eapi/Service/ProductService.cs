using eapi.Models;
using eapi.Repositories;

namespace eapi.Service
{
    public class ProductService : IProductService
    {
        private readonly IRepositoryWrapper repositoryWrapper;
        public ProductService(IRepositoryWrapper repositoryWrapper)
        {
            this.repositoryWrapper = repositoryWrapper;
        }
        public async Task Create(string sku, int count)
        {
           var check =await repositoryWrapper.ProductRepository.FindByCondition(x=>x.Sku.Equals(sku));
            if (check.Any())
            {
                throw new Exception($"{sku}已存在");
            }
           await repositoryWrapper.ProductRepository.Create(Product.Create(sku, count)); 
        }

        public async Task<IEnumerable<Product>> FindAll()
        {
            return await repositoryWrapper.ProductRepository.FindAll();
        }
    }
}
