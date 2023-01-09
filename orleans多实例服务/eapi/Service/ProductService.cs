using eapi.Data;
using eapi.interfaces.Models;
using eapi.Repositories;

namespace eapi.Service
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> repositoryWrapper;
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IRepository<Product> repositoryWrapper,IUnitOfWork unitOfWork)
        {
            this.repositoryWrapper = repositoryWrapper;
            this._unitOfWork = unitOfWork;
        }
        public async Task Create(string sku, int count)
        {
           var check =await repositoryWrapper.FindByCondition(x=>x.Sku.Equals(sku));
            if (check.Any())
            {
                throw new Exception($"{sku}已存在");
            }
           await repositoryWrapper.Create(Product.Create(sku, count)); 
            await _unitOfWork.CommitAsync();
        }

        public async Task<IEnumerable<Product>> FindAll()
        {
            return await repositoryWrapper.FindAll();
        }
    }
}
