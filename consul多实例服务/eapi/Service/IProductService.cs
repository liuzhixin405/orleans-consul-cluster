using eapi.Models;

namespace eapi.Service
{
    public interface IProductService
    {
        Task Create(string sku, int count);
        Task<IEnumerable<Product>> FindAll();
    }
}
